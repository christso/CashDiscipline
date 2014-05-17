using System;
using System.Linq;
using System.Text;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using System.Collections.Generic;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using CTMS.Module.BusinessObjects.Market;
using CTMS.Module.BusinessObjects.Setup;
using DevExpress.ExpressApp.Xpo;
using CTMS.Module.ParamObjects.Cash;
using System.Diagnostics;
using DG2NTT.Utilities;
using GenerateUserFriendlyId.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Forex;
using CTMS.Module.HelperClasses.Xpo;

// With XPO, the data model is declared by classes (so-called Persistent Objects) that will define the database structure, and consequently, the user interface (http://documentation.devexpress.com/#Xaf/CustomDocument2600).
namespace CTMS.Module.BusinessObjects.Cash
{
    [VisibleInReports(true)]
    [ModelDefault("IsCloneable", "True")]
    [ModelDefault("IsFooterVisible", "True")]
    [DefaultListViewOptions(allowEdit:true, newItemRowPosition: NewItemRowPosition.Top)]
    [DefaultProperty("CashFlowId")]
    public class CashFlow : UserFriendlyIdPersistentObject, ICalculateToggleObject
    {

        public CashFlow(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here or place it only when the IsLoading property is false.
            Changed += CashFlow_Changed;
        }

        void CashFlow_Changed(object sender, ObjectChangeEventArgs e)
        {
            if (e.PropertyName == "SequentialNumber")
            {
                if (Snapshot.Oid == SetOfBooks.CachedInstance.CurrentCashFlowSnapshot.Oid)
                {
                    OrigSequentialNumber = (long)e.NewValue;
                }
            }
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            _CalculateEnabled = true;
            if (AppSettings.UserTriggersEnabled)
                InitDefaultValues();
        }
        public void InitDefaultValues()
        {
            Snapshot = GetCurrentSnapshot(Session);

            TranDate = DateTime.Now.Date;

            CounterCcy = Session.GetObjectByKey<Currency>(SetOfBooks.CachedInstance.FunctionalCurrency.Oid);
            
            var defObj = Session.FindObject<CashFlowDefaults>(null);
            if (defObj == null) return;
            
            Counterparty = defObj.Counterparty;
            Account = defObj.Account;
            Activity = defObj.Activity;
        }

        private static CashFlowSnapshot GetCurrentSnapshot(Session session)
        {
            return session.GetObjectByKey<CashFlowSnapshot>(SetOfBooks.CachedInstance.CurrentCashFlowSnapshot.Oid);
        }

        protected override void OnSaving()
        {
            base.OnSaving();

            if (IsFixUpdated)
                IsFixUpdated = false;
        }

        protected override void OnDeleting()
        {
            while (Fixees.Count != 0)
                Fixees[0].Fixer = null;

            ParentCashFlow = null;

            while (ChildCashFlows.Count != 0)
            {
                ChildCashFlows[0].Delete();
            }

            base.OnDeleting();
        }

        // Fields...
        private bool _IsFixUpdated;
        private CashFlowSnapshot _Snapshot;
        private CashFlow _ParentCashFlow;
        private CashFlow _Fixer;
        private CashFlowSource _Source;
        private Activity _FixActivity;
        private DateTime _FixToDate;
        private DateTime _FixFromDate;
        private DateTime _DateUnFix;
        private bool _IsReclass;
        private CashForecastFixTag _Fix;
        private int _FixRank;
        private Account _Account;
        private CashFlowStatus _Status;
        private Counterparty _Counterparty;
        private Currency _CounterCcy;
        private decimal _CounterCcyAmt;
        private decimal _FunctionalCcyAmt;
        private decimal _AccountCcyAmt;
        private string _Description;
        private Activity _Activity;
        private DateTime _TranDate;
        private CashFlowForexSettleType _ForexSettleType;
        private decimal? _ForexLinkedInAccountCcyAmt;
        private decimal? _ForexLinkedOutAccountCcyAmt;
        private bool? _ForexLinkIsClosed;
        private long _OrigSequentialNumber;
        private long _ForexSettleGroupId;
        private bool _CalculateEnabled;

        // Whether Real Time calculation is enabled. If not, then calculation will occur on saving.
        [MemberDesignTimeVisibility(false), Browsable(false), NonPersistent]
        public bool CalculateEnabled
        {
            get { return _CalculateEnabled; }
            set
            {
                _CalculateEnabled = value;
            }
        }

        #region Main Properties

        [ExcelReportField]
        [PersistentAlias("concat('CF', ToStr(SequentialNumber))")]
        public string CashFlowId
        {
            get
            {
                return Convert.ToString(EvaluateAlias("CashFlowId"));
            }
        }

        [Association("CashFlowSnapshot-CashFlows")]
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

        [PersistentAlias("concat('CF', ToStr(OrigSequentialNumber))")]
        public string OrigCashFlowId
        {
            get
            {
                return Convert.ToString(EvaluateAlias("OrigCashFlowId"));
            }
        }

        [ModelDefault("AllowEdit", "false")]
        [VisibleInLookupListView(false)]
        [VisibleInListView(false)]
        [VisibleInDetailView(false)]
        public long OrigSequentialNumber
        {
            get
            {
                return _OrigSequentialNumber;
            }
            set
            {
                SetPropertyValue("OrigSequentialNumber", ref _OrigSequentialNumber, value);
            }
        }



