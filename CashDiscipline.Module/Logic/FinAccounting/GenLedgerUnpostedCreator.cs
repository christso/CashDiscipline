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
    AND GenLedger.JournalGroup IN ('63dab5b7-bc33-47b6-8032-b9ceb8d868d4','027cb724-560a-4f21-a395-05c4fd435419','4389c348-35bf-49fe-ab8d-bbb37ba661f1','a24de1ec-af74-4edd-9bd2-e003024a7924','63adbcdd-2d6f-43d5-aa15-e2a3406fbf46','c3461a1d-5501-4722-9e19-458ccb15aedf','2fec3a3b-09d1-47e8-a4b2-be57607b1c38','53ea0b90-a1f3-4dbe-a11e-39c776270254','441cbd1c-3128-4f81-9028-d564234031e0','10e8f3ba-6660-4f1e-8434-13f2ccc9a3ea','8877d88e-2df4-4d2f-9122-0d553dd877e6','730bad02-a2c0-47ec-96d1-20a38513e096')

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
	ROUND(BankStmt.FunctionalCcyAmt - COALESCE(gl.FunctionalCcyAmt,0.00),2) AS FunctionalCcyAmt,
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
		AND gl.IsActivity = 0
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
	AND ROUND(BankStmt.FunctionalCcyAmt - COALESCE(gl.FunctionalCcyAmt,0.00),2) <> 0.00
    AND FinAccount.JournalGroup IN ('63dab5b7-bc33-47b6-8032-b9ceb8d868d4','027cb724-560a-4f21-a395-05c4fd435419','4389c348-35bf-49fe-ab8d-bbb37ba661f1','a24de1ec-af74-4edd-9bd2-e003024a7924','63adbcdd-2d6f-43d5-aa15-e2a3406fbf46','c3461a1d-5501-4722-9e19-458ccb15aedf','2fec3a3b-09d1-47e8-a4b2-be57607b1c38','53ea0b90-a1f3-4dbe-a11e-39c776270254','441cbd1c-3128-4f81-9028-d564234031e0','10e8f3ba-6660-4f1e-8434-13f2ccc9a3ea','8877d88e-2df4-4d2f-9122-0d553dd877e6','730bad02-a2c0-47ec-96d1-20a38513e096')
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
