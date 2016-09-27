using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
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
*/

namespace CashDiscipline.Module.Logic.Cash
{
    public class RevalueAccounts
    {
        private readonly XPObjectSpace objSpace;
        private CashFlowFixParam paramObj;
        public RevalueAccounts(XPObjectSpace objSpace, CashFlowFixParam paramObj)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;
        }

        public string ProcessCommandText

        {
            get
            {
                return
@"DECLARE @FromDate date = (SELECT TOP 1 FromDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @ToDate date = (SELECT TOP 1 ToDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @Snapshot uniqueidentifier = (SELECT COALESCE(
	(SELECT TOP 1 [Snapshot] FROM CashFlowFixParam WHERE GCRecord IS NULL),
	(SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks WHERE GCRecord IS NULL)
))
DECLARE @DefaultCounterparty uniqueidentifier = (SELECT TOP 1 [Counterparty] FROM CashFlowDefaults WHERE GCRecord IS NULL)
DECLARE @FunctionalCurrency uniqueidentifier = (SELECT TOP 1 [FunctionalCurrency] FROM SetOfBooks WHERE GCRecord IS NULL)
DECLARE @RevalSource uniqueidentifier = (SELECT TOP 1 [FcaRevalCashFlowSource] FROM SetOfBooks WHERE GCRecord IS NULL)
DECLARE @UnrealFxActivity uniqueidentifier = (SELECT TOP 1 [UnrealFxActivity] FROM SetOfBooks WHERE GCRecord IS NULL)

/* Delete existing revaluations ------- */
DELETE FROM CashFlow 
WHERE [Snapshot] = @Snapshot
	AND TranDate BETWEEN @FromDate AND @ToDate
	AND Source = @RevalSource

/* Create account total summary ------- */

SELECT
	cf.TranDate,
	cf.Account,
	SUM(cf.AccountCcyAmt) AS AccountCcyAmt,
	SUM(cf.FunctionalCcyAmt) AS FunctionalCcyAmt
INTO #AccountTotal
FROM CashFlow cf
WHERE
	cf.[Snapshot] = @Snapshot
	AND cf.Account NOT IN 
	(
		SELECT a.Oid FROM Account a
		WHERE a.Currency LIKE 'AUD'
	)
GROUP BY cf.TranDate, cf.Account

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
	SELECT fr.ConversionRate FROM ForexRate fr
	WHERE fr.GCRecord IS NULL
		AND #ForexRate.FxDate = fr.ConversionDate
		AND fr.ToCurrency = #ForexRate.Currency
		AND fr.FromCurrency = @FunctionalCurrency
)

/*
SELECT #ForexRate.*, Currency.Name AS Currency FROM #ForexRate
LEFT JOIN Currency ON Currency.Oid = #ForexRate.Currency
ORDER BY Currency.Name
*/

/* Balances -------------- */

SELECT 
	a1.TranDate,
	a1.Account,
	SUM(a2.AccountCcyAmt) AS AccountCcyAmt,
	SUM(a2.FunctionalCcyAmt) AS OldFunctionalCcyAmt,
	fr.Rate,
	SUM(a2.AccountCcyAmt) / fr.Rate AS NewFunctionalCcyAmt,
	CAST(NULL AS float) AS DiffTotal,
	CAST(NULL AS float) AS DiffChange,
	CAST(NULL AS datetime) AS PrevTranDate
INTO #Valuation
FROM #AccountTotalExt a1
LEFT JOIN #AccountTotal a2 ON a2.TranDate <= a1.TranDate AND a1.Account = a2.Account
LEFT JOIN #ForexRate fr ON fr.Currency = a1.Currency AND fr.TranDate = a1.TranDate
GROUP BY
	a1.TranDate,
	a1.Account,
	fr.Rate
HAVING ROUND ( SUM(a2.FunctionalCcyAmt), 2 ) <> 0.00;

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
	Oid, [Snapshot], Counterparty,
	TranDate, Account, Activity, AccountCcyAmt, FunctionalCcyAmt,
	CounterCcyAmt, CounterCcy, Source, TimeEntered)
SELECT NEWID(), @Snapshot, @DefaultCounterparty,
	TranDate, Account, @UnrealFxActivity, 
	0.00, DiffChange, 0.00, @FunctionalCurrency, 
	@RevalSource AS Source, GETDATE()
FROM #Valuation
WHERE DiffChange <> 0.00"
;
            }
        }

        public void Process()
        {
            var clauses = CreateSqlParameters();
            var parameters = CreateParameters(clauses);

            using (var cmd = ((SqlConnection)objSpace.Session.Connection).CreateCommand())
            {
                cmd.Parameters.AddRange(parameters.ToArray());
                cmd.CommandText = ProcessCommandText;
                cmd.ExecuteNonQuery();
            }
        }

        public List<SqlDeclareClause> CreateSqlParameters()
        {
            var clauses = new List<SqlDeclareClause>()
            {
                //new SqlDeclareClause("FromDate", "date", "(SELECT TOP 1 FromDate FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
            };
            return clauses;
        }

        public List<SqlParameter> CreateParameters(List<SqlDeclareClause> clauses)
        {
            var parameters = new List<SqlParameter>();
            using (var cmd = objSpace.Session.Connection.CreateCommand())
            {
                foreach (var clause in clauses)
                {
                    parameters.Add(new SqlParameter(clause.ParameterName, clause.ExecuteScalar(cmd)));
                }
            }
            return parameters;
        }
    }
}
