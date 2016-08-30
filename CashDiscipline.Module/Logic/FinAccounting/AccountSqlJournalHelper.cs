/* Parameters in SQL
DECLARE @FromDate date = '2013-01-01'
DECLARE @ToDate date = '2016-12-31'
DECLARE @Algorithm int = 1
DECLARE @TargetObject_All int = 0
DECLARE @TargetObject_BankStmt int = 1
DECLARE @GenLedgerEntryType int = 0
*/

using CashDiscipline.Module.BusinessObjects.FinAccounting;
using CashDiscipline.Module.ParamObjects.FinAccounting;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CashDiscipline.Module.Logic.FinAccounting
{
    public class AccountSqlJournalHelper
    {
        protected readonly XPObjectSpace objSpace;
        private readonly FinGenJournalParam paramObj;
        private DateTime fromDate;
        private DateTime toDate;

        #region SQL

        private const string ProcessCommandTextTemplate =
@"-- This will create the account-side of all activity Gen Ledgers

-- BankStmt
INSERT INTO GenLedger
(
    Oid,
    SrcCashFlow,
    SrcBankStmt,
    JournalGroup,
    Activity,
    GlDescription,
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
    EntryType,
    IsActivity,
    GlDate,
    CreationDateTime
)
SELECT 
    NEWID() AS Oid,
	GenLedger.SrcCashFlow,
	GenLedger.SrcBankStmt,
    GenLedger.[JournalGroup],
    GenLedger.Activity,
	CASE 
		WHEN FinAccount.[GlDescription] IS NOT NULL
		THEN FinAccount.[GlDescription]
		WHEN BankStmt.SummaryDescription IS NOT NULL
		THEN BankStmt.TranDescription + '_' + BankStmt.SummaryDescription
		ELSE BankStmt.TranDescription
		END AS [GlDescription],
    FinAccount.[GlCompany],
    FinAccount.[GlAccount],
    FinAccount.[GlCostCentre],
    FinAccount.[GlProduct],
    FinAccount.[GlSalesChannel],
    FinAccount.[GlCountry],
    FinAccount.[GlIntercompany],
    FinAccount.[GlProject],
    FinAccount.[GlLocation],
	GenLedger.FunctionalCcyAmt * -1 AS FunctionalCcyAmt,
    0 AS GenLedgerEntryType,
    0 AS IsActivity,
    GenLedger.GlDate,
    GETDATE()
FROM GenLedger
LEFT JOIN BankStmt ON BankStmt.Oid = GenLedger.SrcBankStmt
    AND BankStmt.TranDate BETWEEN @FromDate AND @ToDate
LEFT JOIN FinAccount ON BankStmt.Account = FinAccount.Account
	AND FinAccount.GCRecord IS NULL
WHERE GenLedger.GCRecord IS NULL
AND FinAccount.JournalGroup IN ({0})

-- Cash Flow
INSERT INTO GenLedger
(
    Oid,
    SrcCashFlow,
    SrcBankStmt,
    JournalGroup,
    Activity,
    GlDescription,
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
    EntryType,
    IsActivity,
    GlDate,
    CreationDateTime
)
SELECT 
    NEWID() AS Oid,
	GenLedger.SrcCashFlow,
	GenLedger.SrcBankStmt,
    GenLedger.[JournalGroup],
    GenLedger.Activity,
	CASE
		WHEN FinAccount.[GlDescription] IS NOT NULL
		THEN FinAccount.[GlDescription]
		ELSE CashFlow.[Description]
		END AS [GlDescription],
    FinAccount.[GlCompany],
    FinAccount.[GlAccount],
    FinAccount.[GlCostCentre],
    FinAccount.[GlProduct],
    FinAccount.[GlSalesChannel],
    FinAccount.[GlCountry],
    FinAccount.[GlIntercompany],
    FinAccount.[GlProject],
    FinAccount.[GlLocation],
	GenLedger.FunctionalCcyAmt * -1 AS FunctionalCcyAmt,
    0 AS GenLedgerEntryType,
    0 AS IsActivity,
    GenLedger.GlDate,
    GETDATE()
FROM GenLedger
LEFT JOIN CashFlow ON CashFlow.Oid = GenLedger.SrcCashFlow
    AND CashFlow.TranDate BETWEEN @FromDate AND @ToDate
LEFT JOIN FinAccount ON CashFlow.Account = FinAccount.Account
	AND FinAccount.GCRecord IS NULL
WHERE GenLedger.GCRecord IS NULL
AND FinAccount.JournalGroup IN ({0})
";

        #endregion

        public AccountSqlJournalHelper(XPObjectSpace objSpace, FinGenJournalParam paramObj)
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
            var clauses = CreateSqlParameters();
            var parameters = sqlUtil.CreateParameters(clauses);

            var commandText = CreateProcessCommandText();

            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            command.Parameters.AddRange(parameters.ToArray());
            command.CommandText = commandText;
            //Debug.Print(commandText);
            int result = command.ExecuteNonQuery();
        }

        public List<SqlDeclareClause> CreateSqlParameters()
        {
            var res = string.Format("{0}", Convert.ToInt32(FinMapAlgorithmType.SQL));
            var clauses = new List<SqlDeclareClause>()
            {
                new SqlDeclareClause("FromDate", "date", string.Format("'{0}'", fromDate.ToString("yyyy-MM-dd"))),
                new SqlDeclareClause("ToDate", "date", string.Format("'{0}'", toDate.ToString("yyyy-MM-dd"))),
            };
            return clauses;
        }
        
        public string CreateProcessCommandText()
        {
            var journalGroupOids = paramObj.JournalGroupParams
                .Select(x => "'" + x.JournalGroup.Oid + "'");

            var journalGroupsParamText = string.Join(",", journalGroupOids);

            var commandText = string.Format(ProcessCommandTextTemplate,
                journalGroupsParamText);
            return commandText;
        }
    }
}
