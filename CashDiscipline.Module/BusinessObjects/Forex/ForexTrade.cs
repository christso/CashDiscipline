using CashDiscipline.Common;
using CashDiscipline.Module.Attributes;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Logic.Forex;
using CashDiscipline.Module.Logic.SqlServer;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;

namespace CashDiscipline.Module.BusinessObjects.Forex
{
    [ModelDefault("ImageName", "BO_List")]
    [ModelDefault("IsCloneable", "True")]
    [DefaultProperty("Oid")]
    [ModelDefault("IsFooterVisible", "True")]
    [AutoColumnWidth(false)]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    public class ForexTrade : BaseObject, ICalculateToggleObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public ForexTrade(Session session)
            : base(session)
        {
            _CalculateEnabled = true;
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if (AppBehaviour.UserTriggersEnabled)
                InitializeValues();
        }

        private string _ConfDealNum;
        private ForexEventType _EventType;
        private Currency _PrimaryCcy;
        private decimal _PrimaryCcyAmt;
        private decimal _Rate;
        private Currency _CounterCcy;
        private decimal _CounterCcyAmt;
        private DateTime _TradeDate;
        private DateTime _ValueDate;
        private ForexCounterparty _Counterparty;
        private Activity _HedgeActivity;
        private string _Description;
        private string _CustomRef;
        private DateTime _OrigTradeDate;
        private DateTime _PrimarySettleDate;
        private DateTime _CounterSettleDate;
        private Account _PrimarySettleAccount;
        private Account _CounterSettleAccount;
        private long _SettleGroupId;
        private string _MtmDealNum;
        private DateTime _CreationDate;
        private CashFlow _PrimaryCashFlow;
        private CashFlow _CounterCashFlow;
        private ForexTrade _OrigTrade;
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

        public string ConfDealNum
        {
            get
            {
                return _ConfDealNum;
            }
            set
            {
                SetPropertyValue("ConfDealNum", ref _ConfDealNum, value);
            }
        }

        public ForexEventType EventType
        {
            get
            {
                return _EventType;
            }
            set
            {
                SetPropertyValue("EventType", ref _EventType, value);
            }
        }

        [RuleRequiredField("ForexTrade.PrimaryCcy_RuleRequiredField", DefaultContexts.Save)]
        [ImmediatePostData(true)]
        public Currency PrimaryCcy
        {
            get
            {
                return _PrimaryCcy;
            }
            set
            {
                if (SetPropertyValue("PrimaryCcy", ref _PrimaryCcy, value))
                {
                    if (!IsLoading && !IsSaving && CalculateEnabled)
                    {
                        UpdatePrimarySettleAccount();
                    }
                }
            }
        }

        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        [ImmediatePostData(true)]
        [EditorAlias("Xafology_DecimalActionPropertyEditor")]
        public decimal PrimaryCcyAmt
        {
            get
            {
                return _PrimaryCcyAmt;
            }
            set
            {
                if (SetPropertyValue("PrimaryCcyAmt", ref _PrimaryCcyAmt, value))
                {
                    if (!IsLoading && !IsSaving && CalculateEnabled)
                    {
                        UpdateCounterCcyAmt();
                    }
                }
            }
        }

        [ModelDefault("EditMask", "n5")]
        [ModelDefault("DisplayFormat", "n5")]
        [DbType("decimal(19, 6)")]
        [ImmediatePostData(true)]
        [EditorAlias("Xafology_DecimalActionPropertyEditor")]
        public decimal Rate
        {
            get
            {
                return _Rate;
            }
            set
            {
                if (SetPropertyValue("Rate", ref _Rate, value))
                {
                    if (!IsLoading && !IsSaving && CalculateEnabled)
                    {
                        var obj = this;
                        var fromCcy = CounterCcy;
                        var fromAmt = CounterCcyAmt;
                        if (fromCcy == null)
                        {
                            return;
                        }
                        if (_Rate != 0)
                        {
                            var toAmt = Math.Round(fromAmt / _Rate, 2);
                            obj.SetPropertyValue("PrimaryCcyAmt", ref obj._PrimaryCcyAmt, toAmt);
                        }
                    }
                }
            }
        }

