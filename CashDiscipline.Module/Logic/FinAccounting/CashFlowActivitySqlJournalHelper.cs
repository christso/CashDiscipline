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
    public class CashFlowActivitySqlJournalHelper : ActivitySqlJournalHelper
    {
        public CashFlowActivitySqlJournalHelper(XPObjectSpace objSpace, FinGenJournalParam paramObj)
            : base(objSpace, paramObj)
        {
        }

        #region SQL

        protected override string ProcessCommandTextTemplate
        {
            get
            {
                return
@"-- CashFlow Activity
-- This will create Gen Ledgers for CashFlow items that match an activity mapped via SQL algorithm

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
    IsJournal,
    FunctionalCcyAmt
)
SELECT
    NEWID() AS Oid,
	CashFlow.Oid AS SrcCashFlow,
	NULL AS SrcBankStmt,
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
    CASE
        WHEN FinActivity.[GlDescription] IS NOT NULL AND FinActivity.[GlDescription] <> ''    
            AND CashFlow.Description IS NOT NULL AND CashFlow.Description <> ''
        THEN [FinActivity].[GlDescription] + '_' + CashFlow.Description
        ELSE [FinActivity].[GlDescription]
    END AS GlDescription,
	0 AS GenLedgerEntryType,
	1 AS IsActivity,
    CashFlow.TranDate AS GlDate,
    GETDATE() AS CreationDateTime,
    1 AS IsJournal,
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
	cf.Oid,
	FinActivity.Oid AS FinActivity,
    ROW_NUMBER() OVER (PARTITION BY FinActivity.JournalGroup, cf.Oid ORDER BY cf.Oid) AS CalcRow
FROM CashFlow cf
JOIN [FinActivity] ON cf.Activity = FinActivity.FromActivity
WHERE FinActivity.GCRecord IS NULL AND cf.GCRecord IS NULL
) cf
JOIN CashFlow ON CashFlow.Oid = cf.Oid
JOIN [FinActivity] ON cf.FinActivity = FinActivity.Oid
JOIN [FinAccount] ON CashFlow.Account = FinAccount.Account
	AND FinAccount.JournalGroup = FinActivity.JournalGroup
	AND FinAccount.GCRecord IS NULL
WHERE
	FinActivity.[Algorithm] = @Algorithm
	AND FinActivity.TargetObject IN (@TargetObject_All, @TargetObject_CashFlow)
	AND FinActivity.[Enabled] = 1
    AND FinActivity.JournalGroup IN ({JG})
    AND CashFlow.TranDate BETWEEN @FromDate AND @ToDate
    AND CashFlow.Snapshot = @Snapshot
    AND CashFlow.Source != @StmtSource
    AND CashFlow.GCRecord IS NULL";
            }
        }

        #endregion
    }
}
