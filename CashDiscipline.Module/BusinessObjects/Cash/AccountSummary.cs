using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CashDiscipline.Module.BusinessObjects.Cash
{
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (http://documentation.devexpress.com/#Xaf/CustomDocument2701).
    public class AccountSummary : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public AccountSummary(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
            LineType = AccountSummaryLineType.Flow;
        }
        // Fields...
        private CashFlowSnapshot _Snapshot;
        private AccountSummaryLineType _LineType;
        private decimal _AccountCcyAmt;
        private Account _Account;
        private DateTime _TranDate;

        #region Persistent Properties

        [ModelDefault("AllowEdit", "false")]
        public CashFlowSnapshot Snapshot
        {
            get
            {
                return _Snapshot;
            }
            set
            {
                SetPropertyValue("Snapshot", ref _Snapshot, value);
            }
        }

        public DateTime TranDate
        {
            get
            {
                return _TranDate;
            }
            set
            {
                SetPropertyValue("TranDate", ref _TranDate, value.Date);
            }
        }


        public Account Account
        {
            get
            {
                return _Account;
            }
            set
            {
                SetPropertyValue("Account", ref _Account, value);
            }
        }


        public decimal AccountCcyAmt
        {
            get
            {
                return _AccountCcyAmt;
            }
            set
            {
                SetPropertyValue("AccountCcyAmt", ref _AccountCcyAmt, value);
            }
        }

        public AccountSummaryLineType LineType
        {
            get
            {
                return _LineType;
            }
            set
            {
                SetPropertyValue("LineType", ref _LineType, value);
            }
        }
        #endregion

        #region Methods

        public static void CalculateBalance(XPObjectSpace objSpace, DateTime atDate, IList<CashFlowSnapshot> snapshots)
        {
            // delete balances matching the specified date
            CriteriaOperator deleteCriteria = Fields.TranDate == atDate & Fields.LineType == new OperandValue(AccountSummaryLineType.Balance);
            var snapshotOids = GetOidsFromSnapshots(snapshots);
            deleteCriteria = deleteCriteria & new InOperator(AccountSummary.Fields.SnapshotOid.PropertyName, snapshotOids);
            objSpace.Session.Delete(objSpace.GetObjects<AccountSummary>(deleteCriteria));

            // get currenet snapshot GUID
            var curSnapshotOid = SetOfBooks.CachedInstance.CurrentCashFlowSnapshot.Oid;

            // add balances to Account Summary
            var maxActualDate = CashFlow.GetMaxActualTranDate(objSpace.Session);
            XPQuery<CashFlow> cashFlows = new XPQuery<CashFlow>(((XPObjectSpace)objSpace).Session);

            foreach (Guid snapshotOid in snapshotOids)
            {
                IQueryable<CashBalanceGrouped> cfGrouped = null;

                if (snapshotOid != curSnapshotOid)
                {

                    // previous snapshot
                    if (maxActualDate == default(DateTime))
                    {
                        cfGrouped = from c in cashFlows
                                    where c.TranDate <= atDate
                                        && (
                                        c.Snapshot.Oid == snapshotOid
                                        //&& c.TranDate > maxActualDate
                                        || c.Snapshot.Oid == curSnapshotOid
                                        && c.Status == CashFlowStatus.Actual
                                        )
                                    group c by new { c.Account } into grp
                                    select new CashBalanceGrouped(
                                        grp.Key.Account,
                                        (decimal)grp.Sum(c => c.AccountCcyAmt)
                                    );
                    }
                    else
                    {
                        cfGrouped = from c in cashFlows
                                    where c.TranDate <= atDate
                                        && (
                                        c.Snapshot.Oid == snapshotOid
                                        && c.TranDate > maxActualDate
                                        || c.Snapshot.Oid == curSnapshotOid
                                        && c.Status == CashFlowStatus.Actual
                                        )
                                    group c by new { c.Account } into grp
                                    select new CashBalanceGrouped(
                                        grp.Key.Account,
                                        (decimal)grp.Sum(c => c.AccountCcyAmt)
                                    );
                    }

                }
                else
                {
                    // current snapshot

                    cfGrouped = from c in cashFlows
                                where c.TranDate <= atDate && c.Snapshot.Oid == snapshotOid
                                group c by new { c.Account } into grp
                                select new CashBalanceGrouped(
                                    grp.Key.Account,
                                    (decimal)grp.Sum(c => c.AccountCcyAmt)
                                );
                }

                if (cfGrouped == null) return;
                foreach (var cfItem in cfGrouped)
                {
                    var summary = objSpace.CreateObject<AccountSummary>();
                    summary.Snapshot = objSpace.GetObjectByKey<CashFlowSnapshot>(snapshotOid);
                    summary.TranDate = atDate;
                    summary.Account = cfItem.Account;
                    summary.AccountCcyAmt = cfItem.AccountCcyAmt;
                    summary.LineType = AccountSummaryLineType.Balance;
                }
            }
            objSpace.CommitChanges();
        }

        public static void CalculateBalance(IObjectSpace objSpace, DateTime atDate)
        {
            CalculateBalance((XPObjectSpace)objSpace, atDate, null);
        }

        public static void CalculateCashFlow(IObjectSpace objSpace, DateTime fromDate, DateTime toDate, IList<CashFlowSnapshot> snapshots)
        {
            CalculateCashFlow((XPObjectSpace)objSpace, fromDate, toDate, snapshots);
        }

        public static void CalculateCashFlow(XPObjectSpace objSpace, DateTime fromDate, DateTime toDate, IList<CashFlowSnapshot> snapshots)
        {
            // delete cash flows within the date range
            CriteriaOperator deleteCriteria = CriteriaOperator.Parse("TranDate Between (?, ?) And LineType = ?",
                fromDate, toDate, AccountSummaryLineType.Flow);
            var snapshotOids = GetOidsFromSnapshots(snapshots);
            deleteCriteria = deleteCriteria & new InOperator("Snapshot.Oid", snapshotOids);
            objSpace.Session.Delete(objSpace.GetObjects<AccountSummary>(deleteCriteria));

            // get currenet snapshot GUID
            var curSnapshotOid = SetOfBooks.CachedInstance.CurrentCashFlowSnapshot.Oid;

            // add cash flows to AccountSummary
            var maxActualDate = CashFlow.GetMaxActualTranDate(objSpace.Session); // TODO: why default(DateTime)?
            XPQuery<CashFlow> cashFlows = new XPQuery<CashFlow>(((XPObjectSpace)objSpace).Session);

            foreach (Guid snapshotOid in snapshotOids)
            {
                IQueryable<CashFlowGrouped> cfQuery = null;
                if (snapshotOid != curSnapshotOid)
                {
                    // previous snapshot
                    if (maxActualDate == default(DateTime))
                    {
                        // exclude max actual date from query
                        cfQuery = from c in cashFlows
                                  where c.TranDate >= fromDate && c.TranDate <= toDate
                                  && (c.Snapshot.Oid == curSnapshotOid
                                  && c.Status == CashFlowStatus.Actual
                                  ||
                                  c.Snapshot.Oid == snapshotOid
                                      // && c.TranDate > maxActualDate
                                  )
                                  orderby c.TranDate ascending
                                  group c by new { c.TranDate, c.Account } into grp
                                  select new CashFlowGrouped(
                                      grp.Key.TranDate,
                                      grp.Key.Account,
                                      (decimal)grp.Sum(c => c.AccountCcyAmt)
                                  );
                    }
                    else
                    {
                        cfQuery = from c in cashFlows
                                  where c.TranDate >= fromDate && c.TranDate <= toDate
                                  && (c.Snapshot.Oid == curSnapshotOid
                                  && c.Status == CashFlowStatus.Actual
                                  ||
                                  c.Snapshot.Oid == snapshotOid
                                  && c.TranDate > maxActualDate
                                  )
                                  orderby c.TranDate ascending
                                  group c by new { c.TranDate, c.Account } into grp
                                  select new CashFlowGrouped(
                                      grp.Key.TranDate,
                                      grp.Key.Account,
                                      (decimal)grp.Sum(c => c.AccountCcyAmt)
                                  );
                    }
                }
                else
                {
                    // current snapshot
                    cfQuery = from c in cashFlows
                              where c.TranDate >= fromDate && c.TranDate <= toDate
                              && c.Snapshot.Oid == snapshotOid
                              orderby c.TranDate ascending
                              group c by new { c.TranDate, c.Account } into grp
                              select new CashFlowGrouped(
                                  grp.Key.TranDate,
                                  grp.Key.Account,
                                  (decimal)grp.Sum(c => c.AccountCcyAmt)
                              );
                }

                foreach (var cfItem in cfQuery)
                {
                    var summary = objSpace.CreateObject<AccountSummary>();
                    summary.Snapshot = objSpace.GetObjectByKey<CashFlowSnapshot>(snapshotOid);
                    summary.TranDate = cfItem.TranDate;
                    summary.Account = cfItem.Account;
                    summary.AccountCcyAmt = cfItem.AccountCcyAmt;
                    summary.LineType = AccountSummaryLineType.Flow;
                }
            }
            objSpace.CommitChanges();
        }

        ///<summary>Get the first date in AccountSummary that is equal to or greater than fromDate
        /// to avoid performing the same calculations (since the result would be the same 
        /// for periods that have no cash flow movement.
        /// If fromDate is greater than any of the dates in AccountSummary, then this will
        /// return Max(TranDate) in Account Summary.
        /// </summary>
        /// <param name="theDate">Effective Date of the Balances.</param>
        public static DateTime GetUniqueBalanceDate(Session session, DateTime theDate)
        {
            var uniqueDate = session.Evaluate<AccountSummary>(
                CriteriaOperator.Parse("Min(TranDate)"),
                CriteriaOperator.Parse("TranDate >= ? And LineType = ?",
                    theDate, AccountSummaryLineType.Flow));
            if (uniqueDate == null)
                uniqueDate = session.Evaluate<AccountSummary>(
                    CriteriaOperator.Parse("Max(TranDate)"), null);
            if (uniqueDate == null)
                uniqueDate = theDate;
            return (DateTime)uniqueDate;
        }

        /// <summary>
        /// Get Oids from Snapshots. If snapshots object is null, 
        /// then the current snapshot GUID is added to the GUID list
        /// </summary>
        private static List<Guid> GetOidsFromSnapshots(IList<CashFlowSnapshot> snapshots)
        {
            var snapshotOids = new List<Guid>();

            if (snapshots == null)
            {
                snapshots = new List<CashFlowSnapshot>();
            }

            foreach (var snapshot in snapshots)
            {
                if (snapshot == null || snapshot.Oid == null) continue;
                snapshotOids.Add(snapshot.Oid);
            }
            if (snapshotOids.Count == 0)
                snapshotOids.Add(SetOfBooks.CachedInstance.CurrentCashFlowSnapshot.Oid);

            return snapshotOids;
        }

        #endregion.

        #region Classes

        private class CashFlowGrouped
        {
            public CashFlowGrouped(DateTime tranDate, Account account, decimal accountCcyAmt)
            {
                TranDate = tranDate;
                Account = account;
                AccountCcyAmt = accountCcyAmt;
            }

            public DateTime TranDate { get; set; }
            public Account Account { get; set; }
            public decimal AccountCcyAmt { get; set; }
        }

        private class CashBalanceGrouped
        {
            public CashBalanceGrouped(Account account, decimal accountCcyAmt)
            {
                Account = account;
                AccountCcyAmt = accountCcyAmt;
            }

            public Account Account { get; set; }
            public decimal AccountCcyAmt { get; set; }
        }

        #endregion

        #region Field Operators

        public new class Fields
        {
            public static OperandProperty TranDate
            {
                get
                {
                    return new OperandProperty("TranDate");
                }
            }
            public static OperandProperty Snapshot
            {
                get
                {
                    return new OperandProperty("Snapshot");
                }
            }
            public static OperandProperty Account
            {
                get
                {
                    return new OperandProperty("Account");
                }
            }
            public static OperandProperty LineType
            {
                get
                {
                    return new OperandProperty("LineType");
                }
            }
            public static OperandProperty AccountCcyAmt
            {
                get
                {
                    return new OperandProperty("AccountCcyAmt");
                }
            }
            public static OperandProperty SnapshotOid
            {
                get
                {
                    return new OperandProperty(Snapshot.PropertyName + "." + CashFlowSnapshot.Fields.Oid.PropertyName);
                }
            }
        }

        public static FieldNamesClass FieldNames { get { return new FieldNamesClass(); } }
        public class FieldNamesClass
        {
            public string TranDate { get { return Fields.TranDate.PropertyName; } }
            public string Snapshot { get { return Fields.Snapshot.PropertyName; } }
            public string Account { get { return Fields.Account.PropertyName; } }
            public string LineType { get { return Fields.LineType.PropertyName; } }
            public string AccountCcyAmt { get { return Fields.AccountCcyAmt.PropertyName; } }
            public string SnapshotOid { get { return Snapshot + "." + CashFlowSnapshot.FieldNames.Oid; } }
        }
        #endregion
    }
    public enum AccountSummaryLineType
    {
        Flow,
        Balance
    }
}
