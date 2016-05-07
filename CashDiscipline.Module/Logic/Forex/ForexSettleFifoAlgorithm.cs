using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.Logic.Cash;
using CashDiscipline.Module.ParamObjects.Forex;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlTypes;

/* Parameters in SQL
DECLARE @Snapshot uniqueidentifier = (SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks)
DECLARE @InSettleType int = 1
DECLARE @OutSettleType int = 2
DECLARE @OutReclassSettleType int = 4
DECLARE @FromDate date = '1970-01-01'
DECLARE @ToDate date = '2199-12-31'
*/

namespace CashDiscipline.Module.Logic.Forex
{
    public class ForexSettleFifoAlgorithm
    {

        public ForexSettleFifoAlgorithm(XPObjectSpace objSpace, ForexSettleFifoParam paramObj)
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

            this.currentSnapshot = CashFlowHelper.GetCurrentSnapshot(objSpace.Session);
        }

        #region SQL

        public string ProcessFifoCommandText
        {
            get
            {
                return
@"-- Pre-validation

DECLARE @IsValid int = 1- COALESCE (
( SELECT 1 WHERE EXISTS ( SELECT * FROM CashFlow
WHERE GCRecord IS NULL
	AND ForexSettleType = @OutSettleType
	AND AccountCcyAmt > 0 )
), 0)

IF @IsValid = 0
	RAISERROR('CashFlow with SettleType = Out cannot be a positve number',16,1)

ELSE

BEGIN

-- FifoCashFlow

IF OBJECT_ID('temp_FifoCashFlow') IS NOT NULL DROP TABLE temp_FifoCashFlow;

SELECT *
INTO temp_FifoCashFlow
FROM
(
SELECT
	ROW_NUMBER() OVER (ORDER BY CashFlow.TranDate, CashFlow.Oid) AS RowId,
	CashFlow.Account,
	CashFlow.Oid,
	CashFlow.TranDate,
	-- <Unlinked Amount>
	-- Original Cash Flow Amount
	CashFlow.AccountCcyAmt 
	-- Linked Inflow Amount
	- COALESCE ( (
		SELECT SUM (fsl.AccountCcyAmt )
		FROM ForexSettleLink fsl
		WHERE fsl.CashFlowIn = CashFlow.Oid
	) , 0 )
	+
	-- Linked Outflow Amount
	COALESCE ( (
		SELECT SUM (fsl.AccountCcyAmt )
		FROM ForexSettleLink fsl
		WHERE fsl.CashFlowOut = CashFlow.Oid
	), 0 ) AS Amount,
	-- </Unlinked Amount>
	CashFlow.ForexSettleType,
	CAST ( 0.00 AS money ) AS IoBalance
FROM CashFlow
WHERE GCRecord IS NULL
	AND [Snapshot] = @Snapshot
	AND TranDate BETWEEN @FromDate AND @ToDate
	AND ForexSettleType IN (@InSettleType, @OutSettleType)
) cf
WHERE cf.Amount != 0

UPDATE cf1 SET
IoBalance =
(
	SELECT SUM(cf2.Amount)
	FROM temp_FifoCashFlow cf2
	WHERE 
		cf2.RowId <= cf1.RowId
		AND cf2.Account = cf1.Account
		AND cf2.ForexSettleType = cf1.ForexSettleType
)
FROM temp_FifoCashFlow cf1

-- InflowOutflow

IF OBJECT_ID('temp_InflowOutflow') IS NOT NULL DROP TABLE temp_InflowOutflow;

SELECT
	cfIn.Account,
	cfIn.Oid,
	cfIn.Amount,
	cfIn.IoBalance,
	cfOut.Oid AS Out_Oid,
	-cfOut.Amount AS Out_Amount,
	-cfOut.IoBalance AS Out_IoBalance,

	/* Minimum ( LinkDemand, LinkSupply)
	*/
	( 
		SELECT MIN(T1.LinkAmount)
		FROM (
			-- LinkSupply
			SELECT -cfOut.IoBalance - cfIn.IoBalance + cfIn.Amount AS LinkAmount
			UNION ALL
			-- LinkDemand
			SELECT	-cfOut.Amount + ( cfIn.IoBalance + cfOut.IoBalance )  
					* ( CASE WHEN cfIn.IoBalance + cfOut.IoBalance < 0 THEN 1 ELSE 0 END )
			UNION ALL
			-- InAmount
			SELECT cfIn.Amount
		) T1
	) AS LinkAmount

INTO temp_InflowOutflow
FROM 
(
	SELECT
		Account,
		Oid,
		TranDate,
		Amount,
		IoBalance
	FROM temp_FifoCashFlow WHERE ForexSettleType = @InSettleType
) cfIn
CROSS JOIN 
(
	SELECT
		Account,
		Oid,
		TranDate,
		Amount,
		IoBalance
	FROM temp_FifoCashFlow WHERE ForexSettleType = @OutSettleType
) cfOut
WHERE
	-- In.Account = Out.Account
	cfIn.Account = cfOut.Account
	-- LinkDemand > 0
	AND -cfOut.Amount + ( cfIn.IoBalance + cfOut.IoBalance )  
		* ( CASE WHEN cfIn.IoBalance + cfOut.IoBalance < 0 THEN 1 ELSE 0 END ) > 0
	AND
	-- LinkSupply > 0
	-cfOut.IoBalance - cfIn.IoBalance + cfIn.Amount > 0
ORDER BY Oid

-- Commit

INSERT INTO ForexSettleLink (Oid, Account, CashFlowIn, CashFlowOut, AccountCcyAmt, TimeCreated)
SELECT 
	CAST(CAST(NEWID() AS BINARY(10)) + CAST(GETDATE() AS BINARY(6)) AS UNIQUEIDENTIFIER),
	Account,
	Oid,
	Out_Oid,
	LinkAmount,
	GETDATE()
FROM temp_InflowOutflow

END";
            }
        }

