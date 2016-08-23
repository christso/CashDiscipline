using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Logic;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
IF OBJECT_ID('tempdb..#TmpForexTrade') IS NOT NULL DROP TABLE tempdb..#TmpForexTrade
DECLARE @ActualStatus int = 1
DECLARE @Snapshot uniqueidentifier = (SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks WHERE GCRecord IS NULL)


*/

namespace CashDiscipline.Module.Controllers.Forex
{
    public class ForexToCashFlowUploader
    {
        public ForexToCashFlowUploader(XPObjectSpace objSpace)
        {
            if (objSpace == null)
                throw new ArgumentException("objectSpace");
            this.objSpace = objSpace;

        }

        private XPObjectSpace objSpace;

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
                new SqlDeclareClause("Snapshot", "uniqueidentifier",
                @"(SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("ActualStatus", "int", Convert.ToInt32(CashFlowStatus.Actual).ToString()),
                new SqlDeclareClause("ForecastStatus", "int", Convert.ToInt32(CashFlowStatus.Forecast).ToString()),
                new SqlDeclareClause("Activity", "uniqueidentifier", "(SELECT TOP 1 [ForexSettleActivity] FROM SetOfBooks WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("Source", "uniqueidentifier", "(SELECT TOP 1 [ForexSettleCashFlowSource] FROM SetOfBooks WHERE GCRecord IS NULL)")
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

        #region SQL

        public string ProcessCommandText
        {
            get
            {
                return
@"DECLARE @MaxActualDate datetime = (
SELECT COALESCE(MAX(TranDate),'1973-01-01') FROM CashFlow
WHERE Snapshot = @Snapshot
AND Status = @ActualStatus)
-- Rank Forex Trades

SELECT
tft.*,
	RANK() OVER (
		ORDER BY
			tft.TranDate,
			tft.CounterCcy,
			tft.Account,
			tft.Counterparty,
            tft.SettleGroupId
	) AS RowId
INTO #TmpForexTrade
FROM
(
	SELECT 
		ft.Oid,
		ft.PrimarySettleDate AS TranDate,
		ft.CounterCcy,
		ft.PrimarySettleAccount AS Account,
		(SELECT c2.CashFlowCounterparty FROM ForexCounterparty c2 WHERE c2.Oid = ft.Counterparty) AS Counterparty,
		-ft.PrimaryCcyAmt AS AccountCcyAmt,
		-ft.CounterCcyAmt AS CounterCcyAmt,
		-ft.PrimaryCcyAmt AS FunctionalCcyAmt,
		(SELECT c.Name FROM ForexCounterparty c WHERE c.Oid = ft.Counterparty) 
			+ ' ' + (SELECT c.Name FROM Currency c WHERE c.Oid = ft.PrimaryCcy)
			+ '-' + (SELECT c.Name FROM Currency c WHERE c.Oid = ft.CounterCcy)
		AS [Description],
		ft.PrimaryCashFlow AS CashFlow,
        ft.SettleGroupId,
        1 AS IsPrimary
	FROM ForexTrade ft
	WHERE GcRecord IS NULL
		AND ft.PrimarySettleDate >= @MaxActualDate
	UNION ALL
	SELECT 
		ft.Oid,
		ft.CounterSettleDate AS TranDate,
		ft.CounterCcy,
		ft.CounterSettleAccount AS Account,
		(SELECT c2.CashFlowCounterparty FROM ForexCounterparty c2 WHERE c2.Oid = ft.Counterparty) AS Counterparty,
		ft.CounterCcyAmt AS AccountCcyAmt,
		ft.CounterCcyAmt AS CounterCcyAmt,
		ft.PrimaryCcyAmt AS FunctionalCcyAmt,
		(SELECT c.Name FROM ForexCounterparty c WHERE c.Oid = ft.Counterparty) 
			+ ' ' + (SELECT c.Name FROM Currency c WHERE c.Oid = ft.PrimaryCcy)
			+ '-' + (SELECT c.Name FROM Currency c WHERE c.Oid = ft.CounterCcy)
		AS [Description],
		ft.CounterCashFlow AS CashFlow,
        ft.SettleGroupId,
        0 AS IsPrimary
	FROM ForexTrade ft
	WHERE GcRecord IS NULL
		AND ft.CounterSettleDate >= @MaxActualDate
) tft

-- Generate CashFlow GUIDs for each temporary BankStmt
UPDATE tft1
SET CashFlow = tft2.CashFlow
FROM #TmpForexTrade tft1
LEFT JOIN (
	SELECT DISTINCT
		RowId,
		CAST(CAST(NEWID() AS BINARY(10)) + CAST(GETDATE() AS BINARY(6)) AS UNIQUEIDENTIFIER) AS CashFlow
	FROM #TmpForexTrade
) tft2 ON tft2.RowId = tft1.RowId

-- Delete Existing Cash Flow from current snapshot

UPDATE CashFlow SET
GCRecord = CAST(RAND() * 2147483646 + 1 AS INT),
Activity = NULL,
Account = NULL,
Counterparty = NULL,
Source = NULL,
CounterCcy = NULL
WHERE 
    Snapshot = @Snapshot
    AND TranDate >= @MaxActualDate
	AND Source = @Source
	AND [Status] = @ForecastStatus
	AND GCRecord IS NULL

-- Upload ForexTrade to Cash Flow

INSERT INTO CashFlow ( [Snapshot], [Oid], [TranDate], [Account], [Activity],
	[Counterparty], AccountCcyAmt, [Description], [Source], [FunctionalCcyAmt],
	[CounterCcyAmt], [CounterCcy], [TimeEntered], [Status] )
SELECT
	@Snapshot AS [Snapshot],
	tft.CashFlow AS Oid,
	tft.TranDate,
	tft.Account,
	@Activity AS Activity,
	tft.Counterparty,
	SUM(tft.AccountCcyAmt) AS AccountCcyAmt,
	tft.[Description],
	@Source AS [Source],
	SUM(tft.FunctionalCcyAmt) AS FunctionalCcyAmt,
	SUM(tft.CounterCcyAmt) AS CounterCcyAmt,
	tft.CounterCcy,
	CURRENT_TIMESTAMP AS TimeEntered,
    @ForecastStatus AS Status
FROM #TmpForexTrade tft
GROUP BY
	tft.CashFlow,
	tft.TranDate,
	tft.Account,
	tft.Counterparty,
	tft.[Description],
	tft.CounterCcy

-- Update Bank Stmt Cash Flow Oids

UPDATE ft SET
ft.PrimaryCashFlow = tft.CashFlow
FROM ForexTrade ft
JOIN #TmpForexTrade tft ON ft.Oid = tft.Oid
WHERE tft.IsPrimary=1;

UPDATE ft SET
ft.CounterCashFlow = tft.CashFlow
FROM ForexTrade ft
JOIN #TmpForexTrade tft ON ft.Oid = tft.Oid
WHERE tft.IsPrimary=0;";
            }
        }
        #endregion

    }
}
