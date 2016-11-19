IF OBJECT_ID('dbo.sp_cashflow_reval') IS NOT NULL
	BEGIN		
		DROP PROCEDURE dbo.sp_cashflow_reval;
	END;

GO

CREATE PROCEDURE [dbo].[sp_cashflow_reval] AS

/* Clean Up ------- */
IF OBJECT_ID('tempdb..#AccountTotal') IS NOT NULL
BEGIN
DROP TABLE #AccountTotal
END

IF OBJECT_ID('tempdb..#AccountTotalExt') IS NOT NULL
BEGIN
DROP TABLE #AccountTotalExt
END

IF OBJECT_ID('tempdb..#ForexRate') IS NOT NULL
BEGIN
DROP TABLE #ForexRate
END

IF OBJECT_ID('tempdb..#Valuation') IS NOT NULL
BEGIN
DROP TABLE #Valuation
END

/* Parameters ------- */
declare @ActualStatus int = 1
declare @ForecastStatus int = 0

DECLARE @FromDate date = (SELECT TOP 1 FromDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @ToDate date = (SELECT TOP 1 ToDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @Snapshot uniqueidentifier = (SELECT COALESCE(
	(SELECT TOP 1 [Snapshot] FROM CashFlowFixParam WHERE GCRecord IS NULL),
	(SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks WHERE GCRecord IS NULL)
))
DECLARE @DefaultCounterparty uniqueidentifier = (SELECT TOP 1 [Counterparty] FROM CashFlowDefaults WHERE GCRecord IS NULL)
DECLARE @FunctionalCurrency uniqueidentifier = (SELECT TOP 1 [FunctionalCurrency] FROM SetOfBooks WHERE GCRecord IS NULL)
DECLARE @RevalSource uniqueidentifier = (SELECT TOP 1 [FcaRevalCashFlowSource] FROM SetOfBooks WHERE GCRecord IS NULL)
DECLARE @UnrealFxActivity uniqueidentifier = (SELECT TOP 1 [UnrealFxActivity] FROM SetOfBooks WHERE GCRecord IS NULL)
DECLARE @LastActualDate datetime = (SELECT MAX(TranDate) FROM CashFlow WHERE [Snapshot] = @Snapshot AND [Status] = @ActualStatus AND GCRecord IS NULL)

/* Insert month end date to ensure revaluation occurs at the last day of each month */
INSERT INTO RevalDates (Oid, PeriodDate)
SELECT NEWID(), EOMONTH(@LastActualDate)
WHERE NOT EXISTS (SELECT * FROM RevalDates r WHERE r.PeriodDate = EOMONTH(@LastActualDate))

/* Delete existing revaluations ------- */
UPDATE CashFlow 
SET GCRecord = CAST(RAND() * 2147483646 + 1 AS INT)
WHERE TranDate BETWEEN @FromDate AND @ToDate 
	AND Source = @RevalSource
	AND [Snapshot] = @Snapshot
    AND GCRecord IS NULL

/* Create account total summary ------- */

SELECT
	cf.TranDate,
	cf.Account,
	SUM(COALESCE(cf.AccountCcyAmt, 0.00)) AS AccountCcyAmt,
	SUM(COALESCE(cf.FunctionalCcyAmt, 0.00)) AS FunctionalCcyAmt,
	Account.Currency AS Currency
INTO #AccountTotal
FROM CashFlow cf
JOIN Account ON Account.Oid = cf.Account
WHERE
	cf.[Snapshot] = @Snapshot
	AND Account.Currency <> @FunctionalCurrency
	AND cf.GCRecord IS NULL
GROUP BY cf.TranDate, cf.Account, Account.Currency

CREATE INDEX i_AccountTotal ON #AccountTotal (TranDate, Account)

/* Insert dates for revaluation in Account Total summary ------- */

INSERT INTO #AccountTotal (TranDate, Account, AccountCcyAmt, FunctionalCcyAmt, Currency)
SELECT DISTINCT
	r.PeriodDate,
	a1.Account,
	0.00 AS AccountCcyAmt,
	0.00 AS FunctionalCcyAmt,
	a1.Currency
FROM #AccountTotal a1
CROSS JOIN 
(
	SELECT r.PeriodDate FROM RevalDates r
	WHERE r.PeriodDate BETWEEN @FromDate AND @ToDate
) r
LEFT JOIN #AccountTotal a2 ON a2.TranDate =r.PeriodDate
	AND a2.Account = a1.Account
WHERE a2.Account IS NULL

/* Create Forex Rate Table ------- */

CREATE TABLE #ForexRate (Currency uniqueidentifier, TranDate datetime, FxDate datetime, Rate float)

INSERT INTO #ForexRate (Currency, TranDate)
SELECT a.Currency, a.TranDate
FROM #AccountTotal a
GROUP BY a.TranDate, a.Currency

UPDATE #ForexRate SET 
FxDate =
(
	SELECT MAX(fr.ConversionDate) FROM ForexRate fr
	WHERE fr.ConversionDate <= #ForexRate.TranDate
		AND fr.ToCurrency = #ForexRate.Currency
		AND fr.FromCurrency = @FunctionalCurrency
		AND fr.GCRecord IS NULL
)
FROM #ForexRate

UPDATE #ForexRate SET 
Rate =
(
	SELECT TOP 1 fr.ConversionRate FROM ForexRate fr
	WHERE fr.GCRecord IS NULL
		AND #ForexRate.FxDate = fr.ConversionDate
		AND fr.ToCurrency = #ForexRate.Currency
		AND fr.FromCurrency = @FunctionalCurrency
)

CREATE INDEX i_ForexRate ON #ForexRate (TranDate, Currency)

/* Balances -------------- */

SELECT 
	a1.TranDate,
	a1.Account,
	(
		SELECT SUM(a2.AccountCcyAmt) 
		FROM #AccountTotal a2
		WHERE a2.TranDate <= a1.TranDate AND a1.Account = a2.Account
	) AS AccountCcyAmt,
	(
		SELECT SUM(a2.FunctionalCcyAmt) 
		FROM #AccountTotal a2
		WHERE a2.TranDate <= a1.TranDate AND a1.Account = a2.Account
	) AS OldFunctionalCcyAmt,
	(
		SELECT SUM(a2.AccountCcyAmt) 
		FROM #AccountTotal a2
		WHERE a2.TranDate <= a1.TranDate AND a1.Account = a2.Account
	) / fr.Rate AS NewFunctionalCcyAmt,
	CAST(NULL AS float) AS DiffTotal,
	CAST(NULL AS float) AS DiffChange,
	CAST(NULL AS datetime) AS PrevTranDate
INTO #Valuation
FROM #AccountTotal a1
LEFT JOIN #ForexRate fr ON fr.Currency = a1.Currency AND fr.TranDate = a1.TranDate
WHERE a1.TranDate BETWEEN @FromDate AND @ToDate

UPDATE #Valuation SET
DiffTotal = NewFunctionalCcyAmt - OldFunctionalCcyAmt,
PrevTranDate = (SELECT Max(V2.TranDate) FROM #Valuation AS V2
WHERE V2.TranDate < #Valuation.TranDate AND V2.Account = #Valuation.Account)

UPDATE #Valuation SET DiffChange = COALESCE(
	DiffTotal - (
	SELECT DiffTotal FROM #Valuation AS V2
	WHERE V2.TranDate = #Valuation.PrevTranDate AND V2.Account = #Valuation.Account)
	, DiffTotal);

/* Upload */

INSERT INTO CashFlow (
	Oid, 
	[Snapshot], 
	Counterparty,
	TranDate, 
	Account,
	Activity, 
	AccountCcyAmt, 
	FunctionalCcyAmt,
	CounterCcyAmt, 
	CounterCcy, 
	[Source], 
	TimeEntered, 
	[Status],
    IsReclass
)
SELECT 
	NEWID() AS Oid, 
	@Snapshot AS [Snapshot],
	@DefaultCounterparty AS Counterparty,
	TranDate, 
	Account, 
	@UnrealFxActivity AS Activity, 
	0.00 AS AccountCcyAmt, 
	DiffChange AS FunctionalCcyAmt, 
	0.00 AS CounterCcyAmt, 
	@FunctionalCurrency AS CounterCcy, 
	@RevalSource AS [Source], 
	GETDATE() AS TimeEntered,
	CASE WHEN TranDate <= @LastActualDate THEN @ActualStatus
	ELSE @ForecastStatus END AS [Status],
    0 AS IsReclass
FROM #Valuation
WHERE DiffChange <> 0.00