        [RuleRequiredField("ForexTrade.CounterCcy_RuleRequiredField", DefaultContexts.Save)]
        [ImmediatePostData(true)]
        public Currency CounterCcy
        {
            get
            {
                return _CounterCcy;
            }
            set
            {
                var oldCounterCcy = _CounterCcy;
                if (SetPropertyValue("CounterCcy", ref _CounterCcy, value))
                {
                    if (!IsLoading && !IsSaving && CalculateEnabled)
                    {
                        var obj = this;
                        var fromCcy = _CounterCcy;
                        var fromAmt = _CounterCcyAmt;
                        if (fromCcy == null)
                        {
                            return;
                        }
                        else if (oldCounterCcy == null)
                        {
                            UpdatePrimaryCcyAmt();
                        }
                        else if (obj.ValueDate != default(DateTime))
                        {
                            //UpdatePrimaryCcyAmt();
                            CalculatePrimaryCcyAmt(fromCcy, fromAmt);
                        }
                        UpdateCounterSettleAccount();
                    }
                }
            }
        }

        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        [ImmediatePostData(true)]
        [EditorAlias("Xafology_DecimalActionPropertyEditor")]
        public decimal CounterCcyAmt
        {
            get
            {
                return _CounterCcyAmt;
            }
            set
            {
                if (SetPropertyValue("CounterCcyAmt", ref _CounterCcyAmt, value))
                {
                    if (!IsLoading && !IsSaving && CalculateEnabled)
                    {
                        ForexTradeLogic.UpdateReverseTrade(this);
                        UpdatePrimaryCcyAmt();
                    }
                }
            }
        }

