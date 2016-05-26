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

namespace CashDiscipline.Module.Logic.FinAccounting
{
    public class JournalGenerator
    {
        private readonly FinGenJournalParam paramObj;
        private readonly XPObjectSpace objSpace;
        private List<GenLedgerFinActivityJoin> genLedgerFinActivityJoin;

        public JournalGenerator(FinGenJournalParam paramObj, XPObjectSpace objSpace)
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
            var activitiesToMap = activityMaps.GroupBy(m => m.FromActivity).Select(k => k.Key);

            var accountMaps = new XPCollection<FinAccount>(session, new InOperator("JournalGroup", jnlGroupsInParams));
            var accountsToMap = accountMaps.Select(k => k.Account);

            #endregion

            DeleteAutoGenLedgerItems();

            #region Process Bank Stmt

            var bsJournalHelper = new BankStmtJournalHelper(objSpace, paramObj);
            ProcessJournals(bsJournalHelper, accountMaps, activityMaps);

            #endregion

            #region Process Cash Flows

            var cfJournalHelper = new CashFlowJournalHelper(objSpace, paramObj);
            ProcessJournals(cfJournalHelper, accountMaps, activityMaps);

            #endregion

            objSpace.Session.CommitTransaction();
        }

        public void ProcessJournals<T>(IJournalHelper<T> helper, IEnumerable<FinAccount> accountMaps, IEnumerable<FinActivity> activityMaps)
        {
            var accountsToMap = accountMaps.Select(k => k.Account);
            var activitiesToMap = activityMaps.GroupBy(m => m.FromActivity).Select(k => k.Key);
            var sourceObjects = helper.GetSourceObjects(activitiesToMap, accountsToMap);
            helper.Process(sourceObjects, accountMaps, activityMaps);
        }

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
    }
}
