using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.BusinessObjects.Setup;
using CashDiscipline.Module.ParamObjects.Cash;
using Xafology.ExpressApp.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xafology.Spreadsheet.Attributes;
using DevExpress.Persistent.BaseImpl;
using Xafology.ExpressApp.Xpo.Import;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects;
using System.Data.SqlClient;

/* Parameters in SQL
DECLARE @FromDate date = (SELECT TOP 1 FromDate FROM CashFlowFixParam)
DECLARE @ToDate date = (SELECT TOP 1 ToDate FROM CashFlowFixParam)
DECLARE @ApayableLockdownDate date = (SELECT TOP 1 ApayableLockdownDate FROM CashFlowFixParam)
DECLARE @IgnoreFixTagType int = 0
DECLARE @ForecastStatus int = 0
DECLARE @Snapshot uniqueidentifier = COALESCE(
	(SELECT TOP 1 [Snapshot] FROM CashFlowFixParam),
	(SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks)
)
DECLARE @DefaultCounterparty uniqueidentifier = (SELECT TOP 1 [Counterparty] FROM CashFlowDefaults)
DECLARE @FunctionalCurrency uniqueidentifier = (SELECT TOP 1 [FunctionalCurrency] FROM SetOfBooks)
DECLARE @ReversalFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'R')
DECLARE @RevRecFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'RR')
DECLARE @ResRevRecFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'RRR')
DECLARE @PayrollFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'PR')
*/

namespace CashDiscipline.Module.Logic.Cash
{
    public class SqlFixCashFlowsAlgorithm : IFixCashFlows
    {
        public SqlFixCashFlowsAlgorithm(XPObjectSpace objSpace, CashFlowFixParam paramObj)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;

            if (paramObj.Snapshot == null)
                currentSnapshot = GetCurrentSnapshot(objSpace.Session);
            else
                currentSnapshot = paramObj.Snapshot;

            if (paramObj.ApReclassActivity == null)
                throw new ArgumentNullException("ApReclassActivity");
            paramApReclassActivity = objSpace.GetObjectByKey<Activity>(objSpace.GetKeyValue(paramObj.ApReclassActivity));

            defaultCounterparty = objSpace.FindObject<Counterparty>(
             CriteriaOperator.Parse("Name LIKE ?", Constants.DefaultFixCounterparty));

            var query = new XPQuery<CashForecastFixTag>(objSpace.Session);

            reversalFixTag = query
                .Where(x => x.Name == Constants.ReversalFixTag).FirstOrDefault();

            revRecFixTag = query
                .Where(x => x.Name == Constants.RevRecFixTag).FirstOrDefault();

            resRevRecFixTag = query
                .Where(x => x.Name == Constants.ResRevRecFixTag).FirstOrDefault();

            payrollFixTag = query
                .Where(x => x.Name == Constants.PayrollFixTag).FirstOrDefault();

            //this.cashFlowsToDelete = new List<CashFlow>();
            setOfBooks = SetOfBooks.GetInstance(objSpace);
        }

        private XPObjectSpace objSpace;
        private CashFlowFixParam paramObj;
        private Activity paramApReclassActivity;
        private Counterparty defaultCounterparty;
        private CashForecastFixTag reversalFixTag;
        private CashForecastFixTag revRecFixTag;
        private CashForecastFixTag resRevRecFixTag;
        private CashForecastFixTag payrollFixTag;
        private SetOfBooks setOfBooks;
        private FixCashFlowsRephaser rephaser;
        private IEnumerable<CashFlowFixMapping> cashFlowMappings;
        private CashFlowFixMapper cashFlowMapper;
        private CashFlowSnapshot currentSnapshot;

        public IEnumerable<CashFlow> GetCashFlowsToFix()
        {
            return null;
        }

        public void ProcessCashFlows()
        {

            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            command.CommandText = ProcessCommandText;

            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("FromDate", paramObj.FromDate),
                new SqlParameter("ToDate", paramObj.ToDate),
                new SqlParameter("Snapshot", currentSnapshot.Oid),
                new SqlParameter("Fix", reversalFixTag.Oid),
                new SqlParameter("IgnoreFixTagType", Convert.ToInt32(CashForecastFixTagType.Ignore)),
                new SqlParameter("ForecastStatus", Convert.ToInt32(CashFlowStatus.Forecast)),
                new SqlParameter("DefaultCounterparty", defaultCounterparty.Oid),
                new SqlParameter("FunctionalCurrency", setOfBooks.FunctionalCurrency.Oid),
                new SqlParameter("ReversalFixTag", reversalFixTag.Oid),
                new SqlParameter("RevRecFixTag", revRecFixTag.Oid),
                new SqlParameter("ResRevRecFixTag", resRevRecFixTag.Oid),
            };


            command.Parameters.AddRange(parameters.ToArray());

