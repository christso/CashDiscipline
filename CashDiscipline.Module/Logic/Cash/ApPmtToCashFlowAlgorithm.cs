using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Logic;
using CashDiscipline.Module.Logic.Cash;
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
DECLARE @Snapshot uniqueidentifier =
	(SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks)
DECLARE @FromDate date = '2016-08-18'
DECLARE @ToDate date = '2016-08-18'
DECLARE @StmtSource uniqueidentifier = 
	(SELECT Oid FROM CashFlowSource WHERE Name LIKE 'Stmt')
DECLARE @ActualStatus int = 1
 */

namespace CashDiscipline.Module.Logic.Cash
{
    public class ApPmtToCashFlowAlgorithm
    {
        public ApPmtToCashFlowAlgorithm(XPObjectSpace objSpace)
        {
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
                cmd.CommandText = ParameterCommandText + "\n\n" + ProcessCommandText;
                cmd.ExecuteNonQuery();
            }
        }

        public List<SqlDeclareClause> CreateSqlParameters()
        {
            var clauses = new List<SqlDeclareClause>()
            {
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

        public string ParameterCommandText
        {
            get
            {
                return
@"DECLARE @Snapshot uniqueidentifier =
	(SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks)
DECLARE @FromDate date = (SELECT TOP 1 FromDate FROM DailyCashUpdateParam WHERE GCRecord IS NULL)
DECLARE @ToDate date = (SELECT TOP 1 ToDate FROM DailyCashUpdateParam WHERE GCRecord IS NULL)
DECLARE @ApReclassSource uniqueidentifier = 
	(SELECT Oid FROM CashFlowSource WHERE Name LIKE 'AP-Reclass')
DECLARE @ActualStatus int = 1
DECLARE @FunctionalCcy uniqueidentifier = (SELECT TOP 1 FunctionalCurrency FROM SetOfBooks WHERE GCRecord IS NULL)
DECLARE @ApReclassActivity uniqueidentifier = (SELECT TOP 1 ApReclassActivity FROM DailyCashUpdateParam WHERE GCRecord IS NULL)
DECLARE @ApCounterparty uniqueidentifier = (SELECT TOP 1 Counterparty FROM DailyCashUpdateParam WHERE GCRecord IS NULL)
DECLARE @ForexSettleType int = 5 --OutReclass
";
            }
        }

        public string ProcessCommandText
        {
            get
            {
                return
@"-- Rank ApPmtDistn lines
IF OBJECT_ID('tempdb..#TmpApPmtDistn') IS NOT NULL DROP TABLE #TmpApPmtDistn
SELECT 
	ap.Oid,
	ap.PaymentDate AS TranDate,
	ap.Account,
	ap.Activity,
	ap.Counterparty,
	CASE WHEN acct.Currency = @FunctionalCcy THEN ap.PaymentAmountAud
		ELSE ap.PaymentAmountFx END AS AccountCcyAmt,
	ap.SummaryDescription AS [Description],
	RANK() OVER (
		ORDER BY
			ap.PaymentDate,
			ap.Account,
			ap.Activity,
			ap.Counterparty,
			ap.SummaryDescription,
			ap.PaymentCurrency
	) AS RowId,
	ap.PaymentAmountFx AS CounterCcyAmt,
	ap.InvoiceCurrency AS CounterCcy,
	ap.PaymentAmountAud AS [FunctionalCcyAmt],
	ap.Oid AS CashFlow
INTO #TmpApPmtDistn
FROM 
(
	SELECT
		ap.Oid,
		ap.PaymentDate,
		ap.Account,
		ap.Activity,
		ap.Counterparty,
		- ap.PaymentAmountFx AS PaymentAmountFx,
		ap.InvoiceCurrency,
		- ap.PaymentAmountAud AS PaymentAmountAud,
		ap.PaymentCurrency,
		COALESCE(ap.SummaryDescription,'') AS SummaryDescription
	FROM ApPmtDistn ap
	WHERE ap.PaymentDate BETWEEN @FromDate AND @ToDate
	AND ap.GCRecord IS NULL

	UNION ALL

	SELECT
		ap.Oid,
		ap.PaymentDate,
		ap.Account,
		@ApReclassActivity,
		@ApCounterparty,
		ap.PaymentAmountFx AS PaymentAmountFx,
		ap.InvoiceCurrency,
		ap.PaymentAmountAud AS PaymentAmountAud,
		ap.PaymentCurrency,
		'' AS SummaryDescription
	FROM ApPmtDistn ap
	WHERE ap.PaymentDate BETWEEN @FromDate AND @ToDate
	AND ap.GCRecord IS NULL
) ap
LEFT JOIN Account acct ON acct.Oid = ap.Account AND acct.GCRecord IS NULL
ORDER BY 
	ap.PaymentDate,
	ap.Account,
	ap.Activity,
	ap.Counterparty,
	ap.SummaryDescription,
	ap.PaymentCurrency

-- Generate CashFlow GUIDs for each temporary ApPmtDistn
UPDATE ap1
SET CashFlow = ap2.CashFlow
FROM
#TmpApPmtDistn ap1 
LEFT JOIN 
(
	SELECT 
		apRows.RowId,
		CAST(CAST(NEWID() AS BINARY(10)) + CAST(GETDATE() AS BINARY(6)) AS UNIQUEIDENTIFIER) AS CashFlow
	FROM
	(
		SELECT DISTINCT
			RowId
		FROM #TmpApPmtDistn
	) apRows
) ap2 ON ap2.RowId = ap1.RowId

-- Delete Existing Cash Flow from current snapshot

UPDATE CashFlow SET
GCRecord = CAST(RAND() * 2147483646 + 1 AS INT),
Activity = NULL,
Account = NULL,
Counterparty = NULL,
Source = NULL,
CounterCcy = NULL
WHERE 
    [Snapshot] = @Snapshot
    AND TranDate BETWEEN @FromDate AND @ToDate
	AND [Source] = @ApReclassSource
	AND GCRecord IS NULL

-- Upload APPmtDistn to Cash Flow

INSERT INTO CashFlow ( [Snapshot], [Oid], [TranDate], [Account], [Activity],
	[Counterparty], AccountCcyAmt, [Description], [Source], [FunctionalCcyAmt],
	[CounterCcyAmt], [CounterCcy], [ForexSettleType], [TimeEntered], [Status] )
SELECT
	@Snapshot AS [Snapshot],
	ap.CashFlow AS Oid,
	ap.TranDate,
	ap.Account,
	ap.Activity,
	ap.Counterparty,
	SUM(ap.AccountCcyAmt) AS AccountCcyAmt,
	ap.[Description],
	@ApReclassSource AS [Source],
	SUM(ap.FunctionalCcyAmt) AS FunctionalCcyAmt,
	SUM(ap.CounterCcyAmt) AS CounterCcyAmt,
	ap.CounterCcy,
	@ForexSettleType,
	CURRENT_TIMESTAMP AS TimeEntered,
    @ActualStatus AS [Status]
FROM #TmpApPmtDistn ap
GROUP BY
	ap.TranDate,
	ap.Account,
	ap.Activity,
	ap.Counterparty,
	ap.[Description],
	ap.CounterCcy,
	ap.CashFlow

-- Update ApPmtDistn Cash Flow Oids

UPDATE ap0 SET
CashFlow = ap1.CashFlow
FROM ApPmtDistn ap0
JOIN #TmpApPmtDistn ap1 ON ap1.Oid = ap0.Oid";
            }
        }
        #endregion

    }
}
