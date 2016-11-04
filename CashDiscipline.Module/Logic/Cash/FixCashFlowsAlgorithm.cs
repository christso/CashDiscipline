using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.BusinessObjects.Setup;
using CashDiscipline.Module.ParamObjects.Cash;
using Xafology.ExpressApp.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using System.Linq;
using DevExpress.Persistent.BaseImpl;
using Xafology.ExpressApp.Xpo.Import;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Diagnostics;
using SmartFormat;

/* PARAMETERS
DECLARE @FromDate date = (SELECT TOP 1 FromDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @ToDate date = (SELECT TOP 1 ToDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @ApayableLockdownDate date = (SELECT TOP 1 ApayableLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @ApayableNextLockdownDate date = (SELECT TOP 1 ApayableNextLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @PayrollLockdownDate date = (SELECT TOP 1 PayrollLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @PayrollNextLockdownDate date = (SELECT TOP 1 PayrollNextLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @IgnoreFixTagType int = 0
DECLARE @AllocateFixTagType int = 1
DECLARE @ScheduleInFixTagType int = 2
DECLARE @ScheduleOutFixTagType int = 3
DECLARE @ForecastStatus int = 0
DECLARE @Snapshot uniqueidentifier = COALESCE(
	(SELECT TOP 1 [Snapshot] FROM CashFlowFixParam WHERE GCRecord IS NULL),
	(SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks WHERE GCRecord IS NULL)
)
DECLARE @DefaultCounterparty uniqueidentifier = (SELECT TOP 1 [Counterparty] FROM CashFlowDefaults WHERE GCRecord IS NULL)
DECLARE @FunctionalCurrency uniqueidentifier = (SELECT TOP 1 [FunctionalCurrency] FROM SetOfBooks WHERE GCRecord IS NULL)
DECLARE @ReversalFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'R' AND GCRecord IS NULL)
DECLARE @RevRecFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'RR' AND GCRecord IS NULL)
DECLARE @ResRevRecFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'RRR' AND GCRecord IS NULL)
DECLARE @PayrollFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'PR' AND GCRecord IS NULL)
DECLARE @AutoFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'Auto' AND GCRecord IS NULL)
DECLARE @ApReclassActivity uniqueidentifier = (SELECT TOP 1 ApReclassActivity FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @UndefActivity uniqueidentifier = (select oid from activity where activity.name like 'UNDEFINED' and GCRecord IS NULL)
 */

/* DEBUG

SELECT *, '#TmpFixReversal' AS TableName FROM #TmpFixReversal
UNION ALL
SELECT *, '#TmpFixRevReclass_Fixee' AS TableName FROM #TmpFixRevReclass_Fixee
UNION ALL
SELECT *, '#TmpFixRevReclass_Fixer' AS TableName FROM #TmpFixRevReclass_Fixer
UNION ALL
SELECT *, '#TmpFixResRevRec_Fixee' AS TableName FROM #TmpFixResRevRec_Fixee
UNION ALL
SELECT *, '#TmpFixResRevReclass_Fixer' AS TableName FROM #TmpFixResRevReclass_Fixer

SELECT fixeeFixer.*, 
	fixee.TranDate, 
	fixee.Counterparty,
	fixee.AccountCcyAmt,
	fixee.Account,
	fixeeAccount.FixAccount,
	fixee.FixFromDate,
	fixee.FixToDate
FROM #TmpFixeeFixer fixeeFixer
LEFT JOIN CashFlow fixee ON fixee.Oid = fixeeFixer.Fixee
LEFT JOIN Account fixeeAccount ON fixee.Account = fixeeAccount.Oid
*/

namespace CashDiscipline.Module.Logic.Cash
{
    public class FixCashFlowsAlgorithm : IFixCashFlows
    {
        private XPObjectSpace objSpace;
        private CashFlowFixParam paramObj;
        private Activity paramApReclassActivity;
        private Counterparty defaultCounterparty;
        private CashForecastFixTag reversalFixTag;
        private CashForecastFixTag revRecFixTag;
        private CashForecastFixTag resRevRecFixTag;
        private CashForecastFixTag payrollFixTag;
        private CashForecastFixTag autoFixTag;
        private SetOfBooks setOfBooks;
        private CashFlowSnapshot currentSnapshot;
        private CashFlowFixMapper mapper;
        private List<SqlDeclareClause> sqlDeclareClauses;
        private readonly string parameterCommandText;

