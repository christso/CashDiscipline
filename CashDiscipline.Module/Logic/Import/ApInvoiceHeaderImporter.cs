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
    public class ApInvoiceHeaderImporter
    {
        public ApInvoiceHeaderImporter(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }

        #region Constants
        private string createSql
        {
            get
            {
                return @"IF OBJECT_ID('tempdb..#TmpApInvoiceHdd') IS NOT NULL DROP TABLE #TmpApInvoiceHdd
CREATE TABLE #TmpApInvoiceHdd (
[Supplier Number] nvarchar(255),
[Vendor Name] nvarchar(255),
[Vendor Site Code] nvarchar(255),
[Invoice Date] nvarchar(255),
[Invoice Num] nvarchar(255),
[Po Number] nvarchar(255),
[Invoice Amount (Orig Curr)] float,
[Invoice Currency Code] nvarchar(255),
[AUD Invoice Amount] float,
[GST Amount] float,
[AUD Invoice NET] float,
[Description] nvarchar(255),
[Inv Payment Term] nvarchar(255),
[Due Date] nvarchar(255),
[Amount Paid (Orig Curr)] float,
[Invoice Received Date] nvarchar(255),
[Invoice Creation Date] nvarchar(255),
[Invoice Type Lookup Code] nvarchar(255),
[Pay Group Lookup Code] nvarchar(255),
[Inv Source] nvarchar(255),
[Payment Date] nvarchar(255),
[Status] nvarchar(255),
[Payment Request] nvarchar(255),
[Project Number] nvarchar(255),
[Project Name] nvarchar(255),
[Query Raised With] nvarchar(255),
[Pending Reason] nvarchar(255),
[Accounting Date] nvarchar(255),
[Local Currency Exchange Rate] float,
[Posting Status] nvarchar(255),
[Payment Method Code] nvarchar(255)
)";
            }
        }

        private string persistSql
        {
            get
            {
                return
@"DELETE FROM VHAFinance.dbo.ApInvoiceHeader
WHERE [Invoice Date] BETWEEN '{FromDate}' AND '{ToDate}'

INSERT INTO VHAFinance.dbo.ApInvoiceHeader
(
    [Oid],
    [Supplier Number],
    [Vendor Name],
    [Vendor Site Code],
    [Invoice Date],
    [Invoice Num],
    [Po Number],
    [Invoice Amount],
    [Invoice Currency Code],
    [Func Amount],
    [Tax Amount],
    [Description],
    [Inv Payment Term],
    [Due Date],
    [Amount Paid],
    [Invoice Received Date],
    [Invoice Creation Date],
    [Invoice Type Lookup Code],
    [Pay Group Lookup Code],
    [Inv Source],
    [Payment Date],
    [Status],
    [Payment Request],
    [Project Number],
    [Project Name]
)
SELECT
    NEWID() AS Oid,
    [Supplier Number],
    [Vendor Name],
    [Vendor Site Code],
    TRY_CAST([Invoice Date] AS date) AS [Invoice Date],
    [Invoice Num],
    TRY_CAST([Po Number] AS int) AS [Po Number],
    [Invoice Amount (Orig Curr)],
    [Invoice Currency Code],
    [AUD Invoice Amount],
    [GST Amount],
    [Description],
    [Inv Payment Term],
    TRY_CAST([Due Date] AS date) AS [Due Date],
    [Amount Paid (Orig Curr)],
    TRY_CAST([Invoice Received Date] AS date) AS [Invoice Received Date],
    TRY_CAST([Invoice Creation Date] AS date) AS [Invoice Creation Date],
    [Invoice Type Lookup Code],
    [Pay Group Lookup Code],
    [Inv Source],
    TRY_CAST([Payment Date] AS date) AS [Payment Date],
    [Status],
    [Payment Request],
    TRY_CAST([Project Number] AS int) AS [Project Number],
    [Project Name]
FROM #TmpApInvoiceHdd
";
            }
        }

        #endregion

        private XPObjectSpace objSpace;

        public string Execute(ImportApInvoiceHeaderParam paramObj)
        {
            var inputFilePath = paramObj.FilePath;

            Func<string, string> formatSql = delegate (string sql)
            {
                return Smart.Format(sql, new
                {
                    FromDate = string.Format("{0:yyyy-MM-dd}", paramObj.FromDate.Date),
                    ToDate = string.Format("{0:yyyy-MM-dd}", paramObj.ToDate.Date)
                });
            };

            var statusMessage = string.Empty;

            var conn = (SqlConnection)objSpace.Connection;

            int rowCount = 0;

            using (var csv = new CachedCsvReader(new StreamReader(inputFilePath), true))
            using (var cmd = conn.CreateCommand())
            using (var bc = new SqlBulkCopy(conn))
            {
                cmd.CommandTimeout = CashDiscipline.Common.Constants.SqlCommandTimeout;

                cmd.CommandText = createSql;
                cmd.ExecuteNonQuery();

                bc.DestinationTableName = "#TmpApInvoiceHdd";
                bc.WriteToServer(csv);

                cmd.CommandText = "SELECT COUNT(*) FROM #TmpApInvoiceHdd";
                rowCount = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.CommandText = formatSql(persistSql);
                cmd.ExecuteNonQuery();
            }
            statusMessage = string.Format("{0} rows processed.", rowCount);
            return statusMessage;
        }
    }
}
