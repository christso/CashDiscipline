IF OBJECT_ID('dbo.sp_dwh_CashFlow') IS NOT NULL
	BEGIN		
		DROP PROCEDURE dbo.sp_dwh_CashFlow;
	END;

GO

CREATE PROCEDURE [dbo].[sp_dwh_CashFlow] AS

DECLARE @DefActivity uniqueidentifier = (SELECT TOP 1 Activity FROM CashReportConfig WHERE GCRecord IS NULL);
DECLARE @DefCounterparty uniqueidentifier = (SELECT TOP 1 Counterparty FROM CashFlowDefaults WHERE GCRecord IS NULL);
DECLARE @FunctionalCurrency uniqueidentifier = (SELECT TOP 1 FunctionalCurrency FROM SetOfBooks WHERE GCRecord IS NULL);
DECLARE @StartDate datetime = (SELECT TOP 1 StartDate FROM CashReportConfig WHERE GCRecord IS NULL);
DECLARE @ActualStatus int = 1;

SELECT 
	cf.[Oid] AS [Cash Flow OID]
	,cf.[Snapshot] AS [Snapshot OID]
	,CAST(cf.[TranDate] AS Date) AS [Tran Date]
	,(SELECT a.Name FROM Account a WHERE a.Oid = cf.Account) AS [Account]
	,(SELECT a.Name FROM Activity a WHERE a.Oid = cf.Activity) AS Activity
	,cf.[Counterparty] AS [Counterparty OID]
	,cf.[AccountCcyAmt] AS [Account Ccy Amt]
	,cf.[FunctionalCcyAmt] AS [Functional Ccy Amt]
	,cf.[CounterCcyAmt] AS [Counter Ccy Amt]
	,(SELECT ccy.Name FROM Currency ccy WHERE ccy.Oid = cf.CounterCcy) AS [Counter Ccy]
	,cf.[Description]
	,src.Name AS [Source]
	,CASE WHEN cf.[Status] = 0 OR cf.[Status] IS NULL THEN 'Forecast' ELSE 'Actual' END AS [Status]
	,(SELECT t1.Name FROM CashForecastFixTag t1 WHERE t1.Oid = cf.[Fix]) AS [Fix]
	,cf.FixRank AS [Fix Rank]
	,cf.[IsReclass] AS [Is Reclass]
	,(SELECT a.Name FROM Activity a WHERE a.Oid = cf.[FixActivity]) AS [Fix Activity]
	,cf.DateUnfix AS [Date Unfix]
	,cf.FixFromDate AS [Fix From Date]
	,cf.FixToDate AS [Fix To Date]
	,COALESCE(pcf.TranDate, cf.TranDate) AS [P Tran Date]
	,COALESCE(pacty.Name, acty.Name) AS [P Activity]
	,COALESCE (psrc.Name, src.Name) AS [P Source]
FROM (

SELECT
cf.[Oid],
cf.[Snapshot],
cf.TranDate,
cf.[Account],
cf.[Activity],
cf.[Counterparty],
cf.[AccountCcyAmt],
cf.[FunctionalCcyAmt],
cf.[CounterCcyAmt],
cf.CounterCcy,
cf.[Description],
cf.[Source],
cf.[Status],
cf.Fix,
cf.FixRank,
cf.[IsReclass],
cf.[FixActivity],
cf.DateUnfix,
cf.FixFromDate,
cf.FixToDate,
cf.ParentCashFlow
FROM CashFlow cf
WHERE 
cf.GCRecord IS NULL 
AND EXISTS (
	SELECT * FROM CashSnapshotReported s1
	WHERE 
	s1.[CurrentSnapshot] = cf.[Snapshot]
	AND s1.IsEnabled = 1
	AND s1.GCRecord IS NULL

	UNION ALL

	SELECT * FROM CashSnapshotReported s1
	WHERE 
	s1.[PreviousSnapshot] = cf.[Snapshot]
	AND s1.IsEnabled = 1
	AND s1.GCRecord IS NULL
)
AND cf.TranDate >= @StartDate

UNION ALL

SELECT
ab.Oid,
ab.Snapshot,
ab.TranDate,
ab.[Account],
@DefActivity AS [Activity],
@DefCounterparty AS [Counterparty],
ab.[AccountCcyAmt],
ab.[FunctionalCcyAmt],
ab.[AccountCcyAmt] AS [CounterCcyAmt],
@FunctionalCurrency AS CounterCcy,
'OPENING BALANCE' AS [Description],
NULL AS [Source],
@ActualStatus AS [Status],
NULL AS Fix,
0 AS FixRank,
0 AS [IsReclass],
@DefActivity AS [FixActivity],
NULL AS DateUnfix,
NULL AS FixFromDate,
NULL AS FixToDate,
NULL AS ParentCashFlow
FROM AccountBalance ab
WHERE ab.TranDate = DATEADD(d, -1, @StartDate)
) cf
LEFT JOIN [CashFlow] pcf ON pcf.Oid = cf.ParentCashFlow
LEFT JOIN [CashFlowSource] src ON cf.Source = src.Oid
LEFT JOIN [CashFlowSource] psrc ON pcf.Source = psrc.Oid
LEFT JOIN Activity acty ON cf.Activity = acty.Oid
LEFT JOIN Activity pacty ON cf.Activity = pacty.Oid