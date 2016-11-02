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
    public class ApPoContractImporter
    {
        public ApPoContractImporter(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }


        #region Constants
        private string createSql
        {
            get
            {
                return @"IF OBJECT_ID('tempdb..#TmpApPoContract') IS NOT NULL DROP TABLE #TmpApPoContract
CREATE TABLE #TmpApPoContract (
[Vendor Name] nvarchar(255),
[Vendor Site Code] nvarchar(255),
[Contract Number] int,
[Contract Status] nvarchar(255),
[Contract Date] date,
[Contract Description] nvarchar(255),
[Start Date] nvarchar(255),
[End Date] nvarchar(255),
[Contract Amt Agreed SUM] nvarchar(255),
[Currency Code] nvarchar(255),
[Req Number] nvarchar(255),
[Req Date] date,
[Pr Status] nvarchar(255),
[Req Description] nvarchar(255),
[Req Amount SUM] nvarchar(255),
[Standard Po#] nvarchar(255),
[Po Date] nvarchar(255),
[Standard Po Status] nvarchar(255),
[Ordered Amt SUM] nvarchar(255),
[Received Amt SUM] nvarchar(255),
[Billed Amt SUM] nvarchar(255),

)";
            }
        }

        private string persistSql
        {
            get
            {
                return
@"DELETE FROM VHAFinance.dbo.PoContract

INSERT INTO VHAFinance.dbo.PoContract
(
    [Oid],
    [Vendor Name],
    [Vendor Site Code],
    [Contract Number],
    [Contract Status],
    [Contract Date],
    [Contract Description],
    [Start Date],
    [End Date],
    [Contract Amt Agreed],
    [Currency Code],
    [Req Number],
    [Req Date],
    [Pr Status],
    [Req Description],
    [Req Amount],
    [Standard Po Num],
    [Po Date],
    [Standard Po Status],
    [Ordered Amt],
    [Received Amt],
    [Billed Amt]
)
SELECT
    NEWID() AS Oid,
    [Vendor Name],
    [Vendor Site Code],
    [Contract Number],
    [Contract Status],
    [Contract Date],
    [Contract Description],
    [Start Date],
    [End Date],
    [Contract Amt Agreed SUM],
    [Currency Code],
    [Req Number],
    [Req Date],
    [Pr Status],
    [Req Description],
    [Req Amount SUM],
    TRY_CAST([Standard Po#] AS int),
    TRY_CAST([Po Date] AS date),
    [Standard Po Status],
    TRY_CAST([Ordered Amt SUM] AS float),
    TRY_CAST([Received Amt SUM] AS float),
    TRY_CAST([Billed Amt SUM] AS float)
FROM #TmpApPoContract
";
            }
        }

        #endregion

        private XPObjectSpace objSpace;

        public string Execute(ImportApPoContractParam paramObj)
        {
            var statusMessage = string.Empty;

            Func<string, string> formatSql = delegate (string sql)
            {
                return Smart.Format(sql, new
                {
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

                bc.DestinationTableName = "#TmpApPoContract";
                bc.WriteToServer(csv);

                cmd.CommandText = "SELECT COUNT(*) FROM #TmpApPoContract";
                rowCount = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.CommandText = formatSql(persistSql);
                cmd.ExecuteNonQuery();

            }
            statusMessage = string.Format("{0} rows processed.", rowCount);
            return statusMessage;
        }
    }
}
