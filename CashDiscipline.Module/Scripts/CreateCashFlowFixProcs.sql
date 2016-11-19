IF OBJECT_ID('dbo.sp_cashflow_fix') IS NOT NULL
	BEGIN		
		DROP PROCEDURE dbo.sp_cashflow_fix;
	END;

GO

CREATE PROCEDURE [dbo].[sp_cashflow_fix] AS
DECLARE @FromDate date = (SELECT TOP 1 FromDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @ToDate date = (SELECT TOP 1 ToDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @ApayableLockdownDate date = (SELECT TOP 1 ApayableLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @ApayableNextLockdownDate date = (SELECT TOP 1 ApayableNextLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
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
DECLARE @DlrComFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'DC' AND GCRecord IS NULL)
DECLARE @AutoFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'Auto' AND GCRecord IS NULL)
DECLARE @ApReclassActivity uniqueidentifier = (SELECT TOP 1 ApReclassActivity FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @PayrollLockdownDate date = (SELECT TOP 1 PayrollLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @PayrollNextLockdownDate date = (SELECT TOP 1 PayrollNextLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @AutoForexSettleType int = 0


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
    [Counterparty] = fixer.Counterparty,

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
WHERE cf.Snapshot = @Snapshot
AND cf.GCRecord IS NULL

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
IsReclass = 0,
TimeEntered = GETDATE()
FROM #TmpFixResRevReclass_Fixer frrr
LEFT JOIN Activity a1 ON a1.Oid = frrr.Activity
;

-- Reset IsReclass Flag
UPDATE cf
SET IsReclass = 0
FROM CashFlow cf
WHERE cf.Snapshot = @Snapshot
AND cf.GCRecord IS NULL
AND cf.IsReclass = 1

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

GO