            command.ExecuteNonQuery();
        }

        public void Reset()
        {

        }
        
        private CashFlowSnapshot GetCurrentSnapshot(Session session)
        {
            return CashFlowHelper.GetCurrentSnapshot(session);
        }

        #region Sql

        public string ResetCommandText
        {
            get
            {
         
                return
@"UPDATE CashFlow SET 
IsFixeeSynced = 0,
IsFixerFixeesSynced = 0,
IsFixerSynced = 0
WHERE GCRecord IS NULL
AND [Snapshot] = @Snapshot
";
            }
        }

        public string ProcessCommandText
        {
            get
            {
                return


@"-- CashFlowsToFix

IF OBJECT_ID('temp_CashFlowsToFix') IS NOT NULL DROP TABLE temp_CashFlowsToFix;

SELECT cf.* INTO temp_CashFlowsToFix
FROM CashFlow cf
LEFT JOIN CashForecastFixTag tag ON tag.Oid = cf.Fix
LEFT JOIN CashFlow fixer ON fixer.Oid = cf.Oid
WHERE
    cf.GCRecord IS NULL
    AND tag.GCRecord IS NULL
    AND cf.TranDate BETWEEN @FromDate AND @ToDate
	AND cf.[Snapshot] = @Snapshot
    AND (cf.Fix = NULL OR tag.FixTagType != @IgnoreFixTagType)
    AND (cf.IsFixeeSynced=0 OR cf.IsFixerSynced=0 OR NOT cf.IsFixerFixeesSynced=0)
    OR fixer.GCRecord IS NOT NULL
    ;

-- Delete Existing Fixes

UPDATE cf
SET 
GCRecord = CAST(RAND() * 2147483646 + 1 AS INT)
FROM CashFlow cf
WHERE GCRecord IS NULL
AND [Snapshot] = @Snapshot
AND Fix IN (@ReversalFixTag, @RevRecFixTag, @ResRevRecFixTag)
AND ParentCashFlow IN (SELECT cf2.Oid FROM temp_CashFlowsToFix cf2)

-- FixeeFixer

IF OBJECT_ID('temp_FixeeFixer') IS NOT NULL DROP TABLE temp_FixeeFixer;

SELECT Fixee, Fixer
INTO temp_FixeeFixer
FROM
(
	SELECT
		-- RowNum = 1 if fixee currency equals fixer currency, otherwise may be greater than 1
		ROW_NUMBER() OVER(
			PARTITION BY fixee.Oid 
			ORDER BY CASE WHEN fixeeAccount.Currency = fixerAccount.Currency THEN 0 ELSE 1 END
		) AS RowNum,
		fixee.Oid AS Fixee,
		fixer.Oid AS Fixer,
		fixee.TranDate,
		fixee.Account,
		fixee.AccountCcyAmt,
		fixee.FixFromDate,
		fixee.FixToDate
	FROM temp_CashFlowsToFix fixee
	LEFT JOIN Counterparty fixeeCparty ON fixeeCparty.Oid = fixee.Counterparty
	LEFT JOIN Account fixeeAccount ON fixeeAccount.Oid = fixee.Account
	LEFT JOIN temp_CashFlowsToFix fixer
		ON fixee.TranDate BETWEEN fixer.FixFromDate AND fixer.FixToDate
			AND fixee.FixActivity = fixer.FixActivity
			AND fixer.[Status] = @ForecastStatus
			AND fixer.FixRank > fixee.FixRank
			AND 
			(
				fixer.Counterparty IS NULL OR fixer.Counterparty = @DefaultCounterparty
				OR fixee.Counterparty IS NULL AND fixer.Counterparty IS NULL
				OR fixee.Counterparty IS NOT NULL 
					AND fixer.Counterparty = fixeeCparty.FixCounterparty
			)
	LEFT JOIN Account fixerAccount 
		ON fixerAccount.Oid = fixer.Account
	WHERE
		fixee.Account IS NOT NULL AND 
		(
			fixeeAccount.FixAccount = fixerAccount.FixAccount
			OR fixee.Account = fixer.Account
		)
) T1
WHERE RowNum = 1

-- Insert Cash Flow Reversal
IF OBJECT_ID('temp_FixReversal') IS NOT NULL DROP TABLE temp_FixReversal;

SELECT cf.*
INTO temp_FixReversal
FROM temp_CashFlowsToFix cf
JOIN temp_FixeeFixer fixeeFixer ON fixeeFixer.Fixee = cf.Oid;

UPDATE revFix SET 
	Oid = NEWID(),
	ParentCashFlow = fixeeFixer.Fixee,
	TranDate = fixer.TranDate,

	-- if the currencies do not match, use functional currency
	CounterCcy = CASE WHEN revFix.CounterCcy <> fixer.CounterCcy THEN @FunctionalCurrency ELSE revFix.CounterCcy END,
	CounterCcyAmt = CASE WHEN revFix.CounterCcy <> fixer.CounterCcy THEN -revFix.FunctionalCcyAmt ELSE -revFix.CounterCcyAmt END,

	AccountCcyAmt = -revFix.AccountCcyAmt,
	FunctionalCcyAmt = -revFix.FunctionalCcyAmt,
	Fix = @ReversalFixTag,
	Fixer = NULL

FROM temp_FixReversal revFix
LEFT JOIN temp_FixeeFixer fixeeFixer
	ON fixeeFixer.Fixee = revFix.Oid
LEFT JOIN temp_CashFlowsToFix fixer
	ON fixer.Oid = fixeeFixer.Fixer

-- Link Fixee Cash Flow to Fixer

UPDATE CashFlow
SET Fixer = fixeeFixer.Fixer
FROM CashFlow cf
	INNER JOIN temp_FixeeFixer fixeeFixer ON cf.Oid = fixeeFixer.Fixee

-- Finalize

INSERT INTO CashFlow
SELECT * FROM temp_FixReversal

-- SELECT
/*
SELECT fixeeFixer.*, 
	fixee.TranDate, 
	fixee.Counterparty,
	fixee.AccountCcyAmt,
	fixee.Account,
	fixeeAccount.FixAccount,
	fixee.FixFromDate,
	fixee.FixToDate
FROM temp_FixeeFixer fixeeFixer
LEFT JOIN CashFlow fixee ON fixee.Oid = fixeeFixer.Fixee
LEFT JOIN Account fixeeAccount ON fixee.Account = fixeeAccount.Oid

SELECT * FROM temp_FixReversal

*/";
            }
        }

        #endregion
    }

}
