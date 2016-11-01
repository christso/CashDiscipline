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
    public class ApPoReceiptImporter
    {
        public ApPoReceiptImporter(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }


        #region Constants
        private string createSql
        {
            get
            {
                return @"IF OBJECT_ID('tempdb..#TmpApPoReceipt') IS NOT NULL DROP TABLE #TmpApPoReceipt
CREATE TABLE #TmpApPoReceipt (
[Po Num] int,
[Receipt Num] int,
[Quantity Received] float,
[Received By] nvarchar(255),
[Req Num] int,
[Requester Name] nvarchar(255),
[Vendor Name] nvarchar(255),
[Vendor Num] nvarchar(255),
[Vendor Site Code] nvarchar(255),
[Line Num] int,
[Distribution Num] int,
[Transaction Type] nvarchar(255),
[Item Description] nvarchar(255),
[Line Unit Price Orig Cur] float,
[Unit Price AUD] float,
[Major Category] nvarchar(255),
[Minor Category] nvarchar(255),
[Quantity Ordered] float,
[Quantity Delivered] float,
[Quantity Billed] float,
[Distr Amt Ordered ORIG CUR] float,
[Distr Amt Delivered ORIG CUR] float,
[Distr Amt Billed ORIG CUR] float,
[Company] nvarchar(10),
[Account] nvarchar(10),
[Cost Centre] nvarchar(10),
[Po Header Closed Code] nvarchar(255),
[Line Closed Code] nvarchar(255),
[Shipment Closed Code] nvarchar(255),
[Receipt Date] date,
[Invoice From Receipt] nvarchar(255),
[Docket From Receipt] nvarchar(255),
[Comments From Receipt] nvarchar(255),
[Project Number] nvarchar(255),
[Project Type] nvarchar(255),
[Project Status Code] nvarchar(255),
[Rate] nvarchar(255),
[Po Creation Date] date,
[Project Manager] nvarchar(255)
)";
            }
        }

        private string persistSql
        {
            get
            {
                return
@"DELETE FROM VHAFinance.dbo.ApPoReceipt
WHERE [Receipt Date] BETWEEN '{FromDate}' AND '{ToDate}'

INSERT INTO VHAFinance.dbo.ApPoReceipt
(
    [Oid],
    [Po Num],
    [Receipt Num],
    [Quantity Received],
    [Received By],
    [Req Num],
    [Requester Name],
    [Vendor Name],
    [Vendor Num],
    [Vendor Site Code],
    [Transaction Type],
    [Item Description],
    [Line Unit Price ORIG CUR],
    [Unit Price AUD],
    [Major Category],
    [Minor Category],
    [Quantity Ordered],
    [Quantity Delivered],
    [Quantity Billed],
    [Distr Amt Ordered Orig Cur],
    [Distr Amt Delivered ORIG CUR],
    [Distr Amt Billed ORIG CUR],
    [Company],
    [Account],
    [Cost Centre],
    [Po Header Closed Code],
    [Line Closed Code],
    [Shipment Closed Code],
    [Receipt Date],
    [Invoice From Receipt],
    [Comments From Receipt],
    [Project Number],
    [Project Type],
    [Project Status Code],
    [Rate],
    [Po Creation Date],
    [Project Manager]
)
SELECT
    NEWID() AS Oid,
    [Po Num],
    [Receipt Num],
    [Quantity Received],
    [Received By],
    [Req Num],
    [Requester Name],
    [Vendor Name],
    [Vendor Num],
    [Vendor Site Code],
    [Transaction Type],
    [Item Description],
    [Line Unit Price Orig Cur],
    [Unit Price AUD],
    [Major Category],
    [Minor Category],
    [Quantity Ordered],
    [Quantity Delivered],
    [Quantity Billed],
    [Distr Amt Ordered ORIG CUR],
    [Distr Amt Delivered ORIG CUR],
    [Distr Amt Billed ORIG CUR],
    [Company],
    [Account],
    [Cost Centre],
    [Po Header Closed Code],
    [Line Closed Code],
    [Shipment Closed Code],
    [Receipt Date],
    [Invoice From Receipt],
    [Comments From Receipt],
    TRY_CAST([Project Number] AS int),
    [Project Type],
    [Project Status Code],
    TRY_CAST([Rate] AS float),
    [Po Creation Date],
    [Project Manager]
FROM #TmpApPoReceipt
";
            }
        }

        #endregion

        private XPObjectSpace objSpace;

        public string Execute(ImportApPoReceiptParam paramObj)
        {
            var statusMessage = string.Empty;

            Func<string, string> formatSql = delegate (string sql)
            {
                return Smart.Format(sql, new
                {
                    FromDate = string.Format("{0:yyyy-MM-dd}", paramObj.FromDate.Date),
                    ToDate = string.Format("{0:yyyy-MM-dd}", paramObj.ToDate.Date),
                });
            };

            var conn = (SqlConnection)objSpace.Connection;

            int rowCount = 0;

            using (var csv = new CachedCsvReader(new StreamReader(paramObj.FilePath), true))
            using (var cmd = conn.CreateCommand())
            using (var bc = new SqlBulkCopy(conn))
            {
                //cmd.Transaction = trn;
                cmd.CommandTimeout = CashDiscipline.Common.Constants.SqlCommandTimeout;

                cmd.CommandText = createSql;
                cmd.ExecuteNonQuery();

                bc.DestinationTableName = "#TmpApPoReceipt";
                bc.WriteToServer(csv);

                cmd.CommandText = "SELECT COUNT(*) FROM #TmpApPoReceipt";
                rowCount = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.CommandText = formatSql(persistSql);
                cmd.ExecuteNonQuery();

            }
            statusMessage = string.Format("{0} rows processed.", rowCount);
            return statusMessage;
        }
    }
}
