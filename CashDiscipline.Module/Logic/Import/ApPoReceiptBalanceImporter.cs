using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp.Xpo;
using LumenWorks.Framework.IO.Csv;
using SmartFormat;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Import
{
    public class ApPoReceiptBalanceImporter
    {
        public ApPoReceiptBalanceImporter(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }

        #region Constants
        private string createSql
        {
            get
            {
                return @"IF OBJECT_ID('tempdb..#TmpApPoReceiptBal') IS NOT NULL DROP TABLE #TmpApPoReceiptBal
CREATE TABLE #TmpApPoReceiptBal (
[PO Number] int,
[PO Release] nvarchar(255),
[PO Line] int,
[PO Shipment] int,
[PO Distribution] int,
[PO Distribution ID] int,
[PO Balance] float,
[AP Balance] float,
[Write Off Balance] float,
[Total Balance] float,
[Age In Days] int,
[Item] nvarchar(255),
[Vendor] nvarchar(255),
[Destination] nvarchar(255),
[Opex/Capex/Other] nvarchar(255),
[Extra Detail1] nvarchar(255),
[Extra Detail2] nvarchar(255),
[Company] nvarchar(10),
[Account] nvarchar(10),
[Cost Centre] nvarchar(10),
[Product] nvarchar(10),
[Saleschannel] nvarchar(10),
[Country] nvarchar(255),
[Interco] nvarchar(10),
[Project] nvarchar(10),
[Location] nvarchar(255)
)";
            }
        }

        private string persistSql
        {
            get
            {
                return
@"DELETE FROM VHAFinance.dbo.ApAccrualPo
WHERE [AsAtDate] = '{AsAtDate}'

INSERT INTO VHAFinance.dbo.ApAccrualPo
(
    [Oid],
    [AsAtDate],
    [PoNumber],
    [PoRelease],
    [PoLine],
    [PoShipment],
    [PoDistribution],
    [PoDistributionId],
    [PoBalance],
    [ApBalance],
    [WriteOffBalance],
    [TotalBalance],
    [AgeInDays],
    [Item],
    [Vendor],
    [Destination],
    [Opex_Capex_Other],
    [ExtraDetail1],
    [ExtraDetail2],
    [Company],
    [Account],
    [CostCentre],
    [Product],
    [Saleschannel],
    [Country],
    [Interco],
    [Project],
    [Location]
)
SELECT
    NEWID() AS Oid,
    '{AsAtDate}' AS AsAtDate,
    [PO Number],
    [PO Release],
    [PO Line],
    [PO Shipment],
    [PO Distribution],
    [PO Distribution ID],
    [PO Balance],
    [AP Balance],
    [Write Off Balance],
    [Total Balance],
    [Age In Days],
    [Item],
    [Vendor],
    [Destination],
    [Opex/Capex/Other],
    [Extra Detail1],
    [Extra Detail2],
    [Company],
    [Account],
    [Cost Centre],
    [Product],
    [Saleschannel],
    [Country],
    [Interco],
    [Project],
    [Location]
FROM #TmpApPoReceiptBal
";
            }
        }

        #endregion

        private XPObjectSpace objSpace;

        public string Execute(ImportApPoReceiptBalanceParam paramObj)
        {
            var inputFilePath = paramObj.FilePath;

            Func<string, string> formatSql = delegate (string sql)
            {
                return Smart.Format(sql, new
                {
                    AsAtDate = string.Format("{0:yyyy-MM-dd}", paramObj.AsAtDate.Date),
                });
            };

            var statusMessage = string.Empty;

            var conn = (SqlConnection)objSpace.Connection;

            int rowCount = 0;

            using (var csv = new CachedCsvReader(new StreamReader(inputFilePath), true, '\t'))
            using (var cmd = conn.CreateCommand())
            using (var bc = new SqlBulkCopy(conn))
            {
                cmd.CommandTimeout = CashDiscipline.Common.Constants.SqlCommandTimeout;

                cmd.CommandText = createSql;
                cmd.ExecuteNonQuery();

                bc.DestinationTableName = "#TmpApPoReceiptBal";
                bc.WriteToServer(csv);

                cmd.CommandText = "SELECT COUNT(*) FROM #TmpApPoReceiptBal";
                rowCount = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.CommandText = formatSql(persistSql);
                cmd.ExecuteNonQuery();
            }
            statusMessage = string.Format("{0} rows processed.", rowCount);
            return statusMessage;
        }
    }
}