        [ExcelReportField]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [RuleRequiredField("CashFlow.TranDate_RuleRequiredField", DefaultContexts.Save)]
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

        [MemberDesignTimeVisibility(false)]
        [ExcelReportField]
        [DevExpress.Xpo.DisplayName("Account")]
        public string AccountName
        {
            get
            {
                return Account.Name;
            }
        }

        [Association("Account-CashFlows")]
        [RuleRequiredField("CashFlow.Account_RuleRequiredField", DefaultContexts.Save)]
        [ImmediatePostData(true)]
        public Account Account
        {
            get
            {
                return _Account;
            }
            set
            {
                if (SetPropertyValue("Account", ref _Account, value))
                {
                    if (!IsLoading && !IsSaving && value != null)
                    {
                        if (CounterCcy == null)
                            SetPropertyValue("CounterCcy", ref _CounterCcy, value.Currency);
                    }
                }
                
            }
        }

        [MemberDesignTimeVisibility(false)]
        [ExcelReportField]
        [DevExpress.Xpo.DisplayName("Activity")]
        public string ActivityName
        {
            get
            {
                return Activity.Name;
            }
        }

        [Association("Activity-CashFlows")]
        [RuleRequiredField("CashFlow.Activity_RuleRequiredField", DefaultContexts.Save)]
        public Activity Activity
        {
            get
            {
                return _Activity;
            }
            set
            {
                SetPropertyValue("Activity", ref _Activity, value);
            }
        }
        
        [Association("Counterparty-CashFlows")]
        [RuleRequiredField("CashFlow.Counterparty_RuleRequiredField", DefaultContexts.Save)]
        public Counterparty Counterparty
        {
            get
            {
                return _Counterparty;
            }
            set
            {
                SetPropertyValue("Counterparty", ref _Counterparty, value);
            }
        }

        [ExcelReportField("Activity")]
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        [ImmediatePostData(true)]
        public decimal AccountCcyAmt
        {
            get
            {
                return _AccountCcyAmt;
            }
            set
            {
                if (SetPropertyValue("AccountCcyAmt", ref _AccountCcyAmt, Math.Round(value, 2)))
                {
                    if (!IsLoading && !IsSaving && Account != null && CalculateEnabled
                        && TranDate != default(DateTime))
                    {
                        UpdateFunctionalCcyAmt(this, value, Account.Currency);
                        UpdateCounterCcyAmt(this, value, Account.Currency);
                    }
                }
            }
        }

        [ExcelReportField]
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal FunctionalCcyAmt
        {
            get
            {
                return _FunctionalCcyAmt;
            }
            set
            {
                if (SetPropertyValue("FunctionalCcyAmt", ref _FunctionalCcyAmt, Math.Round(value,2)))
                {
                    if (!IsSaving && !IsLoading)
                    {
                        UpdateBankStmtsFunctionalCcyAmt();
                    }
                }
            }
        }

        [ExcelReportField]
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        [ImmediatePostData(true)]
        public decimal CounterCcyAmt
        {
            get
            {
                return _CounterCcyAmt;
            }
            set
            {
                if (SetPropertyValue("CounterCcyAmt", ref _CounterCcyAmt, Math.Round(value,2)))
                {
                    if (!IsLoading && CounterCcy != null && CalculateEnabled && TranDate != default(DateTime))
                    {
                        UpdateFunctionalCcyAmt(this, CounterCcyAmt, CounterCcy);
                        UpdateAccountCcyAmt(this, CounterCcyAmt, CounterCcy);
                    }
                }
            }
        }

        [ExcelReportField]
        [MemberDesignTimeVisibility(false)]
        [DevExpress.Xpo.DisplayName("Counter Ccy")]
        public string CounterCcyName
        {
            get
            {
                return CounterCcy.Name;
            }
        }

        [Association("Currency-CashFlows")]
        [RuleRequiredField("CashFlow.CounterCcy_RuleRequiredField", DefaultContexts.Save)]
        [ImmediatePostData(true)]
        public Currency CounterCcy
        {
            get
            {
                return _CounterCcy;
            }
            set
            {
                SetPropertyValue("CounterCcy", ref _CounterCcy, value);
            }
        }

