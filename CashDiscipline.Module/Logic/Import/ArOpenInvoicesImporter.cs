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
    public class ArOpenInvoicesImporter
    {
        public ArOpenInvoicesImporter(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }


        #region Constants
        private string createSql
        {
            get
            {
                return @"IF OBJECT_ID('tempdb..#TmpArOpenInvoice') IS NOT NULL DROP TABLE #TmpArOpenInvoice
CREATE TABLE #TmpArOpenInvoice (
[Customer Name] nvarchar(255),
[Customer Number] nvarchar(255),
[Trx Date] date,
[Due Date] date,
[Trx Type] nvarchar(255),
[Trx Number] nvarchar(255),
[Apply Date] nvarchar(255),
[Trx Status] nvarchar(255),
[Sales Order] nvarchar(255),
[Reference] nvarchar(255),
[Currency Code] nvarchar(255),
[Original Amount] nvarchar(255),
[Balance Due] nvarchar(255),
[Credited Amount] nvarchar(255),
[Adjustment Amount] nvarchar(255),
[Applied Amount] nvarchar(255),
[Original Receipt Amount] nvarchar(255),
[Overdue Days] nvarchar(255)
)";
            }
        }

        private string persistSql
        {
            get
            {
                return
@"DELETE FROM VHAFinance.dbo.ArOpenInvoices
WHERE [AsAtDate] = '{AsAtDate}'

INSERT INTO VHAFinance.dbo.ArOpenInvoices
(
    [Oid],
    [AsAtDate],
    [CustomerName],
    [CustomerNumber],
    [TrxDate],
    [DueDate],
    [TrxType],
    [TrxNumber],
    [ApplyDate],
    [TrxStatus],
    [SalesOrder],
    [Reference],
    [CurrencyCode],
    [OriginalAmount],
    [BalanceDue],
    [CreditedAmount],
    [AdjustmentAmount],
    [AppliedAmount],
    [OriginalReceiptAmount],
    [OverdueDays]
)
SELECT
    NEWID() AS Oid,
    {AsAtDate} AS [As At Date],
    [Customer Name],
    [Customer Number],
    [Trx Date],
    [Due Date],
    [Trx Type],
    [Trx Number],
    TRY_CAST([Apply Date] AS date),
    [Trx Status],
    [Sales Order],
    [Reference],
    [Currency Code],
    TRY_CAST([Original Amount] AS float),
    TRY_CAST([Balance Due] AS float),
    TRY_CAST([Credited Amount] AS float),
    TRY_CAST([Adjustment Amount] AS float),
    TRY_CAST([Applied Amount] AS float),
    TRY_CAST([Original Receipt Amount] AS float),
    TRY_CAST([Overdue Days] AS int)
FROM #TmpArOpenInvoice
";
            }
        }

        #endregion

        private XPObjectSpace objSpace;

        public string Execute(ImportArOpenInvoicesParam paramObj)
        {
            var statusMessage = string.Empty;

            Func<string, string> formatSql = delegate (string sql)
            {
                return Smart.Format(sql, new
                {
                    AsAtDate = string.Format("{0:yyyy-MM-dd}", paramObj.AsAtDate.Date),
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

                bc.DestinationTableName = "#TmpArOpenInvoice";
                bc.WriteToServer(csv);

                cmd.CommandText = "SELECT COUNT(*) FROM #TmpArOpenInvoice";
                rowCount = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.CommandText = formatSql(persistSql);
                cmd.ExecuteNonQuery();

            }
            statusMessage = string.Format("{0} rows processed.", rowCount);
            return statusMessage;
        }
    }
}