        public FixCashFlowsAlgorithm(XPObjectSpace objSpace, CashFlowFixParam paramObj)
            : this(objSpace, paramObj, new CashFlowFixMapper(objSpace))
        {

        }

        public FixCashFlowsAlgorithm(XPObjectSpace objSpace, CashFlowFixParam paramObj, CashFlowFixMapper mapper)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;

            if (paramObj.Snapshot == null)
                currentSnapshot = GetCurrentSnapshot(objSpace.Session);
            else
                currentSnapshot = paramObj.Snapshot;

            if (paramObj.ApReclassActivity != null)
                paramApReclassActivity = objSpace.GetObjectByKey<Activity>(objSpace.GetKeyValue(paramObj.ApReclassActivity));

            defaultCounterparty = objSpace.FindObject<Counterparty>(
             CriteriaOperator.Parse("Name LIKE ?", CashDiscipline.Common.Constants.DefaultFixCounterparty));

            var query = new XPQuery<CashForecastFixTag>(objSpace.Session);

            reversalFixTag = query
                .Where(x => x.Name == CashDiscipline.Common.Constants.ReversalFixTag).FirstOrDefault();

            revRecFixTag = query
                .Where(x => x.Name == CashDiscipline.Common.Constants.RevRecFixTag).FirstOrDefault();

            resRevRecFixTag = query
                .Where(x => x.Name == CashDiscipline.Common.Constants.ResRevRecFixTag).FirstOrDefault();

            payrollFixTag = query
                .Where(x => x.Name == CashDiscipline.Common.Constants.PayrollFixTag).FirstOrDefault();

            autoFixTag = query
                .Where(x => x.Name == CashDiscipline.Common.Constants.AutoFixTag).FirstOrDefault();

            setOfBooks = SetOfBooks.GetInstance(objSpace);
            this.mapper = mapper;

            this.sqlDeclareClauses = CreateSqlDeclareClauses();
            var sqlStringUtil = new SqlStringUtil();
            this.parameterCommandText = sqlStringUtil.CreateCommandText(sqlDeclareClauses);
        }

        public void ProcessCashFlows()
        {
            mapper.Process();
            Rephase();
            ApplyFix();
        }

