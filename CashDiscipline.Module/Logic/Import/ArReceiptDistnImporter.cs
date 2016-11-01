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
    public class ArReceiptDistnImporter
    {
        public ArReceiptDistnImporter(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }

        #region Constants
        private string createSql
        {
            get
            {
                return @"IF OBJECT_ID('tempdb..#TmpArReceipts') IS NOT NULL DROP TABLE #TmpArReceipts
CREATE TABLE #TmpArReceipts (
[Statement Line Id] nvarchar(255),
[Statement Date] nvarchar(255),
[Line Number] nvarchar(255),
[Receipt Number] nvarchar(255),
[Receipt Date] date,
[Receipt Currency] nvarchar(255),
[Remittance Bank Account] nvarchar(255),
[Receipt Status] nvarchar(255),
[Invoice Receipt Status] nvarchar(255),
[Receipt History Status] nvarchar(255),
[Due Date] date,
[Trx Date] date,
[Customer Number] nvarchar(255),
[Customer Name] nvarchar(255),
[Cust Trx Type] nvarchar(255),
[Trx Number] nvarchar(255),
[Inv Currency] nvarchar(255),
[Distribution Amount] float,
[Allocated Receipt Amount] float,
[Original Receipt Amount] float,
[Original Invoice Amount] float,
[Receipt Distribution Amount] float,
[Inv Line Description] nvarchar(255),
[Dist Gl Date] nvarchar(255),
[Description] nvarchar(255),
[Segment1] nvarchar(10),
[Segment2] nvarchar(10),
[Segment3] nvarchar(10),
[Segment4] nvarchar(10),
[Segment5] nvarchar(10),
[Segment6] nvarchar(255),
[Segment7] nvarchar(10),
[Segment8] nvarchar(10),
[Segment9] nvarchar(255),
[Trans Line] nvarchar(255),
[Line Class] nvarchar(255)
)";
            }
        }

        private string persistSql
        {
            get
            {
                return
@"DELETE FROM VHAFinance.dbo.ArReceiptInvoiceDistn
WHERE [Receipt Date] BETWEEN '{FromDate}' AND '{ToDate}'

INSERT INTO VHAFinance.dbo.ArReceiptInvoiceDistn
(
    [Oid],
    [Receipt Number],
    [Receipt Date],
    [Receipt Currency],
    [Remittance Bank Account],
    [Receipt Status],
    [Invoice Receipt Status],
    [Receipt History Status],
    [Due Date],
    [Trx Date],
    [Customer Number],
    [Customer Name],
    [Cust Trx Type],
    [Trx Number],
    [Inv Currency],
    [Distribution Amount],
    [Allocated Receipt Amount],
    [Original Receipt Amount],
    [Original Invoice Amount],
    [Receipt Distribution Amount],
    [Inv Line Description],
    [Description],
    [Segment1],
    [Segment2],
    [Segment3],
    [Segment4],
    [Segment5],
    [Segment6],
    [Segment7],
    [Segment8],
    [Segment9],
    [Trans Line],
    [Line Class]
)
SELECT
    NEWID() AS Oid,
    TRY_CAST([Receipt Number] AS int),
    [Receipt Date],
    [Receipt Currency],
    [Remittance Bank Account],
    [Receipt Status],
    [Invoice Receipt Status],
    [Receipt History Status],
    [Due Date],
    [Trx Date],
    [Customer Number],
    [Customer Name],
    [Cust Trx Type],
    [Trx Number],
    [Inv Currency],
    [Distribution Amount],
    [Allocated Receipt Amount],
    [Original Receipt Amount],
    [Original Invoice Amount],
    [Receipt Distribution Amount],
    [Inv Line Description],
    [Description],
    [Segment1],
    [Segment2],
    [Segment3],
    [Segment4],
    [Segment5],
    [Segment6],
    [Segment7],
    [Segment8],
    [Segment9],
    [Trans Line],
    [Line Class]
FROM #TmpArReceipts
";
            }
        }

        #endregion

        private XPObjectSpace objSpace;

        public string Execute(ImportArReceiptDistnParam paramObj)
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

                bc.DestinationTableName = "#TmpArReceipts";
                bc.WriteToServer(csv);

                cmd.CommandText = "SELECT COUNT(*) FROM #TmpArReceipts";
                rowCount = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.CommandText = formatSql(persistSql);
                cmd.ExecuteNonQuery();
            }
            statusMessage = string.Format("{0} rows processed.", rowCount);
            return statusMessage;
        }
    }
}
