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

            char delimiter = ParseDelimiter(paramObj.Delimiter ?? ",");
            using (var csv = new CachedCsvReader(new StreamReader(inputFilePath), true, delimiter))
            using (var cmd = conn.CreateCommand())
            using (var bc = new SqlBulkCopy(conn))
            {
                cmd.CommandTimeout = CashDiscipline.Common.Constants.SqlCommandTimeout;

                cmd.CommandText = createSql;
                cmd.ExecuteNonQuery();

                bc.DestinationTableName = "#TmpApPoReceiptBal";
                AddColumnMappings(bc);
                bc.WriteToServer(csv);

                cmd.CommandText = "SELECT COUNT(*) FROM #TmpApPoReceiptBal";
                rowCount = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.CommandText = formatSql(persistSql);
                cmd.ExecuteNonQuery();
            }
            statusMessage = string.Format("{0} rows processed.", rowCount);
            return statusMessage;
        }

        private void AddColumnMappings(SqlBulkCopy bc)
        {

            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "PO Number", DestinationColumn = "PO Number" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "PO Release", DestinationColumn = "PO Release" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "PO Line", DestinationColumn = "PO Line" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "PO Shipment", DestinationColumn = "PO Shipment" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "PO Distribution", DestinationColumn = "PO Distribution" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "PO Distribution ID", DestinationColumn = "PO Distribution ID" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "PO Balance", DestinationColumn = "PO Balance" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "AP Balance", DestinationColumn = "AP Balance" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Write Off Balance", DestinationColumn = "Write Off Balance" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Total Balance", DestinationColumn = "Total Balance" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Age In Days", DestinationColumn = "Age In Days" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Item", DestinationColumn = "Item" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Vendor", DestinationColumn = "Vendor" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Destination", DestinationColumn = "Destination" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Opex/Capex/Other", DestinationColumn = "Opex/Capex/Other" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Extra Detail1", DestinationColumn = "Extra Detail1" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Extra Detail2", DestinationColumn = "Extra Detail2" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Company", DestinationColumn = "Company" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Account", DestinationColumn = "Account" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Cost Centre", DestinationColumn = "Cost Centre" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Product", DestinationColumn = "Product" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Saleschannel", DestinationColumn = "Saleschannel" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Country", DestinationColumn = "Country" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Interco", DestinationColumn = "Interco" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Project", DestinationColumn = "Project" });
            bc.ColumnMappings.Add(new SqlBulkCopyColumnMapping() { SourceColumn = "Location", DestinationColumn = "Location" });
        }

        private char ParseDelimiter(string delimiter)
        {
            char result = ',';
            switch (delimiter)
            {
                case "\t":
                    result = '\t';
                    break;
                default:
                    result = Convert.ToChar(delimiter.Substring(0, 1));
                    break;
            }
            return result;
        }
    }
}