        [Size(SizeAttribute.Unlimited)]
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                SetPropertyValue("Description", ref _Description, value);
            }
        }


        [Association("CashFlowSource-CashFlows")]
        public CashFlowSource Source
        {
            get
            {
                return _Source;
            }
            set
            {
                SetPropertyValue("Source", ref _Source, value);
            }
        }

        public CashFlowStatus Status
        {
            get
            {
                return _Status;
            }
            set
            {
                SetPropertyValue("Status", ref _Status, value);
            }
        }

        [Association("CashFlow-BankStmts")]
        public XPCollection<BankStmt> BankStmts
        {
            get
            {
                return GetCollection<BankStmt>("BankStmts");
            }
        }

        [Association("PrimaryCashFlow-ForexTrades")]
        public XPCollection<ForexTrade> PrimaryCashFlowForexTrades
        {
            get
            {
                return GetCollection<ForexTrade>("PrimaryCashFlowForexTrades");
            }
        }

        [Association("CounterCashFlow-ForexTrades")]
        public XPCollection<ForexTrade> CounterCashFlowForexTrades
        {
            get
            {
                return GetCollection<ForexTrade>("CounterCashFlowForexTrades");
            }
        }

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        public DateDim DateKey
        {
            get
            {
                return Session.GetObjectByKey<DateDim>(TranDate);
            }
        }
        #endregion

        #region Amount Calculators

        private static CriteriaOperator GetRateCriteriaOperator(Currency fromCcy, Currency toCcy, DateTime convDate)
        {
            var rateOp = CriteriaOperator.Parse(
                              "ConversionDate = [<ForexRate>][FromCurrency.Oid = ? "
                                + "AND ToCurrency.Oid = ? AND ConversionDate <= ?].Max(ConversionDate) "
                                + "AND FromCurrency.Oid = ? AND ToCurrency.Oid = ?",
                              fromCcy.Oid, toCcy.Oid, convDate, fromCcy.Oid, toCcy.Oid);
            return rateOp;
        }

        private static ForexRate GetForexRateObject(Session session, Currency fromCcy, Currency toCcy, DateTime convDate)
        {
            var rateOp = GetRateCriteriaOperator(fromCcy, toCcy, convDate);
            return session.FindObject<ForexRate>(rateOp);
        }

        private static decimal GetForexRate(Session session, Currency fromCcy, Currency toCcy, DateTime convDate)
        {
            if (fromCcy == toCcy) return 1;
            var rateObj = GetForexRateObject(session, fromCcy, toCcy, convDate);
            if (rateObj != null)
                return rateObj.ConversionRate;
            return 0;
        }

        public static void UpdateAccountCcyAmt(CashFlow obj, decimal fromAmt, Currency fromCcy)
        {
            if (obj.Account == null) return;
            var session = obj.Session;
            if (obj.Account.Currency == null)
                throw new UserFriendlyException("No currency for account " + obj.Account.Name);
            if (obj.Account.Currency.Oid == fromCcy.Oid)
                obj.AccountCcyAmt = fromAmt;
            else if (obj.TranDate != default(DateTime))
            {
                var rateObj = GetForexRateObject(session, fromCcy, obj.Account.Currency, (DateTime)obj.TranDate);
                if (rateObj != null)
                {
                    // TODO: do not assume that Functional Currency is 'AUD'
                    var value = fromAmt * (decimal)rateObj.ConversionRate;
                    obj.SetPropertyValue("AccountCcyAmt", ref obj._AccountCcyAmt, value);
                }
            }
        }

        public void UpdateFunctionalCcyAmt(decimal fromAmt, Currency fromCcy)
        {
            CashFlow.UpdateFunctionalCcyAmt(this, fromAmt, fromCcy);
        }

        public static void UpdateFunctionalCcyAmt(CashFlow obj, decimal fromAmt, Currency fromCcy)
        {
            if (obj == null || fromCcy == null) return;
            var session = obj.Session;

            //if (obj.Account != null && obj.Account.Currency.Oid == SetOfBooks.CachedInstance.FunctionalCurrency.Oid)
            //    obj.FunctionalCcyAmt = fromAmt;
            if (SetOfBooks.CachedInstance.FunctionalCurrency.Oid == fromCcy.Oid)
                obj.FunctionalCcyAmt = fromAmt;
            else if (obj.TranDate != default(DateTime))
            {
                var rateObj = GetForexRateObject(session, fromCcy, SetOfBooks.CachedInstance.FunctionalCurrency, (DateTime)obj.TranDate);
                if (rateObj != null)
                {
                    var value = fromAmt * (decimal)rateObj.ConversionRate;
                    obj.FunctionalCcyAmt = value;
                }
            }
        }

        public static void UpdateCounterCcyAmt(CashFlow obj, decimal fromAmt, Currency fromCcy)
        {
            if (obj.CounterCcy == null) return;
            var session = obj.Session;
            if (obj.CounterCcy.Oid == fromCcy.Oid)
                obj.CounterCcyAmt = fromAmt;
            else if (obj.TranDate != default(DateTime))
            {
                var rateObj = GetForexRateObject(session, fromCcy, obj.CounterCcy, (DateTime)obj.TranDate);
                if (rateObj != null)
                {
                    // TODO: do not assume that Local Currency is 'AUD'
                    var value = Math.Round(fromAmt * (decimal)rateObj.ConversionRate, 2);
                    obj.SetPropertyValue("CounterCcyAmt", ref obj._CounterCcyAmt, value);
                }
            }
        }
        #endregion

        #region Fixing

        public int FixRank
        {
            get
            {
                return _FixRank;
            }
            set
            {
                SetPropertyValue("FixRank", ref _FixRank, value);
            }
        }

        [Association("CashForecastFixTag-CashFlows")]
        public CashForecastFixTag Fix
        {
            get
            {
                return _Fix;
            }
            set
            {
                SetPropertyValue("Fix", ref _Fix, value);
            }
        }

        public bool IsReclass
        {
            get
            {
                return _IsReclass;
            }
            set
            {
                SetPropertyValue("IsReclass", ref _IsReclass, value);
            }
        }

        public DateTime DateUnFix
        {
            get
            {
                return _DateUnFix;
            }
            set
            {
                SetPropertyValue("DateUnFix", ref _DateUnFix, value);
            }
        }

        public DateTime FixFromDate
        {
            get
            {
                return _FixFromDate;
            }
            set
            {
                SetPropertyValue("FixFromDate", ref _FixFromDate, value);
            }
        }


        public DateTime FixToDate
        {
            get
            {
                return _FixToDate;
            }
            set
            {
                SetPropertyValue("FixToDate", ref _FixToDate, value);
            }
        }


        public Activity FixActivity
        {
            get
            {
                return _FixActivity;
            }
            set
            {
                SetPropertyValue("FixActivity", ref _FixActivity, value);
            }
        }

        [Association("CashFlowFixer-CashFlows")]
        public CashFlow Fixer
        {
            get
            {
                return _Fixer;
            }
            set
            {
                SetPropertyValue("Fixer", ref _Fixer, value);
            }
        }

        [Association("ParentCashFlow-ChildCashFlows")]
        public XPCollection<CashFlow> ChildCashFlows
        {
            get
            {
                return GetCollection<CashFlow>("ChildCashFlows");
            }
        }

        [Association("ParentCashFlow-ChildCashFlows")]
        public CashFlow ParentCashFlow
        {
            get
            {
                return _ParentCashFlow;
            }
            set
            {
                SetPropertyValue("ParentCashFlow", ref _ParentCashFlow, value);
            }
        }

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        public DateTime ParentTranDate
        {
            get
            {
                if (ParentCashFlow == null)
                    return TranDate;
                return ParentCashFlow.TranDate;
            }
        }

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        public Activity ParentActivity
        {
            get
            {
                if (ParentCashFlow == null)
                    return Activity;
                return ParentCashFlow.ParentActivity;
            }
        }

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        public CashFlowSource ParentSource
        {
            get
            {
                if (ParentCashFlow == null)
                    return Source;
                return ParentCashFlow.Source;
            }
        }

        [Association("CashFlowFixer-CashFlows")]
        public XPCollection<CashFlow> Fixees
        {
            get
            {
                return GetCollection<CashFlow>("Fixees");
            }
        }

        //[VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        public bool IsFixUpdated
        {
            get
            {
                return _IsFixUpdated;
            }
            set
            {
                SetPropertyValue("IsFixUpdated", ref _IsFixUpdated, value);
            }
        }

        #endregion

        #region Forex
        public CashFlowForexSettleType ForexSettleType
        {
            get
            {
                return _ForexSettleType;
            }
            set
            {
                SetPropertyValue("ForexSettleType", ref _ForexSettleType, value);
            }
        }

        [MemberDesignTimeVisibility(false)]
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal ForexLinkedInAccountCcyAmt
        {
            get
            {
                if (!IsLoading && !IsSaving && _ForexLinkedInAccountCcyAmt == null)
                    UpdateForexLinkedInAmt(false);
                return (decimal)(_ForexLinkedInAccountCcyAmt ?? 0);
            }
        }

        [MemberDesignTimeVisibility(false)]
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal ForexLinkedOutAccountCcyAmt
        {
            get
            {
                if (!IsLoading && !IsSaving && _ForexLinkedOutAccountCcyAmt == null)
                    UpdateForexLinkedOutAmt(false);
                return (decimal)(_ForexLinkedOutAccountCcyAmt ?? 0);
            }
        }

        // Potentially determine whether this is an outflow or inflow to compute either In or Out but not both
        // for lazy calculation
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal ForexLinkedAccountCcyAmt
        {
            get
            {
                return ForexLinkedInAccountCcyAmt + ForexLinkedOutAccountCcyAmt;
            }
        }
        // this has same sign as AccountCcyAmt
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal ForexUnlinkedAccountCcyAmt
        {
            get
            {
                return AccountCcyAmt - ForexLinkedAccountCcyAmt;
            }
        }

        public bool ForexLinkIsClosed
        {
            get
            {
                if (!IsLoading && !IsSaving && _ForexLinkIsClosed == null)
                    UpdateForexLinkIsClosed(false);
                return (bool)(_ForexLinkIsClosed ?? false);
            }
        }


        [Association("CashFlowIn-ForexSettleLinks")]
        public XPCollection<ForexSettleLink> ForexSettleLinksIn
        {
            get
            {
                return GetCollection<ForexSettleLink>("ForexSettleLinksIn");
            }
        }

        [Association("CashFlowOut-ForexSettleLinks")]
        public XPCollection<ForexSettleLink> ForexSettleLinksOut
        {
            get
            {
                return GetCollection<ForexSettleLink>("ForexSettleLinksOut");
            }
        }

        #endregion

        #region Forex Trade Calculators

        // This assumes that the CounterCcy and value date is the same for all ForexTrade
        private decimal GetForexTradeToCashFlowRate()
        {
            decimal forexRate = 0;
            ForexRate forexRateObj = null;
            Currency counterCcy = null;
            if (PrimaryCashFlowForexTrades.Count > 0)
            {
                var ft = PrimaryCashFlowForexTrades[0];
                counterCcy = ft.CounterCcy;
                if (Account.Currency != counterCcy)
                {
                    forexRateObj = GetForexRateObject(Session, ft.CounterCcy, Account.Currency, ft.PrimarySettleDate);
                    if (forexRateObj == null)
                        throw new ApplicationException(string.Format(
                            "Forex Rate to convert {0} to {1} does not exist for date {2}.",
                            counterCcy.Name, Account.Currency.Name, ft.PrimarySettleDate));
                    forexRate = forexRateObj.ConversionRate;
                }
                else
                    forexRate = 1;
            }
            else if (CounterCashFlowForexTrades.Count > 0)
            {
                var ft = CounterCashFlowForexTrades[0];
                counterCcy = ft.CounterCcy;
                if (ft.PrimaryCcy != Account.Currency)
                {
                    forexRateObj = GetForexRateObject(Session, ft.PrimaryCcy, Account.Currency, ft.CounterSettleDate);
                    if (forexRateObj == null)
                        throw new ApplicationException(string.Format(
                            "Forex Rate to convert {0} to {1} does not exist for date {2}.",
                            ft.PrimaryCcy.Name, Account.Currency.Name, ft.CounterSettleDate));
                    forexRate = forexRateObj.ConversionRate;
                }
                else
                    forexRate = 1;
            }
            return forexRate;
        }

        public void UpdatePrimaryForexTradeAmounts()
        {
            if (Account == null) return;

            bool oldCalculateEnabled = _CalculateEnabled;
            _CalculateEnabled = false;

            decimal oldCounterCcyAmt = _CounterCcyAmt;
            decimal oldAccountCcyAmt = _AccountCcyAmt;
            _AccountCcyAmt = 0;
            _CounterCcyAmt = 0;
            _FunctionalCcyAmt = 0;

            try
            {
                if (PrimaryCashFlowForexTrades.Count == 0)
                {
                    this.Delete();
                    return;
                }

                var ft1 = PrimaryCashFlowForexTrades[0];
                decimal accRate = GetForexRate(Session, ft1.PrimaryCcy, Account.Currency, ft1.PrimarySettleDate);
                decimal couRate = GetForexRate(Session, ft1.CounterCcy, CounterCcy, ft1.CounterSettleDate);

                if (ft1.PrimaryCcy.Oid == SetOfBooks.CachedInstance.FunctionalCurrency.Oid)
                {
                    foreach (var ft in PrimaryCashFlowForexTrades)
                    {
                        _FunctionalCcyAmt -= ft.PrimaryCcyAmt;
                    }
                }
                foreach (var ft in PrimaryCashFlowForexTrades)
                {
                    _AccountCcyAmt -= ft.PrimaryCcyAmt * accRate;
                    _CounterCcyAmt -= ft.CounterCcyAmt * couRate;
                }

                _FunctionalCcyAmt = Math.Round(_FunctionalCcyAmt, 2);
                _AccountCcyAmt = Math.Round(_AccountCcyAmt, 2);
                _CounterCcyAmt = Math.Round(_CounterCcyAmt, 2);

                //if (_FunctionalCcyAmt == 0 && _AccountCcyAmt == 0 && _CounterCcyAmt == 0)
                //{
                //    this.Delete();
                //    return;
                //}

                OnChanged("CounterCcyAmt", oldCounterCcyAmt, _CounterCcyAmt);
                OnChanged("AccountCcyAmt", oldAccountCcyAmt, _AccountCcyAmt);
            }
            finally
            {
                _CalculateEnabled = oldCalculateEnabled;
            }
        }

        public void UpdateCounterForexTradeAmounts()
        {
            if (Account == null) return;

            bool oldCalculateEnabled = _CalculateEnabled;
            _CalculateEnabled = false;

            decimal oldCounterCcyAmt = _CounterCcyAmt;
            decimal oldAccountCcyAmt = _AccountCcyAmt;
            _AccountCcyAmt = 0;
            _CounterCcyAmt = 0;
            _FunctionalCcyAmt = 0;

            try
            {
                if (CounterCashFlowForexTrades.Count == 0)
                {
                    this.Delete();
                    return;
                }

                var ft1 = CounterCashFlowForexTrades[0];
                decimal accRate = GetForexRate(Session, ft1.CounterCcy, Account.Currency, ft1.CounterSettleDate);
                decimal couRate = GetForexRate(Session, ft1.CounterCcy, CounterCcy, ft1.CounterSettleDate);

                if (ft1.PrimaryCcy.Oid == SetOfBooks.CachedInstance.FunctionalCurrency.Oid)
                {
                    foreach (var ft in CounterCashFlowForexTrades)
                    {
                        _FunctionalCcyAmt += ft.PrimaryCcyAmt;
                    }
                }
                foreach (var ft in CounterCashFlowForexTrades)
                {
                    _AccountCcyAmt += ft.CounterCcyAmt * accRate;
                    _CounterCcyAmt += ft.CounterCcyAmt * couRate;
                }

                _FunctionalCcyAmt = Math.Round(_FunctionalCcyAmt, 2);
                _AccountCcyAmt = Math.Round(_AccountCcyAmt, 2);
                _CounterCcyAmt = Math.Round(_CounterCcyAmt, 2);

                //if (_FunctionalCcyAmt == 0 && _AccountCcyAmt == 0 && _CounterCcyAmt == 0)
                //{
                //    this.Delete();
                //    return;
                //}

                OnChanged("CounterCcyAmt", oldCounterCcyAmt, _CounterCcyAmt);
                OnChanged("AccountCcyAmt", oldAccountCcyAmt, _AccountCcyAmt);
 
            }
            finally
            {
                _CalculateEnabled = oldCalculateEnabled;
            }
        }

        public long ForexSettleGroupId
        {
            get
            {
                return _ForexSettleGroupId;
            }
            set
            {
                SetPropertyValue("ForexSettleGroupId", ref _ForexSettleGroupId, value);
            }
        }
        #endregion

        #region Forex Calculators

        private bool IsForexAccount
        {
            get
            {
                if (SetOfBooks.CachedInstance == null || this.Account == null) return false;
                if (SetOfBooks.CachedInstance.FunctionalCurrency.Oid != this.Account.Currency.Oid) return true;
                return false;
            }
        }

        private void UpdateForexLinkIsClosed(bool forceChangeEvents)
        {
            // get accounts with currency that don't equal functional currency --> ForexSettleLink.FcaAccounts
            // set IsClosed = true

            if (AccountCcyAmt - ForexLinkedOutAccountCcyAmt - ForexLinkedInAccountCcyAmt == 0
                || !IsForexAccount)
            {
                _ForexLinkIsClosed = true;
                if (forceChangeEvents)
                    OnChanged("ForexLinkIsClosed");
            }
        }

        public void UpdateForexLinkedInAmt(bool forceChangeEvents)
        {
            decimal? oldAmount = _ForexLinkedInAccountCcyAmt;
            decimal tempTotal = 0;
            foreach (ForexSettleLink detail in ForexSettleLinksIn)
                tempTotal += detail.AccountCcyAmt;
            _ForexLinkedInAccountCcyAmt = tempTotal;
            if (forceChangeEvents)
                OnChanged("ForexLinkedInAccountCcyAmt", oldAmount, _ForexLinkedInAccountCcyAmt);
        }


        public void UpdateForexLinkedOutAmt(bool forceChangeEvents)
        {
            decimal? oldAmount = _ForexLinkedOutAccountCcyAmt;
            decimal tempTotal = 0;
            foreach (ForexSettleLink detail in ForexSettleLinksOut)
                // we subtract because the links are based on linkedins and we want linkedout
                tempTotal -= detail.AccountCcyAmt; 
            _ForexLinkedOutAccountCcyAmt = tempTotal;
            if (forceChangeEvents)
                OnChanged("ForexLinkedOutAccountCcyAmt", oldAmount, _ForexLinkedOutAccountCcyAmt);
        }

        private decimal GetUnlinkedFunctionalCcyAmt()
        {
            var defaultRate = FunctionalCcyAmt / AccountCcyAmt;
            decimal unlinkedAmt = ForexUnlinkedAccountCcyAmt * defaultRate;

            // use ForexRates table
            if (TranDate != default(DateTime))
            {
                var rateObj = GetForexRateObject(Session, Account.Currency, SetOfBooks.CachedInstance.FunctionalCurrency, (DateTime)TranDate);
                if (rateObj != null)
                {
                    unlinkedAmt = ForexUnlinkedAccountCcyAmt * (decimal)rateObj.ConversionRate;
                }
            }
            return unlinkedAmt;
        }

        public void UpdateForexFunctionalCcyAmt(bool forceChangeEvents)
        {
            if (ForexSettleLinksOut.Count == 0
                || !IsForexAccount) return;

            // Linked Rate
            decimal oldAmount = _FunctionalCcyAmt;
            decimal tempTotal = 0;
            foreach (ForexSettleLink detail in ForexSettleLinksOut)
                // we subtract because the links are based on linkedins and we want linkedout
                tempTotal -= detail.FunctionalCcyAmt;

            // Spot Rate
            tempTotal += GetUnlinkedFunctionalCcyAmt();

            _FunctionalCcyAmt = Math.Round(tempTotal, 2);
            if (forceChangeEvents)
                OnChanged("FunctionalCcyAmt", oldAmount, _FunctionalCcyAmt);
            UpdateBankStmtsFunctionalCcyAmt();
        }

        private void UpdateBankStmtsFunctionalCcyAmt()
        {
            // why is BankStmts.Count == 0?
            foreach (BankStmt detail in BankStmts)
            {
                detail.FunctionalCcyAmt = detail.TranAmount * FunctionalCcyAmt / AccountCcyAmt;
            }
        }



        #endregion

        protected override void OnLoaded()
        {
            Reset();
            base.OnLoaded();
        }

        protected void Reset()
        {
            _ForexLinkedInAccountCcyAmt = null;
            _ForexLinkedOutAccountCcyAmt = null;
            _ForexLinkIsClosed = null;
            ForexSettleLinksIn.Reload();
            ForexSettleLinksOut.Reload();
        }

        public static DateTime GetMaxActualTranDate(Session session)
        {
            DateTime? res = (DateTime?)session.Evaluate<CashFlow>(CriteriaOperator.Parse("Max(TranDate)"),
                CriteriaOperator.Parse("Status = ?", CashFlowStatus.Actual));
            if (res == null)
                return default(DateTime);
            return (DateTime)res;
        }

        #region Fix Cash Flow Algorithm

        public static void FixCashFlows(XafApplication application, XPObjectSpace objSpace, CashFlowFixParam paramObj)
        {
            var algo = new FixCashFlowsAlgorithm(application, objSpace, paramObj);
            algo.FixCashFlows();
        }

        private class FixCashFlowsAlgorithm
        {
            private XPObjectSpace objSpace;
            private CashFlowFixParam paramObj;
            private Activity paramApReclassActivity;
            private Counterparty defaultCounterparty;
            private CashForecastFixTag reversalFixTag;
            private CashForecastFixTag revRecFixTag;
            private CashForecastFixTag resRevRecFixTag;
            private CashForecastFixTag payrollFixTag;
            private XafApplication application;

            public FixCashFlowsAlgorithm(XafApplication app, XPObjectSpace objSpace, CashFlowFixParam paramObj)
            {
                this.objSpace = objSpace;
                this.paramObj = paramObj;
                this.application = app;
                // default values and parameters
                paramApReclassActivity = objSpace.GetObjectByKey<Activity>(objSpace.GetKeyValue(paramObj.ApReclassActivity));
                defaultCounterparty = objSpace.FindObject<Counterparty>(
                 CriteriaOperator.Parse("Name LIKE ?", "UNDEFINED"));
                reversalFixTag = objSpace.FindObject<CashForecastFixTag>(
                    CriteriaOperator.Parse("Name = 'R'"));
                revRecFixTag = objSpace.FindObject<CashForecastFixTag>(
                    CriteriaOperator.Parse("Name = 'RR'"));
                resRevRecFixTag = objSpace.FindObject<CashForecastFixTag>(
                    CriteriaOperator.Parse("Name = 'RRR'"));
                payrollFixTag = objSpace.FindObject<CashForecastFixTag>(
                    CriteriaOperator.Parse("Name = 'PY'"));
            }

            public void FixCashFlows()
            {
                bool bTriggersEnabled = AppSettings.UserTriggersEnabled;
                
                cashFlowsToDelete.Clear();

                try
                {
                    AppSettings.UserTriggersEnabled = false;

                    // delete existing fixes that are not valid due to potential application bug, i.e. orphans.
                    var cashFlowFixes = objSpace.GetObjects<CashFlow>(
                        CriteriaOperator.Parse(
                        "Snapshot = ? And Fix In (?,?,?) And ParentCashFlow Is Null",
                        GetCurrentSnapshot(objSpace.Session), reversalFixTag, revRecFixTag, resRevRecFixTag));
                    objSpace.Delete(cashFlowFixes);

                    var cashFlows = objSpace.GetObjects<CashFlow>(CriteriaOperator.Parse(
                    "TranDate >= ? And TranDate <= ? And Fix.FixTagType != ?"
                    + " And (Not IsFixUpdated Or IsFixUpdated Is Null)"
                    + " And Snapshot = ?",
                    paramObj.FromDate,
                    paramObj.ToDate,
                    CashForecastFixTagType.Ignore,
                    GetCurrentSnapshot(objSpace.Session)));

                    // The fixee may require updating even if IsFixUpdated == True,
                    // if a new fixer is entered with a criteria that fits the fixee.
                    // This code will update the fixee in such a case.
                    foreach (var fixer in cashFlows)
                    {
                        var fixees = cashFlows.Where(delegate(CashFlow c)
                        {
                            return GetFixCriteria(c, fixer);
                        });
                        foreach (var fixee in fixees)
                        {
                            // since one fixee can have many fixers, we avoid
                            // running the algorithm twice on the same fixee
                            if (fixee.IsFixUpdated) continue;
                            FixFixee(cashFlows, fixee);
                        }
                        fixer.IsFixUpdated = true;
                        fixer.Save();
                    }

                    foreach (var fixee in cashFlows)
                    {
                        if (fixee.IsFixUpdated) continue;
                        FixFixee(cashFlows, fixee);
                    }

                    objSpace.Delete(cashFlowsToDelete);
                    cashFlowsToDelete.Clear();
                    objSpace.CommitChanges();
                }
                finally
                {
                    AppSettings.UserTriggersEnabled = bTriggersEnabled;
                }
            }


            private List<CashFlow> cashFlowsToDelete = new List<CashFlow>();

            // TODO: get session from fixee instead of objspace
            private void FixFixee(IList<CashFlow> cashFlows, CashFlow fixee)
            {
                
                // delete existing fixes
                foreach (var child in fixee.ChildCashFlows)
                {
                    if (child != fixee)
                        cashFlowsToDelete.Add(child);
                }
                //while (fixee.ChildCashFlows.Count != 0)
                //    fixee.ChildCashFlows[0].Delete();

                var fixers = cashFlows.Where(delegate(CashFlow c)
                {
                    return GetFixCriteria(fixee, c);
                }).OrderBy(c => c.TranDate);

                var fixersWithCcy = fixers.Where(c => c.CounterCcy == fixee.CounterCcy);

                // fix with CounterCcy but use AUD if not found
                CashFlow fixer = fixersWithCcy.FirstOrDefault();
                if (fixer == null)
                    fixer = fixers.FirstOrDefault();

                if (fixer == null) return;

                // base
                fixee.Fixer = fixer;

                // reversal
                var revFix = objSpace.CreateObject<CashFlow>();
                revFix.ParentCashFlow = fixee;
                revFix.Account = fixee.Account.FixAccount;
                revFix.Activity = fixee.FixActivity;
                revFix.TranDate = fixer.TranDate;
                revFix.AccountCcyAmt = -fixee.AccountCcyAmt;
                revFix.FunctionalCcyAmt = -fixee.FunctionalCcyAmt;
                revFix.CounterCcyAmt = -fixee.CounterCcyAmt;
                revFix.CounterCcy = fixer.CounterCcy;
                revFix.Source = fixee.FixActivity.FixSource;
                revFix.Fix = reversalFixTag;

                // Reversal logic for AP Lockdown (i.e. payroll is excluded)

                if (fixee.TranDate <= paramObj.ApayableLockdownDate
                     && fixer.Fix.FixTagType == CashForecastFixTagType.Allocate
                    && fixee.Fix != payrollFixTag)
                {
                    revFix.IsReclass = true;

                    #region Accounts Payable
                    // Reverse Reversal To Reclass where AP <= 2 weeks and Fixer is Allocate
                    // i.e. reversal will have no effect in this case
                    var revRecFix = objSpace.CreateObject<CashFlow>();
                    CopyCashFlowFixDefaults(revFix, revRecFix, -1);
                    revRecFix.Activity = paramApReclassActivity;
                    revRecFix.Fix = revRecFixTag;
                    revRecFix.IsReclass = true;

                    // Reverse AP 'reversal reclass' back into week 3
                    var resrevrecFix = objSpace.CreateObject<CashFlow>();
                    CopyCashFlowFixDefaults(revRecFix, resrevrecFix, -1);
                    resrevrecFix.TranDate = paramObj.ApayableNextLockdownDate;
                    resrevrecFix.Fix = resRevRecFixTag;
                    resrevrecFix.IsReclass = false;
                    #endregion

                    #region Allocate
                    // Reverse 'Allocate' To Reclass where <= 2 week 
                    // TODO: get Fix Source instead of underlying source
                    var revRecFixer = objSpace.CreateObject<CashFlow>();
                    CopyCashFlowFixDefaults(fixer, revRecFixer, -1);
                    revRecFixer.Fix = revRecFixTag;
                    revRecFixer.TranDate = paramObj.ApayableLockdownDate;
                    revRecFixer.Activity = paramApReclassActivity;
                    revRecFixer.IsReclass = true;

                    // Reverse "Reverse Allocate To Reclass" back into week 3
                    var resRevRecFix = objSpace.CreateObject<CashFlow>();
                    CopyCashFlowFixDefaults(revRecFixer, resRevRecFix, -1);
                    resRevRecFix.Fix = resRevRecFixTag;
                    resRevRecFix.TranDate = paramObj.ApayableNextLockdownDate;
                    resRevRecFix.IsReclass = false;
                    #endregion

                    revRecFix.Save();
                    resrevrecFix.Save();
                    revRecFixer.Save();
                    resRevRecFix.Save();
                }
                revFix.Save();
                fixee.IsFixUpdated = true;
                fixee.Save();
            }

            private bool GetFixCriteria(CashFlow fixee, CashFlow fixer)
            {
                return fixee.DateUnFix >= fixer.FixFromDate && fixee.DateUnFix <= fixer.FixToDate
                            && fixee.FixActivity == fixer.FixActivity
                    // should we do activity = fixactivity as well?
                            && fixer.Status == CashFlowStatus.Forecast
                            && fixer.FixRank > fixee.FixRank
                            && (fixer.Counterparty == null || fixer.Counterparty == defaultCounterparty
                            || fixee.Counterparty == null && fixer.Counterparty == null
                            || fixee.Counterparty != null && fixer.Counterparty == fixee.Counterparty.FixCounterparty)
                            && fixee.Account != null && fixee.Account.FixAccount == fixer.Account.FixAccount;
            }

            private void CopyCashFlowFixDefaults(CashFlow source, CashFlow target, decimal amtFactor = 1)
            {
                target.ParentCashFlow = source.ParentCashFlow;
                target.TranDate = source.TranDate;
                target.ParentCashFlow = source.ParentCashFlow;
                target.Fixer = source.Fixer;
                target.Activity = source.Activity;
                target.Account = source.Account;
                target.AccountCcyAmt = amtFactor * source.AccountCcyAmt;
                target.FunctionalCcyAmt = amtFactor * source.FunctionalCcyAmt;
                target.CounterCcyAmt = amtFactor * source.CounterCcyAmt;
                target.Source = source.Source;
                target.CounterCcy = source.CounterCcy;
            }
        }
        #endregion

        #region Snapshot
        public static CashFlowSnapshot SaveSnapshot(Session session, DateTime fromTranDate)
        {
            var snapshot = new CashFlowSnapshot(session);
            snapshot.FromDate = fromTranDate;
            snapshot.Name = string.Format("Snapshot {0:d-MMM-yy HH:mm:ss}", DateTime.Now);

            var criteria = CriteriaOperator.Parse("Snapshot = ?",
                session.GetObjectByKey<CashFlowSnapshot>(SetOfBooks.CachedInstance.CurrentCashFlowSnapshot.Oid));
            if (fromTranDate != default(DateTime))
                criteria = criteria & CriteriaOperator.Parse("TranDate >= ?", fromTranDate);
            var cashFlows = session.GetObjects(session.GetClassInfo(typeof(CashFlow)),
                            criteria,
                            new SortingCollection(null), 0, false, true);
            foreach (CashFlow cf in cashFlows)
            {
                var cfShot = (CashFlow)CloneIXPSimpleObjectHelper.CloneLocal(cf);
                cfShot.Snapshot = snapshot;
                cfShot.OrigSequentialNumber = cf.SequentialNumber;
            }

            return snapshot;
        }

        public static CashFlowSnapshot SaveForecast(XPObjectSpace objSpace, bool commit = true)
        {
            var session = ((XPObjectSpace)objSpace).Session;
            var minDate = (DateTime)(session.Evaluate<CashFlow>(CriteriaOperator.Parse("Min(TranDate)"),
                CriteriaOperator.Parse("Status = ?", CashFlowStatus.Forecast)) ?? default(DateTime));
            var result = CashFlow.SaveSnapshot(session, minDate);
            if (commit)
                objSpace.CommitChanges();
            return result;
        }

        #endregion

        public static class FieldNames
        {
            public const string TranDate = "TranDate";
            public const string Snapshot = "Snapshot";
            public const string Source = "Source";
        }

    }
    public enum CashFlowStatus
    {
        Forecast=0,
        Actual=1
    }
}
