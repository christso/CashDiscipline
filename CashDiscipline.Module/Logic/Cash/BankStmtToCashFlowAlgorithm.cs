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
DECLARE @FromDate date = '2000-01-01'
DECLARE @ToDate date = '2100-12-31'
DECLARE @StmtSource uniqueidentifier = 
	(SELECT Oid FROM CashFlowSource WHERE Name LIKE 'Stmt')
DECLARE @Snapshot uniqueidentifier =
	(SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks)
DECLARE @ActualStatus int = 1
 */

namespace CashDiscipline.Module.Logic.Cash
{
    public class BankStmtToCashFlowAlgorithm
    {
        public BankStmtToCashFlowAlgorithm(XPObjectSpace objSpace, DailyCashUpdateParam paramObj)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;

            if (paramObj.FromDate <= SqlDateTime.MinValue.Value)
                this.fromDate = SqlDateTime.MinValue.Value;
            else
                this.fromDate = paramObj.FromDate;

            if (paramObj.ToDate <= SqlDateTime.MinValue.Value)
                this.toDate = SqlDateTime.MaxValue.Value;
            else
                this.toDate = paramObj.ToDate;
        }

        private XPObjectSpace objSpace;
        private DailyCashUpdateParam paramObj;
        private DateTime fromDate;
        private DateTime toDate;

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
                new SqlDeclareClause("FromDate", "date", string.Format("'{0}'", fromDate.ToString("yyyy-MM-dd"))),
                new SqlDeclareClause("ToDate", "date", string.Format("'{0}'", toDate.ToString("yyyy-MM-dd")))
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
@"DECLARE @StmtSource uniqueidentifier = 
	(SELECT Oid FROM CashFlowSource WHERE Name LIKE 'Stmt')
DECLARE @ActualStatus int = 1

-- Rank Bank Stmt lines
IF OBJECT_ID('temp_BankStmt') IS NOT NULL DROP TABLE temp_BankStmt
SELECT 
	bs.Oid,
	bs.TranDate,
	bs.Account,
	bs.Activity,
	bs.Counterparty,
	bs.TranAmount,
	bs.SummaryDescription,
	RANK() OVER (
		ORDER BY
			bs.TranDate,
			bs.Account,
			bs.Activity,
			bs.Counterparty,
			bs.SummaryDescription,
			bs.CounterCcy,
			bs.ForexSettleType
	) AS RowId,
	bs.CounterCcyAmt,
	bs.CounterCcy,
	bs.FunctionalCcyAmt,
	bs.ForexSettleType,
	bs.Oid AS CashFlow
INTO temp_BankStmt
FROM BankStmt bs
ORDER BY 
	bs.TranDate,
	bs.Account,
	bs.Activity,
	bs.Counterparty,
	bs.SummaryDescription,
	bs.CounterCcy,
	bs.ForexSettleType;

-- Generate CashFlow GUIDs
UPDATE bs1
SET CashFlow = bs2.CashFlow
FROM
temp_BankStmt bs1 
LEFT JOIN 
(
	SELECT DISTINCT
		RowId,
		CAST(CAST(NEWID() AS BINARY(10)) + CAST(GETDATE() AS BINARY(6)) AS UNIQUEIDENTIFIER) AS CashFlow
	FROM temp_BankStmt bs
) bs2 ON bs2.RowId = bs1.RowId

-- Delete Existing Cash Flow

UPDATE CashFlow SET
GCRecord = CAST(RAND() * 2147483646 + 1 AS INT)
WHERE TranDate BETWEEN @FromDate AND @ToDate;

-- Upload Bank Stmt to Cash Flow

INSERT INTO CashFlow ( [Snapshot], [Oid], [TranDate], [Account], [Activity],
	[Counterparty], AccountCcyAmt, [Description], [Source], [FunctionalCcyAmt],
	[CounterCcyAmt], [CounterCcy], [ForexSettleType], [TimeEntered] )
SELECT
	@Snapshot AS [Snapshot],
	bs.CashFlow AS Oid,
	bs.TranDate,
	bs.Account,
	bs.Activity,
	bs.Counterparty,
	SUM(bs.TranAmount) AS AccountCcyAmt,
	bs.SummaryDescription AS [Description],
	@StmtSource AS [Source],
	SUM(bs.FunctionalCcyAmt) AS FunctionalCcyAmt,
	SUM(bs.CounterCcyAmt) AS CounterCcyAmt,
	bs.CounterCcy,
	bs.ForexSettleType,
	CURRENT_TIMESTAMP AS TimeEntered
FROM temp_BankStmt bs
GROUP BY
	bs.TranDate,
	bs.Account,
	bs.Activity,
	bs.Counterparty,
	bs.SummaryDescription,
	bs.CounterCcy,
	bs.ForexSettleType,
	bs.CashFlow

-- Update Bank Stmt Cash Flow Oids

UPDATE bs0 SET
CashFlow = bs1.CashFlow
FROM BankStmt bs0
JOIN temp_BankStmt bs1 ON bs1.Oid = bs0.Oid";
            }
        }
        #endregion

    }
}
