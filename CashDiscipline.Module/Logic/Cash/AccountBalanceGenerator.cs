using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFormat;
using CashDiscipline.Module.BusinessObjects.Cash;

namespace CashDiscipline.Module.Logic.Cash
{
    public class AccountBalanceGenerator
    {
        private XPObjectSpace objSpace;

        public AccountBalanceGenerator(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }

        public void Generate(AccountBalanceParam paramObj)
        {
            var conn = (SqlConnection)objSpace.Session.Connection;
            using (var command = conn.CreateCommand())
            {
                command.CommandText = Smart.Format(
@"/* Parameters ------- */
DECLARE @FromDate datetime = '{FromDate}';
DECLARE @ToDate datetime = '{ToDate}';
DECLARE @Snapshot uniqueidentifier = (SELECT TOP 1 CurrentCashFlowSnapshot FROM SetOfBooks WHERE GCRecord IS NULL);
DECLARE @ActualStatus int = {ActualStatus};

/* Clean Up ------- */
IF OBJECT_ID('tempdb..#AccountTotal') IS NOT NULL DROP TABLE #AccountTotal;
IF OBJECT_ID('tempdb..#Dates') IS NOT NULL DROP TABLE #Dates;
IF OBJECT_ID('tempdb..#DateAccount') IS NOT NULL DROP TABLE #DateAccount;

/* Dates */
SELECT 
cf1.TranDate
INTO #Dates
FROM CashFlow cf1
WHERE cf1.Snapshot = @Snapshot
AND cf1.TranDate BETWEEN @FromDate AND @ToDate
AND cf1.Status = @ActualStatus
AND cf1.GCRecord IS NULL
UNION
SELECT @FromDate
UNION
SELECT @ToDate;

/* Create account total summary ------- */
SELECT
	cf.TranDate,
	cf.Account,
	SUM(COALESCE(cf.AccountCcyAmt, 0.00)) AS AccountCcyAmt,
	SUM(COALESCE(cf.FunctionalCcyAmt, 0.00)) AS FunctionalCcyAmt
INTO #AccountTotal
FROM CashFlow cf
JOIN Account ON Account.Oid = cf.Account
WHERE
	cf.[Snapshot] = @Snapshot
	AND cf.TranDate <= @ToDate
	AND cf.Status = @ActualStatus
	AND cf.GCRecord IS NULL
GROUP BY cf.TranDate, cf.Account;

/* Date Account Combination */

SELECT DISTINCT
	d1.TranDate,
	a1.Account
INTO #DateAccount
FROM #Dates d1
CROSS JOIN #AccountTotal a1;

/* Account Totals */

DELETE FROM AccountBalance WHERE TranDate BETWEEN @FromDate AND @ToDate;

INSERT INTO AccountBalance (Oid, Snapshot, TranDate, Account, AccountCcyAmt, FunctionalCcyAmt)
SELECT
NEWID(),
@Snapshot,
da1.TranDate,
da1.Account,
SUM(at1.AccountCcyAmt) AS AccountCcyAmt,
SUM(at1.FunctionalCcyAmt) AS FunctionalCcyAmt
FROM #DateAccount da1
LEFT JOIN #AccountTotal at1
	ON at1.TranDate <= da1.TranDate
	AND at1.Account = da1.Account
WHERE da1.TranDate BETWEEN @FromDate AND @ToDate
GROUP BY 
da1.TranDate,
da1.Account
HAVING ROUND(SUM(at1.AccountCcyAmt), 2) <> 0.00 AND ROUND(SUM(at1.FunctionalCcyAmt),2) <> 0.00
ORDER BY da1.TranDate, da1.Account;", new
{
    FromDate = string.Format("{0:yyyy-MM-dd}", paramObj.FromDate.Date),
    ToDate = string.Format("{0:yyyy-MM-dd}", paramObj.ToDate.Date),
    ActualStatus = Convert.ToInt32(CashFlowStatus.Actual).ToString()
});
                command.ExecuteNonQuery();
            }
        }
    }

    
}
