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
using SmartFormat;

namespace CashDiscipline.Module.Logic.FinAccounting
{
    public class BankStmtActivitySqlJournalHelper : ActivitySqlJournalHelper
    {
        public BankStmtActivitySqlJournalHelper(XPObjectSpace objSpace, FinGenJournalParam paramObj)
            : base(objSpace, paramObj)
        {
        }

        #region SQL

        protected override string ProcessCommandTextTemplate
        {
            get
            {
                return
@"-- BankStmt Activity
-- This will create Gen Ledgers for BankStmt items that match an activity mapped via SQL algorithm

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
    GlDescription,
    EntryType,
    IsActivity,
    GlDate,
    CreationDateTime,
    FunctionalCcyAmt
)
SELECT
    NEWID() AS Oid,
	NULL AS SrcCashFlow,
	BankStmt.Oid AS SrcBankStmt,
    [FinActivity].[JournalGroup],
	[FinActivity].[ToActivity] AS Activity,
	[FinActivity].[GlCompany],
    [FinActivity].[GlAccount],
    [FinActivity].[GlCostCentre],
    [FinActivity].[GlProduct],
    [FinActivity].[GlSalesChannel],
    [FinActivity].[GlCountry],
    [FinActivity].[GlIntercompany],
    [FinActivity].[GlProject],
    [FinActivity].[GlLocation],
    [FinActivity].[GlDescription],
	0 AS GenLedgerEntryType,
	1 AS IsActivity,
    BankStmt.TranDate AS GlDate,
    GETDATE() AS CreationDateTime,
    {FA} AS FunctionalCcyAmt
{filter}";

    }
}

        protected override string FilterCommandTextTemplate
        {
            get
            {
                return
@"FROM
(
SELECT 
	BankStmt.*,
	FinActivity.Oid AS FinActivity,
    ROW_NUMBER() OVER (PARTITION BY FinActivity.JournalGroup, BankStmt.Oid ORDER BY BankStmt.Oid) AS CalcRow
FROM BankStmt
JOIN [FinActivity] ON BankStmt.Activity = FinActivity.FromActivity
WHERE FinActivity.GCRecord IS NULL AND BanKStmt.GCRecord IS NULL
) BankStmt
JOIN [FinActivity] ON BankStmt.FinActivity = FinActivity.Oid
JOIN [FinAccount] ON BankStmt.Account = FinAccount.Account
	AND FinAccount.JournalGroup = FinActivity.JournalGroup
	AND FinAccount.GCRecord IS NULL
WHERE
	FinActivity.[Algorithm] = @Algorithm
	AND FinActivity.TargetObject IN (@TargetObject_All, @TargetObject_BankStmt)
	AND FinActivity.[Enabled] = 1
    AND FinActivity.JournalGroup IN ({JG})
    AND BankStmt.TranDate BETWEEN @FromDate AND @ToDate
    AND BanKStmt.GCRecord IS NULL";
            }
        }

        #endregion
    }
}
