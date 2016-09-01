using CashDiscipline.Module.BusinessObjects.FinAccounting;
using CashDiscipline.Module.ParamObjects.FinAccounting;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using SmartFormat;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CashDiscipline.Module.Logic.FinAccounting
{
    public class GenLedgerUnpostedCreator
    {
        protected readonly XPObjectSpace objSpace;
        private readonly FinGenJournalParam paramObj;
        private DateTime fromDate;
        private DateTime toDate;

        #region SQL

        private string ProcessCommandTextTemplate
        {
            get
            {
                return @"
DECLARE @FromDate date = {fromdate}
DECLARE @ToDate date = {todate}

DELETE 
FROM GenLedger
WHERE 
	GenLedger.GlDate BETWEEN @FromDate AND @ToDate
	AND GenLedger.IsJournal = 0
    AND GenLedger.JournalGroup IN ({jg})

INSERT INTO GenLedger
(
Oid,
SrcCashFlow,
SrcBankStmt,
JournalGroup,
Activity,
GlCompany,
GlAccount,
GlCostCentre,
GlProduct,
GlSalesChannel,
GlCountry,
GlIntercompany,
GlProject,
GlLocation,
FunctionalCcyAmt,
GlDescription,
EntryType,
IsActivity,
GlDate,
CreationDateTime,
IsJournal
)
SELECT
	NEWID() AS Oid,
	CAST(NULL as uniqueidentifier) AS SrcCashFlow,
	BankStmt.Oid AS SrcBankStmt,
	FinAccount.JournalGroup,
	BankStmt.Activity,
	'0' AS GlCompany,
	'0' AS GlAccount,
	'0' AS GlCostCentre,
	'0' AS GlProduct,
	'0' AS GlSalesChannel,
	'0' AS GlCountry,
	'0' AS GlIntercompany,
	'0' AS GlProject,
	'0' AS GlLocation,
	ROUND(BankStmt.FunctionalCcyAmt + COALESCE(gl.FunctionalCcyAmt,0.00),2)*-1 AS FunctionalCcyAmt,
	BankStmt.TranDescription AS GlDescription,
	0 AS EntryType,
	1 AS IsActivity,
	BankStmt.TranDate AS GlDate,
	GETDATE() AS CreationDateTime,
	0 AS IsJournal
FROM BankStmt
LEFT JOIN 
(
	SELECT 
		gl.SrcBankStmt,
		SUM(gl.FunctionalCcyAmt) AS FunctionalCcyAmt
	FROM GenLedger gl
	WHERE
        gl.GCRecord IS NULL
		AND gl.IsActivity = 1
        AND gl.IsJournal = 1
		AND gl.SrcBankStmt IS NOT NULL
		AND gl.GlDate BETWEEN @FromDate AND @ToDate
	GROUP BY gl.SrcBankStmt
) gl ON
	gl.SrcBankStmt = BankStmt.Oid
JOIN FinAccount ON FinAccount.Account = BankStmt.Account
	AND FinAccount.GCRecord IS NULL
WHERE 
	BankStmt.TranDate BETWEEN @FromDate AND @ToDate
	AND BankStmt.GCRecord IS NULL
	AND ROUND(BankStmt.FunctionalCcyAmt + COALESCE(gl.FunctionalCcyAmt,0.00),2)*-1 <> 0.00
    AND FinAccount.JournalGroup IN ({jg})
";
            }
        }

        #endregion

        public GenLedgerUnpostedCreator(XPObjectSpace objSpace, FinGenJournalParam paramObj)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;

            if (paramObj.FromDate <= SqlDateTime.MinValue.Value)
                this.fromDate = SqlDateTime.MinValue.Value;
            else
                this.fromDate = paramObj.FromDate;

            if (paramObj.ToDate <= SqlDateTime.MinValue.Value)
                this.toDate = SqlDateTime.MaxValue.Value;
            else
                this.toDate = paramObj.ToDate;
        }

        public void Process()
        {
            var sqlUtil = new SqlProcessorUtil(objSpace.Session);
            //var clauses = CreateSqlParameters();
            //var parameters = sqlUtil.CreateParameters(clauses);

            var commandText = CreateProcessCommandText();

            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            //command.Parameters.AddRange(parameters.ToArray());
            command.CommandText = commandText;
            int result = command.ExecuteNonQuery();
        }

        //public List<SqlDeclareClause> CreateSqlParameters()
        //{
        //    var clauses = new List<SqlDeclareClause>()
        //    {
        //        new SqlDeclareClause("FromDate", "datetime", string.Format("'{0}'", fromDate.ToString("yyyy-MM-dd"))),
        //        new SqlDeclareClause("ToDate", "datetime", string.Format("'{0}'", toDate.ToString("yyyy-MM-dd"))),
        //    };
        //    return clauses;
        //}

        public string CreateProcessCommandText()
        {
            var journalGroupOids = paramObj.JournalGroupParams
                .Select(x => "'" + x.JournalGroup.Oid + "'");

            var journalGroupsParamText = string.Join(",", journalGroupOids);

            var commandText = Smart.Format(ProcessCommandTextTemplate,
                new {
                    jg = journalGroupsParamText,
                    fromdate = string.Format("'{0}'", fromDate.ToString("yyyy-MM-dd")),
                    todate = string.Format("'{0}'", toDate.ToString("yyyy-MM-dd"))
                });
            return commandText;
        }
    }
}
