using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.FinAccounting;
using CashDiscipline.Module.ParamObjects.FinAccounting;
using Xafology.StringEvaluators;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using Xafology.Utils.Data;
using DevExpress.ExpressApp.Xpo;
using System.Diagnostics;

namespace CashDiscipline.Module.Logic.FinAccounting
{
    public class ParamJournalGenerator
    {
        private readonly FinGenJournalParam paramObj;
        private readonly XPObjectSpace objSpace;
        private List<GenLedgerFinActivityJoin> genLedgerFinActivityJoin;

        public ParamJournalGenerator(FinGenJournalParam paramObj, XPObjectSpace objSpace)
        {
            this.paramObj = paramObj;
            this.objSpace = objSpace;
        }

        public void Execute()
        {
            var session = objSpace.Session;
            if (paramObj == null) throw new UserFriendlyException("Param Object cannot be null.");

            #region Get Lookup Objects

            genLedgerFinActivityJoin = new List<GenLedgerFinActivityJoin>();

            var jnlGroupKeysInParams = paramObj.JournalGroupParams.Select(p => p.JournalGroup.Oid);
            var jnlGroupsInParams = new XPCollection<FinJournalGroup>(session,
                new InOperator("Oid", jnlGroupKeysInParams));

            var accountMaps = new XPCollection<FinAccount>(session, new InOperator("JournalGroup", jnlGroupsInParams));
            var accountsToMap = accountMaps.Select(k => k.Account);

            var activityMaps = new Func<List<FinActivity>>(() =>
            {
                var sortProps = new SortingCollection(null);
                sortProps.Add(new SortProperty("RowIndex", DevExpress.Xpo.DB.SortingDirection.Ascending));
                var criteria = CriteriaOperator.And(
                    new InOperator("JournalGroup", jnlGroupsInParams),
                    new BinaryOperator("Enabled", true, BinaryOperatorType.Equal));
                var result = session.GetObjects(session.GetClassInfo(typeof(FinActivity)),
                                criteria, sortProps, 0, false, true)
                                .Cast<FinActivity>().ToList();
                return result;
            })();
            //var activitiesToMap = activityMaps.GroupBy(m => m.FromActivity).Select(k => k.Key);

            #endregion

            DeleteAutoGenLedgerItems();

            #region Process via ORM

            var ormActivityMaps = activityMaps.Where(x => x.Algorithm == FinMapAlgorithmType.ORM);

            var bsJournalHelper = new BankStmtActivityOrmJournalHelper(objSpace, paramObj);
            bsJournalHelper.Process(accountMaps, ormActivityMaps);

            var cfJournalHelper = new CashFlowActivityOrmJournalHelper(objSpace, paramObj);
            cfJournalHelper.Process(accountMaps, ormActivityMaps);

            // commit to datastore (required for SQL algorithm which creates account gen ledgers)
            objSpace.Session.CommitTransaction();

            #endregion

            #region Process via SQL

            var sqlActivityMaps = activityMaps.Where(x => x.Algorithm == FinMapAlgorithmType.SQL);

            var bankStmtActivitySqlJnlr = new BankStmtActivitySqlJournalHelper(objSpace, paramObj);
            bankStmtActivitySqlJnlr.Process(sqlActivityMaps);

            var cashFlowActivitySqlJnlr = new CashFlowActivitySqlJournalHelper(objSpace, paramObj);
            cashFlowActivitySqlJnlr.Process(sqlActivityMaps);
            
            var accountSqlJnlr = new AccountSqlJournalHelper(objSpace, paramObj);
            accountSqlJnlr.Process();

            #endregion
            
        }

        #region Deleter
        public void DeleteAutoGenLedgerItems()
        {
            // Delete Bank Stmts and Cash Flows
            var sqlParamNames = new string[] { "FromDate", "ToDate", "EntryType", "SnapshotOid" };
            var sqlParamValues = new object[] { paramObj.FromDate, paramObj.ToDate,
                                    GenLedgerEntryType.Auto, SetOfBooks.CachedInstance.CurrentCashFlowSnapshot.Oid};
            objSpace.Session.ExecuteNonQuery(DeleteCommandText, sqlParamNames, sqlParamValues);
        }

        #region SQL
        public string DeleteCommandText
        {
            get
            {
                return @"
UPDATE GenLedger SET GCRecord = CAST(RAND() * 2147483646 + 1 AS INT)
FROM GenLedger
WHERE GenLedger.EntryType = @EntryType
AND (
    GenLedger.SrcBankStmt IN 
    (
        SELECT BankStmt.Oid FROM BankStmt WHERE BankStmt.TranDate BETWEEN @FromDate AND @ToDate
    )
    OR GenLedger.SrcCashFlow IN
    (
        SELECT CashFlow.Oid FROM CashFlow WHERE CashFlow.TranDate BETWEEN @FromDate AND @ToDate
        AND CashFlow.Snapshot = @SnapshotOid
    )
)";
            }
        }
        #endregion

        #endregion
    }
}
