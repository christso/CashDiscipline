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
    FunctionalCcyAmt,
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
    IsActivity
)
SELECT 
    NEWID() AS Oid,
    CashFlow.Oid AS SrcCashFlow,
	NULL AS SrcBankStmt,
    [FinActivity].[JournalGroup],
    [FinActivity].[ToActivity] AS Activity,
	{FA} AS FunctionalCcyAmt,
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
    1 AS IsActivity
" + FilterCommandTextTemplate;

            }
        }

        protected override string FilterCommandTextTemplate
        {
            get
            {
                return
@"FROM CashFlow
JOIN [FinActivity] ON CashFlow.Activity = FinActivity.FromActivity
JOIN [FinAccount] ON CashFlow.Account = FinAccount.Account
	AND FinAccount.JournalGroup = FinActivity.JournalGroup
WHERE FinActivity.GCRecord IS NULL
	AND FinActivity.[Algorithm] = @Algorithm
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