        public List<SqlDeclareClause> CreateSqlDeclareClauses()
        {
            var clauses = new List<SqlDeclareClause>()
            {
                new SqlDeclareClause("FromDate", "date", "(SELECT TOP 1 FromDate FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("ToDate", "date", "(SELECT TOP 1 ToDate FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("ApayableLockdownDate", "date", "(SELECT TOP 1 ApayableLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("ApayableNextLockdownDate", "date", "(SELECT TOP 1 ApayableNextLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("IgnoreFixTagType", "int", "0"),
                new SqlDeclareClause("AllocateFixTagType", "int", "1"),
                new SqlDeclareClause("ScheduleInFixTagType", "int", "2"),
                new SqlDeclareClause("ScheduleOutFixTagType", "int", "3"),
                new SqlDeclareClause("ForecastStatus", "int", "0"),
                new SqlDeclareClause("Snapshot", "uniqueidentifier", @"COALESCE(
	(SELECT TOP 1 [Snapshot] FROM CashFlowFixParam WHERE GCRecord IS NULL),
	(SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks WHERE GCRecord IS NULL)
)"),
                new SqlDeclareClause("DefaultCounterparty", "uniqueidentifier",
                    "(SELECT TOP 1 [Counterparty] FROM CashFlowDefaults WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("FunctionalCurrency", "uniqueidentifier",
                    "(SELECT TOP 1 [FunctionalCurrency] FROM SetOfBooks WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("ReversalFixTag", "uniqueidentifier",
                    "(SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'R' AND GCRecord IS NULL)"),
                new SqlDeclareClause("RevRecFixTag", "uniqueidentifier",
                    "(SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'RR' AND GCRecord IS NULL)"),
                new SqlDeclareClause("ResRevRecFixTag", "uniqueidentifier",
                    "(SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'RRR' AND GCRecord IS NULL)"),
                new SqlDeclareClause("PayrollFixTag", "uniqueidentifier",
                    "(SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'PR' AND GCRecord IS NULL)"),
                new SqlDeclareClause("AutoFixTag", "uniqueidentifier",
                    "(SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'Auto' AND GCRecord IS NULL)"),
                new SqlDeclareClause("ApReclassActivity", "uniqueidentifier",
                    "(SELECT TOP 1 ApReclassActivity FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("PayrollLockdownDate", "date",
                    "(SELECT TOP 1 PayrollLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("PayrollNextLockdownDate", "date",
                    "(SELECT TOP 1 PayrollNextLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("AutoForexSettleType", "int", Convert.ToString(
                        Convert.ToInt32(CashFlowForexSettleType.Auto)))

            };
            return clauses;
        }


        public void ApplyFix()
        {
            if (defaultCounterparty == null)
                throw new ArgumentException("DefaultCounterparty");
            if (paramApReclassActivity == null)
                throw new ArgumentNullException("ApReclassActivity");
            if (payrollFixTag == null)
                throw new ArgumentException("PayrollFixTag");

            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            command.CommandTimeout = CashDiscipline.Common.Constants.SqlCommandTimeout;
            //command.Parameters.AddRange(parameters.ToArray());
            command.CommandText = this.parameterCommandText + "\n\n" + ProcessCommandText;
            int result = command.ExecuteNonQuery();
        }

        public void Map()
        {
            mapper.Process();
        }

        public void Rephase()
        {
            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            //command.Parameters.AddRange(parameters.ToArray());

            command.CommandText =  this.parameterCommandText + "\n\n" + RephaseCommandText;
            command.ExecuteNonQuery();
        }



        public void Reset()
        {
            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            command.CommandText = ResetCommandText;

            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("Snapshot", currentSnapshot.Oid),
            };

            command.Parameters.AddRange(parameters.ToArray());
            command.ExecuteNonQuery();
        }

        private CashFlowSnapshot GetCurrentSnapshot(Session session)
        {
            return CashFlowHelper.GetCurrentSnapshot(session);
        }

        #region Core Sql

        public static string ResetCommandText
        {
            get
            {
                //TODO: unhack: ft.Name IN ('R','RR','RRR')
                return
@"UPDATE CashFlow SET
GCRecord = CAST(RAND() * 2147483646 + 1 AS INT),
Fixer = NULL,
ParentCashFlow = NULL
FROM CashFlow
LEFT JOIN CashForecastFixTag ft 
	ON ft.Oid = CashFlow.Fix AND ft.GCRecord IS NULL

WHERE ft.Name IN ('R','RR','RRR')
AND CashFlow.[Snapshot] = @Snapshot
AND CashFlow.GCRecord IS NULL;";
            }
        }

        public string ProcessCommandText
        {
            get
            {
                return
                    @"
-- CashFlowsToFix

IF OBJECT_ID('tempdb..#TmpCashFlowsToFix') IS NOT NULL DROP TABLE #TmpCashFlowsToFix;

SELECT cf.* INTO #TmpCashFlowsToFix
FROM CashFlow cf
LEFT JOIN CashForecastFixTag tag ON tag.Oid = cf.Fix
LEFT JOIN CashFlow fixer ON fixer.Oid = cf.Oid
WHERE
    cf.GCRecord IS NULL
    AND tag.GCRecord IS NULL
    AND cf.TranDate BETWEEN @FromDate AND @ToDate
	AND cf.[Snapshot] = @Snapshot
    AND (cf.Fix = NULL OR tag.FixTagType != @IgnoreFixTagType)
    AND fixer.GCRecord IS NOT NULL

-- FixeeFixer

IF OBJECT_ID('tempdb..#TmpFixeeFixer') IS NOT NULL DROP TABLE #TmpFixeeFixer;

SELECT Fixee, Fixer
INTO #TmpFixeeFixer
FROM
(
	SELECT
		-- RowNum = 1 if fixee currency equals fixer currency, otherwise may be greater than 1
		ROW_NUMBER() OVER(
			PARTITION BY fixee.Oid
			ORDER BY 
				CASE WHEN fixeeAccount.Currency = fixerAccount.Currency THEN 0 ELSE 1 END,
				COALESCE(fixer.DateUnFix, fixer.TranDate)
		) AS RowNum,
		fixee.Oid AS Fixee,
		fixer.Oid AS Fixer,
		fixee.TranDate,
		fixee.Account,
		fixee.AccountCcyAmt,
		fixee.FixFromDate,
		fixee.FixToDate
	FROM #TmpCashFlowsToFix fixee
	LEFT JOIN Counterparty fixeeCparty ON fixeeCparty.Oid = fixee.Counterparty
	LEFT JOIN Account fixeeAccount ON fixeeAccount.Oid = fixee.Account
	LEFT JOIN #TmpCashFlowsToFix fixer
		ON COALESCE(fixee.DateUnFix, fixee.TranDate) BETWEEN fixer.FixFromDate AND fixer.FixToDate
			AND fixee.FixActivity = fixer.FixActivity
			AND fixer.[Status] = @ForecastStatus
			AND fixer.FixRank > fixee.FixRank
	LEFT JOIN Counterparty fixerCparty ON fixerCparty.Oid = fixer.Counterparty
	LEFT JOIN Account fixerAccount 
		ON fixerAccount.Oid = fixer.Account
	WHERE
		fixee.Account IS NOT NULL AND 
		(
			fixeeAccount.FixAccount = fixerAccount.FixAccount
			OR fixee.Account = fixer.Account
		)
		AND 
		(
			fixer.Counterparty IS NULL OR fixer.Counterparty = @DefaultCounterparty
			OR fixee.Counterparty IS NULL AND fixer.Counterparty IS NULL
			OR fixer.Counterparty = fixeeCparty.FixCounterparty
			OR fixer.Counterparty = fixee.Counterparty
			OR fixerCparty.FixCounterparty = fixeeCparty.FixCounterparty
		)
) T1
WHERE RowNum = 1

-- Insert Cash Flow Reversal
IF OBJECT_ID('tempdb..#TmpFixReversal') IS NOT NULL DROP TABLE #TmpFixReversal;

SELECT cf.*
INTO #TmpFixReversal
FROM #TmpCashFlowsToFix cf
JOIN #TmpFixeeFixer fixeeFixer ON fixeeFixer.Fixee = cf.Oid;

UPDATE revFix SET 
	Oid = NEWID(),
	ParentCashFlow = revFix.Oid,
	TranDate = fixer.TranDate,

	-- if the currencies do not match, use functional currency
	CounterCcy = CASE WHEN revFix.CounterCcy <> fixer.CounterCcy THEN @FunctionalCurrency ELSE revFix.CounterCcy END,
	CounterCcyAmt = CASE WHEN revFix.CounterCcy <> fixer.CounterCcy THEN -revFix.FunctionalCcyAmt ELSE -revFix.CounterCcyAmt END,
    Account = fixer.Account,

	AccountCcyAmt = -revFix.AccountCcyAmt,
	FunctionalCcyAmt = -revFix.FunctionalCcyAmt,
	Fix = @ReversalFixTag,
	Fixer = NULL,
	[Source] = a1.FixSource,
	[Status] = @ForecastStatus,
    TimeEntered = GETDATE()
FROM #TmpFixReversal revFix
LEFT JOIN #TmpFixeeFixer fixeeFixer
	ON fixeeFixer.Fixee = revFix.Oid
LEFT JOIN #TmpCashFlowsToFix fixer
	ON fixer.Oid = fixeeFixer.Fixer
LEFT JOIN Activity a1 ON a1.Oid = revFix.Activity

-- Link Fixee Cash Flow to Fixer

UPDATE CashFlow
SET 
Fixer = fixeeFixer.Fixer
FROM CashFlow cf
	INNER JOIN #TmpFixeeFixer fixeeFixer ON cf.Oid = fixeeFixer.Fixee

-- Reversal logic for AP Lockdown (i.e. payroll is excluded)

	-- Fixee.RR: Reverse Reversal To Reclass where AP <= @ApayableLockdownDate AND Fixer is Allocate
	-- (i.e. reverse the reversal so net reversal is zero because the total amount of AP is correct)
IF OBJECT_ID('tempdb..#TmpFixRevReclass_Fixee') IS NOT NULL DROP TABLE #TmpFixRevReclass_Fixee;

SELECT fr.*
INTO #TmpFixRevReclass_Fixee
FROM #TmpFixReversal fr
JOIN CashFlow fixee ON fixee.Oid = fr.ParentCashFlow
JOIN CashFlow fixer ON fixer.Oid = fixee.Fixer
JOIN CashForecastFixTag fixerTag ON fixerTag.Oid = fixer.Fix
WHERE fr.TranDate <= @ApayableLockdownDate 
	AND fixer.TranDate <= @ApayableLockdownDate 
	AND (@PayrollFixTag IS NULL OR fr.Fix != @PayrollFixTag)
	AND fixerTag.FixTagType = @AllocateFixTagType

UPDATE frr SET 
Oid = NEWID(),
CounterCcyAmt = -frr.CounterCcyAmt,
AccountCcyAmt = -frr.AccountCcyAmt,
FunctionalCcyAmt = -frr.FunctionalCcyAmt,
Fix = @RevRecFixTag,
Activity = @ApReclassActivity,
Source = a1.FixSource
FROM #TmpFixRevReclass_Fixee frr
LEFT JOIN CashFlow fixer ON fixer.Oid = frr.Fixer
LEFT JOIN Activity a1 ON a1.Oid = frr.Activity
;

	-- Fixee.RRR: Restore AP reversal to future date

IF OBJECT_ID('tempdb..#TmpFixResRevRec_Fixee') IS NOT NULL DROP TABLE #TmpFixResRevRec_Fixee;

SELECT fixee.*
INTO #TmpFixResRevRec_Fixee
FROM #TmpFixRevReclass_Fixee fixee

UPDATE frrr SET 
ParentCashFlow = frrr.Oid,
Oid = NEWID(),
Fix = @ResRevRecFixTag,
Activity = @ApReclassActivity,
AccountCcyAmt = -frrr.AccountCcyAmt,
CounterCcyAmt = -frrr.CounterCcyAmt,
FunctionalCcyAmt = -frrr.FunctionalCcyAmt,
TranDate = @ApayableNextLockdownDate,
Source = a1.FixSource,
[Status] = @ForecastStatus,
TimeEntered = GETDATE()
FROM #TmpFixResRevRec_Fixee frrr
LEFT JOIN Activity a1 ON a1.Oid = frrr.Activity

	-- Fixer.RR: Reverse 'Allocate Cash Flow' into AP Pymt
IF OBJECT_ID('tempdb..#TmpFixRevReclass_Fixer') IS NOT NULL DROP TABLE #TmpFixRevReclass_Fixer;

SELECT cf.*
INTO #TmpFixRevReclass_Fixer
FROM #TmpCashFlowsToFix cf
JOIN CashForecastFixTag fixerTag ON fixerTag.Oid = cf.Fix
WHERE cf.TranDate <= @ApayableLockdownDate 
	AND (@PayrollFixTag IS NULL OR cf.Fix != @PayrollFixTag)
	AND fixerTag.FixTagType = @AllocateFixTagType

UPDATE frr SET
ParentCashFlow = frr.Oid, 
Oid = NEWID(),
Fix = @RevRecFixTag,
Activity = @ApReclassActivity,
AccountCcyAmt = -frr.AccountCcyAmt,
FunctionalCcyAmt = -frr.FunctionalCcyAmt,
CounterCcyAmt = -frr.CounterCcyAmt,
Source = a1.FixSource,
[Status] = @ForecastStatus,
TimeEntered = GETDATE()
FROM #TmpFixRevReclass_Fixer frr
LEFT JOIN Activity a1 ON a1.Oid = frr.Activity
;

	-- Fixer.RRR: Restore Fixer Fix to future date

IF OBJECT_ID('tempdb..#TmpFixResRevReclass_Fixer') IS NOT NULL DROP TABLE #TmpFixResRevReclass_Fixer;

SELECT cf.*
INTO #TmpFixResRevReclass_Fixer
FROM #TmpCashFlowsToFix cf
JOIN CashForecastFixTag fixerTag ON fixerTag.Oid = cf.Fix
WHERE cf.TranDate <= @ApayableLockdownDate 
	AND (@PayrollFixTag IS NULL OR cf.Fix != @PayrollFixTag)
	AND fixerTag.FixTagType = @AllocateFixTagType

UPDATE frrr SET
ParentCashFlow = frrr.Oid, 
Oid = NEWID(),
Fix = @ResRevRecFixTag,
Activity = @ApReclassActivity,
TranDate = @ApayableNextLockdownDate,
Source = a1.FixSource,
[Status] = @ForecastStatus,
TimeEntered = GETDATE()
FROM #TmpFixResRevReclass_Fixer frrr
LEFT JOIN Activity a1 ON a1.Oid = frrr.Activity
;

-- Update Reclass

UPDATE fr SET IsReclass = 1
FROM #TmpFixReversal fr 
WHERE EXISTS (SELECT * FROM #TmpFixRevReclass_Fixee frr WHERE fr.ParentCashFlow = frr.ParentCashFlow)

UPDATE #TmpFixRevReclass_Fixee SET IsReclass = 1

UPDATE cf SET IsReclass = 1
FROM CashFlow cf
WHERE EXISTS (SELECT * FROM #TmpFixRevReclass_Fixer frr WHERE frr.ParentCashFlow = cf.Oid)
	AND cf.[Snapshot] = @Snapshot

UPDATE #TmpFixRevReclass_Fixer SET IsReclass = 1

-- Finalize

INSERT INTO CashFlow
SELECT * FROM #TmpFixReversal
UNION ALL
SELECT * FROM #TmpFixRevReclass_Fixee
UNION ALL
SELECT * FROM #TmpFixRevReclass_Fixer
UNION ALL
SELECT * FROM #TmpFixResRevRec_Fixee
UNION ALL
SELECT * FROM #TmpFixResRevReclass_Fixer
";
            }
        }

        public string RephaseCommandText
        {
            get
            {
                return
@"DECLARE @MaxActualDate date = (
    SELECT Max(TranDate) FROM CashFlow
    WHERE CashFlow.[Snapshot] = @Snapshot
    AND CashFlow.Status != @ForecastStatus
	AND CashFlow.GCRecord IS NULL
);

UPDATE cf SET TranDate = CASE
-- move AP payments forecast to next lockdown date
WHEN cf.Fix <> @PayrollFixTag 
	AND FixTag.FixTagType IN (@ScheduleOutFixTagType)
	AND cf.FixRank > 2 AND cf.TranDate <= @ApayableLockdownDate
THEN @ApayableNextLockdownDate
-- move business forecast to next week 
WHEN cf.Fix <> @PayrollFixTag 
	AND FixTag.FixTagType IN (@AllocateFixTagType)
	AND cf.FixRank > 2 AND cf.TranDate <= @MaxActualDate
THEN DATEADD(d, 7, @MaxActualDate)
-- adjust date of payroll payments
WHEN cf.Fix = @PayrollFixTag
	AND cf.FixRank > 2 AND cf.TranDate <= @PayrollLockdownDate
THEN @PayrollNextLockdownDate
-- move receipts to forecast period
WHEN FixTag.FixTagType = @ScheduleInFixTagType
    AND cf.FixRank > 2
    AND cf.TranDate <= @MaxActualDate
THEN DATEADD(d, 1, @MaxActualDate)
ELSE cf.TranDate
END
FROM CashFlow cf
LEFT JOIN CashForecastFixTag FixTag ON FixTag.Oid = cf.Fix
WHERE cf.[Status] = @ForecastStatus
AND cf.[Snapshot] = @Snapshot

-- delete everything else
UPDATE CashFlow SET
GCRecord = CAST(RAND() * 2147483646 + 1 AS INT),
Activity = NULL,
Account = NULL,
Counterparty = NULL,
Source = NULL,
CounterCcy = NULL
WHERE 
    Snapshot = @Snapshot
    AND TranDate <= @MaxActualDate
    AND [Status] = @ForecastStatus

";
            }
        }

        #endregion
    }
}
