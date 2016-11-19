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

/* Delete existing revaluations ------- */
UPDATE CashFlow 
SET GCRecord = CAST(RAND() * 2147483646 + 1 AS INT)
WHERE [Snapshot] = @Snapshot
	AND TranDate BETWEEN @FromDate AND @ToDate
	AND Source = @RevalSource
    AND GCRecord IS NULL

/* Create account total summary ------- */

SELECT
	cf.TranDate,
	cf.Account,
	SUM(cf.AccountCcyAmt) AS AccountCcyAmt,
	SUM(cf.FunctionalCcyAmt) AS FunctionalCcyAmt
INTO #AccountTotal
FROM CashFlow cf
WHERE
    cf.GCRecord IS NULL
	AND cf.[Snapshot] = @Snapshot
	AND cf.Account NOT IN 
	(
		SELECT a.Oid FROM Account a
		WHERE a.Currency LIKE @FunctionalCurrency
        AND a.GCRecord IS NULL
	)
GROUP BY cf.TranDate, cf.Account

CREATE INDEX i_AccountTotal ON #AccountTotal (TranDate, Account)

/* Transform account total summary to include all dates ------- */
SELECT
	dates.TranDate,
	accts.Account,
	COALESCE(totals.AccountCcyAmt, 0.00) AS AccountCcyAmt,
	COALESCE(totals.FunctionalCcyAmt, 0.00) AS FunctionalCcyAmt,
	Currency.Oid AS Currency
INTO #AccountTotalExt
FROM
(
	SELECT DISTINCT
		d.TranDate
	FROM #AccountTotal d
) dates
CROSS JOIN
(
	SELECT DISTINCT
		a.Account
	FROM #AccountTotal a
) accts
LEFT JOIN #AccountTotal totals ON totals.TranDate = dates.TranDate
	AND totals.Account = accts.Account
LEFT JOIN Account ON Account.Oid = accts.Account
LEFT JOIN Currency ON Currency.Oid = Account.Currency;

CREATE INDEX i_AccountTotalExt ON #AccountTotalExt (TranDate, Account)

/* Create Forex Rate Table ------- */

CREATE TABLE #ForexRate (Currency uniqueidentifier, TranDate datetime, FxDate datetime, Rate float)

INSERT INTO #ForexRate (Currency, TranDate)
SELECT a.Currency, a.TranDate
FROM #AccountTotalExt a
GROUP BY a.TranDate, a.Currency

UPDATE #ForexRate SET 
FxDate =
(
	SELECT MAX(fr.ConversionDate) FROM ForexRate fr
	WHERE fr.GCRecord IS NULL
		AND fr.ConversionDate <= #ForexRate.TranDate
		AND fr.ToCurrency = #ForexRate.Currency
		AND fr.FromCurrency = @FunctionalCurrency
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
FROM #AccountTotalExt a1
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
GO