        [ModelDefault("EditMask", "dd-MMM-yy HH:mm:ss")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime TradeDate
        {
            get
            {
                return _TradeDate;
            }
            set
            {
                if (SetPropertyValue("TradeDate", ref _TradeDate, value))
                {
                    if (!IsLoading && !IsSaving && CalculateEnabled && OrigTrade == null)
                    {
                        _OrigTradeDate = value;
                        OnChanged("OrigTradeDate");
                    }
                }

            }
        }

        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [RuleRequiredField("ForexTrade.ValueDate_RuleRequiredField", DefaultContexts.Save, SkipNullOrEmptyValues = false)]
        public DateTime ValueDate
        {
            get
            {
                return _ValueDate;
            }
            set
            {
                if (SetPropertyValue("ValueDate", ref _ValueDate, value.Date))
                {
                    if (!IsLoading && !IsSaving && CalculateEnabled)
                    {
                        SetPropertyValue("CounterSettleDate", ref _CounterSettleDate, _ValueDate);
                        SetPropertyValue("PrimarySettleDate", ref _PrimarySettleDate, _ValueDate);
                    }
                }
            }
        }
        [RuleRequiredField("ForexTrade.Counterparty_RuleRequiredField", DefaultContexts.Save)]
        [ImmediatePostData(true)]
        public ForexCounterparty Counterparty
        {
            get
            {
                return _Counterparty;
            }
            set
            {
                if (SetPropertyValue("Counterparty", ref _Counterparty, value))
                {
                    if (!IsSaving && !IsLoading && CalculateEnabled)
                    {
                        UpdateCounterSettleAccount();
                        UpdatePrimarySettleAccount();
                    }
                }
            }
        }

        private ForexStatus _Status;
        public ForexStatus Status
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

        public Activity HedgeActivity
        {
            get
            {
                return _HedgeActivity;
            }
            set
            {
                SetPropertyValue("HedgeActivity", ref _HedgeActivity, value);
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
        public string CustomRef
        {
            get
            {
                return _CustomRef;
            }
            set
            {
                SetPropertyValue("CustomRef", ref _CustomRef, value);
            }
        }
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime OrigTradeDate
        {
            get
            {
                return _OrigTradeDate;
            }
            set
            {
                SetPropertyValue("OrigTradeDate", ref _OrigTradeDate, value);
            }
        }
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime PrimarySettleDate
        {
            get
            {
                return _PrimarySettleDate;
            }
            set
            {
                SetPropertyValue("PrimarySettleDate", ref _PrimarySettleDate, value);
            }
        }
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime CounterSettleDate
        {
            get
            {
                return _CounterSettleDate;
            }
            set
            {
                SetPropertyValue("CounterSettleDate", ref _CounterSettleDate, value);
            }
        }
        public Account PrimarySettleAccount
        {
            get
            {
                return _PrimarySettleAccount;
            }
            set
            {
                SetPropertyValue("PrimarySettleAccount", ref _PrimarySettleAccount, value);
            }
        }
        public Account CounterSettleAccount
        {
            get
            {
                return _CounterSettleAccount;
            }
            set
            {
                SetPropertyValue("CounterSettleAccount", ref _CounterSettleAccount, value);
            }
        }
        [ModelDefault("EditMask", "dd-MMM-yy HH:mm:ss")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy HH:mm:ss")]
        [ModelDefault("AllowEdit", "False")]
        public DateTime CreationDate
        {
            get
            {
                return _CreationDate;
            }
            set
            {
                SetPropertyValue("CreationDate", ref _CreationDate, value);
            }
        }
        public long SettleGroupId
        {
            get
            {
                return _SettleGroupId;
            }
            set
            {
                SetPropertyValue("SettleGroupId", ref _SettleGroupId, value);
            }
        }
        public string MtmDealNum
        {
            get
            {
                return _MtmDealNum;
            }
            set
            {
                SetPropertyValue("MtmDealNum", ref _MtmDealNum, value);
            }
        }

        [Association("PrimaryCashFlow-ForexTrades")]
        public CashFlow PrimaryCashFlow
        {
            get
            {
                return _PrimaryCashFlow;
            }
            set
            {
                SetPropertyValue("PrimaryCashFlow", ref _PrimaryCashFlow, value);
            }
        }

        [Association("CounterCashFlow-ForexTrades")]
        public CashFlow CounterCashFlow
        {
            get
            {
                return _CounterCashFlow;
            }
            set
            {
                SetPropertyValue("CounterCashFlow", ref _CounterCashFlow, value);
            }
        }

        public ForexTrade OrigTrade
        {
            get
            {
                return _OrigTrade;
            }
            set
            {
                SetPropertyValue("OrigTrade", ref _OrigTrade, value);
            }
        }

        private ForexTrade _ReverseTrade;
        public ForexTrade ReverseTrade
        {
            get
            {
                return _ReverseTrade;
            }
            set
            {
                SetPropertyValue("ReverseTrade", ref _ReverseTrade, value);
            }
        }

        #region Settle Account Logic

        public void UpdatePrimarySettleAccount()
        {
            var account = GetDefaultSettleAccount(PrimaryCcy, Counterparty);
            if (account == null) return;
            SetPropertyValue("PrimarySettleAccount", ref _PrimarySettleAccount, account);
        }

        public void UpdateCounterSettleAccount()
        {
            var account = GetDefaultSettleAccount(CounterCcy, Counterparty);
            if (account == null) return;
            SetPropertyValue("CounterSettleAccount", ref _CounterSettleAccount, account);
        }

        private Account GetDefaultSettleAccount(Currency currency, ForexCounterparty forexCounterparty)
        {
            var ssa = Session.FindObject<ForexStdSettleAccount>(CriteriaOperator.Parse(
                "Currency = ? And Counterparty = ?", currency, forexCounterparty));
            if (ssa != null && ssa.Account != null)
                return ssa.Account;
            return null;
        }
        #endregion

        #region Forex Rate Logic
        public void UpdateRate()
        {
            var fromAmt = CounterCcyAmt;
            var fromCcy = CounterCcy;

            if (this.PrimaryCcyAmt != 0.00M && this.CounterCcyAmt != 0.00M)
            {
                this.SetPropertyValue("Rate", ref this._Rate, Math.Round(this.CounterCcyAmt / this.PrimaryCcyAmt, 5));
            }
            else if (this.ValueDate != default(DateTime))
            {
                var toCcy = SetOfBooks.GetInstance(fromCcy.Session).FunctionalCurrency;
                var ratethis = GetForexRateObject(fromCcy, toCcy, this.ValueDate);
                if (ratethis != null)
                {
                    var value = Math.Round(fromAmt * (decimal)ratethis.ConversionRate, 2);
                    this.SetPropertyValue("PrimaryCcyAmt", ref this._PrimaryCcyAmt, value);
                }
            }
        }

        public void UpdatePrimaryCcyAmt()
        {
            var fromCcy = CounterCcy;
            var fromAmt = CounterCcyAmt;
            if (Rate != 0.00M)
            {
                SetPropertyValue("PrimaryCcyAmt", ref _PrimaryCcyAmt, Math.Round(fromAmt / Rate, 2));
            }
            else if (fromCcy == null)
            {
                return;
            }
            else if (SetOfBooks.CachedInstance.FunctionalCurrency.Oid == fromCcy.Oid)
            {
                SetPropertyValue("PrimaryCcyAmt", ref _PrimaryCcyAmt, Math.Round(fromAmt, 2));
                SetPropertyValue("Rate", ref this._Rate, 1);
            }
            else if (this.ValueDate != default(DateTime))
            {
                CalculatePrimaryCcyAmt(fromCcy, fromAmt);
            }
        }

        public void CalculatePrimaryCcyAmt(Currency fromCcy, decimal fromAmt)
        {
            var toCcy = SetOfBooks.GetInstance(fromCcy.Session).FunctionalCurrency;
            if (fromCcy.Session != toCcy.Session)
                throw new InvalidOperationException("Both currencies must be in the same session.");

            var rateObj = GetForexRateObject(fromCcy, toCcy, ValueDate);
            if (rateObj != null)
            {
                var usedRate = 1 / rateObj.ConversionRate;
                SetPropertyValue("Rate", ref _Rate, usedRate);
                SetPropertyValue("PrimaryCcyAmt", ref _PrimaryCcyAmt, Math.Round(fromAmt / usedRate, 2));
            }
        }

        public void UpdateCounterCcyAmt()
        {
            var fromCcy = PrimaryCcy;
            var fromAmt = PrimaryCcyAmt;
            if (this.Rate != 0.00M)
            {
                SetPropertyValue("CounterCcyAmt", ref _CounterCcyAmt, Math.Round(fromAmt * this.Rate, 2));
            }
            else if (fromCcy == null)
            {
                return;
            }
            else if (SetOfBooks.CachedInstance.FunctionalCurrency.Oid == fromCcy.Oid)
            {
                SetPropertyValue("CounterCcyAmt", ref _CounterCcyAmt, Math.Round(fromAmt, 2));
                SetPropertyValue("Rate", ref this._Rate, 1);
            }
            else if (this.ValueDate != default(DateTime))
            {
                CalculateCounterCcyAmt(fromCcy, fromAmt);
            }
        }

        public void CalculateCounterCcyAmt(Currency fromCcy, decimal fromAmt)
        {
            var toCcy = SetOfBooks.GetInstance(fromCcy.Session).FunctionalCurrency;
            if (fromCcy.Session != toCcy.Session)
                throw new InvalidOperationException("Both currencies must be in the same session.");

            var rateObj = GetForexRateObject(fromCcy, toCcy, ValueDate);
            if (rateObj != null)
            {
                var usedRate = rateObj.ConversionRate;
                SetPropertyValue("Rate", ref _Rate, usedRate);
                SetPropertyValue("PrimaryCcyAmt", ref _PrimaryCcyAmt, Math.Round(fromAmt * usedRate, 2));
            }
        }

        private static ForexRate GetForexRateObject(Currency fromCcy, Currency toCcy, DateTime convDate)
        {
            return ForexRate.GetForexRateObject(fromCcy, toCcy, convDate);
        }

        #endregion

        public void InitializeValues()
        {
            OrigTrade = this;
            var sqlUtil = new SqlQueryUtil(Session);
            CreationDate = sqlUtil.GetDate();
            TradeDate = CreationDate.Date;
            OrigTradeDate = CreationDate;
            ValueDate = CreationDate.Date;
            PrimarySettleDate = ValueDate;
            CounterSettleDate = ValueDate;

            // Initialise Primary Currency and Settlement Account
            PrimaryCcy = Session.GetObjectByKey<Currency>(
                SetOfBooks.CachedInstance.FunctionalCurrency.Oid);
            // TODO: default settle account if Counterparty is UNDEFINED
            //PrimarySettleAccount = GetDefaultSettleAccount(PrimaryCcy, Counterparty);
        }

        protected override void OnSaving()
        {
            base.OnSaving();
        }

        public class FieldNames
        {
            public const string CounterCcyAmt = "CounterCcyAmt";
            public const string PrimaryCcyAmt = "PrimaryCcyAmt";
            public const string Rate = "Rate";
        }
    }

    public enum ForexEventType
    {
        Buy,
        Sell,
        Predeliver,
        Amend,
        Cancel,
        Extend,
        Reclass,
        Gain,
        Settle
    }

    public enum ForexStatus
    {
        Entered,
        Prepared,
        Proposed,
        Scheduled,
        Settled
    }
}