        public string RevalueOutflowsCommandText
        {
            get
            {
                return
@"UPDATE CashFlow SET
FunctionalCcyAmt =
(
	SELECT fsm.FunctionalCcyAmt / fsm.AccountCcyAmt * CashFlow.AccountCcyAmt
)
FROM CashFlow
JOIN
(
	SELECT 
		fsl.CashFlowOut,
		SUM ( fsl.AccountCcyAmt ) AS AccountCcyAmt,
		SUM ( cfIn.FunctionalCcyAmt * fsl.AccountCcyAmt / cfIn.AccountCcyAmt ) AS FunctionalCcyAmt
	FROM ForexSettleLink fsl
	LEFT JOIN CashFlow cfIn ON cfIn.Oid = fsl.CashFlowIn
	WHERE fsl.GCRecord IS NULL
	GROUP BY
		fsl.CashFlowOut
) fsm ON fsm.CashFlowOut = CashFlow.Oid
WHERE 
CashFlow.[Snapshot] = @Snapshot
AND CashFlow.TranDate BETWEEN @FromDate AND @ToDate";
            }
        }

        public string RevalueOutflowReclassCommandText
        {
            get
            {
                return
@"UPDATE cf0 SET
FunctionalCcyAmt =
cf0.AccountCcyAmt * cf1.FunctionalCcyAmt / cf1.AccountCcyAmt
FROM CashFlow cf0
JOIN
(
	SELECT 
	cf1.Account,
	cf1.TranDate,
	SUM ( cf1.AccountCcyAmt ) AS AccountCcyAmt,
	SUM ( cf1.FunctionalCcyAmt ) AS FunctionalCcyAmt,
	SUM ( cf1.AccountCcyAmt ) / SUM ( cf1.FunctionalCcyAmt ) AS ForexRate
	FROM CashFlow cf1
	WHERE cf1.ForexSettleType = @OutSettleType
	GROUP BY 
		cf1.Account,
		cf1.TranDate
) cf1 ON 
	cf1.TranDate = cf0.TranDate
	AND cf1.Account = cf0.Account
WHERE cf0.GCRecord IS NULL
AND cf0.[Snapshot] = @Snapshot
AND cf0.TranDate BETWEEN @FromDate AND @ToDate
AND cf0.ForexSettleType = @OutReclassSettleType";
            }
        }

        public string RevalueBankStmtCommandText
        {
            get
            {
                return
@"UPDATE BankStmt SET
FunctionalCcyAmt = CashFlow.FunctionalCcyAmt
FROM BankStmt
JOIN CashFlow ON CashFlow.Oid = BankStmt.CashFlow
WHERE 
	BankStmt.GCRecord IS NULL
	AND CashFlow.TranDate BETWEEN @FromDate AND @ToDate
	AND CashFlow.[Snapshot] = @Snapshot";
            }
        }

        #endregion

        private XPObjectSpace objSpace;
        private ForexSettleFifoParam paramObj;
        private DateTime fromDate;
        private DateTime toDate;
        private CashFlowSnapshot currentSnapshot;

        public void Process()
        {
            LinkCashFlows();
            RevalueOutflows();
            RevalueOutflowReclass();
            RevalueBankStmt();
        }

        public void LinkCashFlows()
        {
            var clauses = CreateSqlParameters();
            var parameters = CreateParameters(clauses);

            using (var cmd = ((SqlConnection)objSpace.Session.Connection).CreateCommand())
            {
                cmd.Parameters.AddRange(parameters.ToArray());
                cmd.CommandText = ProcessFifoCommandText;
                cmd.ExecuteNonQuery();
            }
        }

        public void RevalueOutflows()
        {
            var clauses = CreateSqlParameters();
            var parameters = CreateParameters(clauses);

            using (var cmd = ((SqlConnection)objSpace.Session.Connection).CreateCommand())
            {
                cmd.Parameters.AddRange(parameters.ToArray());
                cmd.CommandText = RevalueOutflowsCommandText;
                cmd.ExecuteNonQuery();
            }
        }

        public void RevalueOutflowReclass()
        {
            var clauses = CreateSqlParameters();
            var parameters = CreateParameters(clauses);

            using (var cmd = ((SqlConnection)objSpace.Session.Connection).CreateCommand())
            {
                cmd.Parameters.AddRange(parameters.ToArray());
                cmd.CommandText = RevalueOutflowReclassCommandText;
                cmd.ExecuteNonQuery();
            }
        }

        public void RevalueBankStmt()
        {
            var clauses = CreateSqlParameters();
            var parameters = CreateParameters(clauses);

            using (var cmd = ((SqlConnection)objSpace.Session.Connection).CreateCommand())
            {
                cmd.Parameters.AddRange(parameters.ToArray());
                cmd.CommandText = RevalueBankStmtCommandText;
                cmd.ExecuteNonQuery();
            }
        }

        public List<SqlDeclareClause> CreateSqlParameters()
        {
            var clauses = new List<SqlDeclareClause>()
            {
                new SqlDeclareClause("Snapshot", "uniqueidentifier",
                @"(SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("InSettleType", "int", Convert.ToString(Convert.ToInt32(CashFlowForexSettleType.In))),
                new SqlDeclareClause("OutSettleType", "int", Convert.ToString(Convert.ToInt32(CashFlowForexSettleType.Out))),
                new SqlDeclareClause("OutReclassSettleType", "int", Convert.ToString(Convert.ToInt32(CashFlowForexSettleType.OutReclass))),
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

        public void Reset()
        {

        }

    }
}
