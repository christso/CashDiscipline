using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Logic.Cash;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Parameters in SQL
DECLARE @InSettleType int = 1
DECLARE @OutSettleType int = 2
*/

namespace CashDiscipline.Module.Logic.Forex
{
    public class ForexSettleFifoAlgorithm
    {

        public ForexSettleFifoAlgorithm(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
            this.currentSnapshot = CashFlowHelper.GetCurrentSnapshot(objSpace.Session);
        }

        #region SQL

        public const string ProcessFifoCommandText =
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

SELECT 
	CashFlow.Account,
	CashFlow.Oid,
	CashFlow.TranDate,
	CashFlow.AccountCcyAmt AS Amount,
	CashFlow.ForexSettleType,
	CAST ( 0.00 AS money ) AS IoBalance
INTO temp_FifoCashFlow
FROM CashFlow
WHERE GCRecord IS NULL
	AND ForexSettleType IN (@InSettleType, @OutSettleType)

UPDATE cf1 SET
IoBalance =
(
	SELECT SUM(cf2.Amount)
	FROM temp_FifoCashFlow cf2
	WHERE 
		cf2.TranDate <= cf1.TranDate
		AND cf2.Oid <= cf1.Oid
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
	*/CASE 
		WHEN
			-- LinkDemand
			-cfOut.Amount + ( cfIn.IoBalance + cfOut.IoBalance )  
				* ( CASE WHEN cfIn.IoBalance + cfOut.IoBalance < 0 THEN 1 ELSE 0 END )
			>
			--LinkSupply
			-cfOut.IoBalance - cfIn.IoBalance + cfIn.Amount
		THEN
			--LinkSupply
			-cfOut.IoBalance - cfIn.IoBalance + cfIn.Amount
		ELSE
			-- LinkDemand
			-cfOut.Amount + ( cfIn.IoBalance + cfOut.IoBalance )  
				* ( CASE WHEN cfIn.IoBalance + cfOut.IoBalance < 0 THEN 1 ELSE 0 END )			
	END AS LinkAmount
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

        #endregion

        private XPObjectSpace objSpace;
        private CashFlowSnapshot currentSnapshot;

        public void Process()
        {

        }

        public void Reset()
        {

        }

    }
}
