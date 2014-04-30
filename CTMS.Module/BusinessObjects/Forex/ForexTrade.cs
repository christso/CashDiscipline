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
using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.BusinessObjects.Market;
using DevExpress.ExpressApp.Xpo;
using GenerateUserFriendlyId.Module.BusinessObjects;
using System.Collections;

namespace CTMS.Module.BusinessObjects.Forex
{
    [ModelDefault("IsCloneable", "True")]
    [DefaultProperty("TradeId")]
    public class ForexTrade : UserFriendlyIdPersistentObject, ICalculateToggleObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public ForexTrade(Session session)
            : base(session)
        {
            _CalculateEnabled = true;
            _SyncEnabled = true;
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if (AppSettings.UserTriggersEnabled)
                InitDefaultValues();
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
        private CashFlow _OldPrimaryCashFlow;
        private CashFlow _OldCounterCashFlow;
        private ForexTrade _OrigTrade;
        private bool _CalculateEnabled;
        private bool _SyncEnabled;

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

        /// <summary>
        /// Forex Trade To Cash Flow Link
        /// </summary>
        public bool SyncEnabled
        {
            get
            {
                return _SyncEnabled;
            }
            set
            {
                _SyncEnabled = value;
            }
        }

        [PersistentAlias("concat('FT', ToStr(SequentialNumber))")]
        public string TradeId
        {
            get
            {
                return Convert.ToString(EvaluateAlias("TradeId"));
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
                if (SetPropertyValue("EventType", ref _EventType, value))
                {
                }
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
                        UpdateCashFlowForecast();
                    }
                }
            }
        }

        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
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
                        UpdateCashFlowForecast();
                }
            }
        }

        [ModelDefault("EditMask", "n5")]
        [ModelDefault("DisplayFormat", "n5")]
        [DbType("decimal(19, 6)")]
        [ImmediatePostData(true)]
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
                            UpdateCashFlowForecast();
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
                        else if (SetOfBooks.CachedInstance.FunctionalCurrency.Oid == fromCcy.Oid)
                        {
                            obj.PrimaryCcyAmt = Math.Round(fromAmt, 2);
                            obj.SetPropertyValue("Rate", ref obj._Rate, 1);
                        }
                        else if (obj.ValueDate != default(DateTime))
                        {
                            CalculatePrimaryCcyAmt(obj, fromCcy, fromAmt);
                        }

                        UpdateCounterSettleAccount();
                        UpdateCashFlowForecast();
                    }
                }
            }
        }

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
                if (SetPropertyValue("CounterCcyAmt", ref _CounterCcyAmt, value))
                {
                    if (!IsLoading && !IsSaving && CalculateEnabled)
                    {
                        var obj = this;
                        var fromCcy = CounterCcy;
                        var fromAmt = CounterCcyAmt;
                        if (obj.Rate != 0.00M)
                        {
                            obj.PrimaryCcyAmt = Math.Round(fromAmt / obj.Rate, 2);
                        }
                        else if (fromCcy == null)
                        {
                            return;
                        }
                        else if (SetOfBooks.CachedInstance.FunctionalCurrency.Oid == fromCcy.Oid)
                        {
                            obj.PrimaryCcyAmt = Math.Round(fromAmt, 2);
                            obj.SetPropertyValue("Rate", ref obj._Rate, 1);
                        }
                        else if (obj.ValueDate != default(DateTime))
                        {
                            CalculatePrimaryCcyAmt(obj, fromCcy, fromAmt);
                        }
                        UpdateCashFlowForecast();
                    }
                }
            }
        }

        [ModelDefault("EditMask", "dd-MMM-yy HH:mm:ss")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy HH:mm:ss")]
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
                    if (!IsLoading && !IsSaving && CalculateEnabled)
                    {
                        _OrigTradeDate = value;
                        OnChanged("OrigTradeDate");
                    }
                }

            }
        }

        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [RuleRequiredField("ForexTrade.ValueDate_RuleRequiredField", DefaultContexts.Save, SkipNullOrEmptyValues=false)]
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
                        UpdateCashFlowForecast();
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
                        UpdateCashFlowForecast();
                    }
                }
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
                if (!IsLoading && !IsSaving && CalculateEnabled)
                    UpdateCashFlowForecast();
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
                if (!IsLoading && !IsSaving && CalculateEnabled)
                    UpdateCashFlowForecast();
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
                if (SetPropertyValue("PrimarySettleAccount", ref _PrimarySettleAccount, value))
                {
                    if (!IsLoading && !IsSaving && CalculateEnabled)
                    {
                        UpdatePrimaryCashFlowAccount();
                        UpdateCashFlowForecast();
                    }
                }
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
                if (SetPropertyValue("CounterSettleAccount", ref _CounterSettleAccount, value))
                {
                    if (!IsLoading && !IsSaving && CalculateEnabled)
                    {
                        UpdateCounterCashFlowAccount();
                        UpdateCashFlowForecast();
                    }
                }
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
                if (SetPropertyValue("SettleGroupId", ref _SettleGroupId, value))
                {
                    if (!IsLoading && !IsSaving && CalculateEnabled)
                        UpdateCashFlowForecast();
                }
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
                if (IsDeleted)
                {
                    PrimaryCashFlow.UpdatePrimaryForexTradeAmounts();
                }
                if (SetPropertyValue("PrimaryCashFlow", ref _PrimaryCashFlow, value))
                {
                    if (!IsLoading && !IsSaving && !IsDeleted && CalculateEnabled)
                    {
                        UpdateCashFlowForecast();
                    }
                }
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
                if (IsDeleted)
                {
                    CounterCashFlow.UpdateCounterForexTradeAmounts();
                }
                if (SetPropertyValue("CounterCashFlow", ref _CounterCashFlow, value))
                {
                    if (!IsLoading && !IsSaving && !IsDeleted && CalculateEnabled)
                    {
                        UpdateCashFlowForecast();
                    }
                }
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

        [Association("FromForexTrade-ForexTradePredelivery")]
        public XPCollection<ForexTradePredelivery> FromForexTradePredelivery
        {
            get
            {
                return GetCollection<ForexTradePredelivery>("FromForexTradePredelivery");
            }
        }

        [Association("ToForexTrade-ForexTradePredelivery")]
        public XPCollection<ForexTradePredelivery> ToForexTradePredelivery
        {
            get
            {
                return GetCollection<ForexTradePredelivery>("ToForexTradePredelivery");
            }
        }

        [Association("AmendForexTrade-ForexTradePredelivery")]
        public XPCollection<ForexTradePredelivery> AmendForexTradePredelivery
        {
            get
            {
                return GetCollection<ForexTradePredelivery>("AmendForexTradePredelivery");
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

        #region Cash Flow Link Logic

        private void UpdateCashFlowForecast()
        {
            // if no date specified, then it's not worth query the database
            // for the Max Actual Date
            if (PrimarySettleDate == default(DateTime)
                || CounterSettleDate == default(DateTime))
                return;

            if (Counterparty == null
                || CounterCcy == null || PrimaryCcy == null
                || CounterSettleAccount == null || PrimarySettleAccount == null) return;

            // do not update if settle date is after forecast date
            // TODO: cache max Actual Date
            var maxActualDate = CashFlow.GetMaxActualTranDate(Session);

            if (PrimarySettleDate <= maxActualDate
                || CounterSettleDate <= maxActualDate
                || PrimarySettleAccount == null
                || CounterSettleAccount == null)
                return;

            UpdatePrimaryCashFlow();
            UpdateCounterCashFlow();
        }

        private void UpdatePrimaryCashFlow()
        {
            if (!IsForexTradeValid) return;

            _OldPrimaryCashFlow = PrimaryCashFlow;

            if (SettleGroupId == 0)
            {
                CreatePrimaryCashFlow();
            }
            else if (_PrimaryCashFlow == null)
            {
                FindOrCreatePrimaryCashFlow();
            }
            else if (_PrimaryCashFlow.TranDate != PrimarySettleDate
                    || _PrimaryCashFlow.Account != PrimarySettleAccount
                    || IsForexTradeCashFlowBaseDiff(_PrimaryCashFlow))
            {
                // use different cash flow if descriptive fields are different
                FindOrCreatePrimaryCashFlow();
            }
            _PrimaryCashFlow.UpdatePrimaryForexTradeAmounts();
            if (_OldPrimaryCashFlow != null)
                _OldPrimaryCashFlow.UpdatePrimaryForexTradeAmounts();
        }

        private void UpdateCounterCashFlow()
        {
            if (!IsForexTradeValid) return;

            _OldCounterCashFlow = CounterCashFlow;

            if (SettleGroupId == 0)
            {
                CreateCounterCashFlow();
            }
            else if (_CounterCashFlow == null)
            {
                FindOrCreateCounterCashFlow();
            }
            else if (_CounterCashFlow.TranDate != CounterSettleDate
                    || _CounterCashFlow.Account != CounterSettleAccount
                    || IsForexTradeCashFlowBaseDiff(_CounterCashFlow))
            {
                // use different cash flow if descriptive fields are different
                FindOrCreateCounterCashFlow();
            }
            _CounterCashFlow.UpdateCounterForexTradeAmounts();
            if (_OldCounterCashFlow != null)
                _OldCounterCashFlow.UpdateCounterForexTradeAmounts();
        }

        private bool IsForexTradeValid
        {
            get
            {
                if (PrimarySettleAccount == null || PrimarySettleDate == default(DateTime)
                    || CounterSettleAccount == null || CounterSettleDate == default(DateTime)
                  || CounterCcy == null || PrimaryCcy == null) return false;
                return true;
            }
        }

        private bool IsForexTradeCashFlowBaseDiff(CashFlow cashFlow)
        {
            if (cashFlow.CounterCcy != CounterCcy
                || cashFlow.Counterparty != Counterparty.CashFlowCounterparty
                || cashFlow.ForexSettleGroupId != SettleGroupId) return true;
            return false;
        }

        private void FindOrCreatePrimaryCashFlow()
        {
            SetPropertyValue("PrimaryCashFlow", ref _PrimaryCashFlow, FindPrimaryCashFlow());
            if (_PrimaryCashFlow == null)
                CreatePrimaryCashFlow();
        }
        private void FindOrCreateCounterCashFlow()
        {
            // use different cash flow if descriptive fields are different
            SetPropertyValue("CounterCashFlow", ref _CounterCashFlow, FindCounterCashFlow());
            // create cash flow if different cash flow does not exist
            if (_CounterCashFlow == null)
                CreateCounterCashFlow();
        }
        private void CreatePrimaryCashFlow()
        {
            // Create Cash Flow if one does not exist
            SetPropertyValue("PrimaryCashFlow", ref _PrimaryCashFlow, new CashFlow(Session));
            SetPrimaryCashFlowAttrs(_PrimaryCashFlow);
        }
        private void CreateCounterCashFlow()
        {
            SetPropertyValue("CounterCashFlow", ref _CounterCashFlow, new CashFlow(Session));
            SetCounterCashFlowAttrs(_CounterCashFlow);
        }

        // Find matching CashFlow
        public CashFlow FindPrimaryCashFlow()
        {
            var forexActivity = Session.GetObjectByKey<Activity>(SetOfBooks.CachedInstance.ForexSettleActivity.Oid);
            var source = Session.GetObjectByKey<CashFlowSource>(SetOfBooks.CachedInstance.ForexSettleCashFlowSource.Oid);

            var criteria = CriteriaOperator.Parse(
                       "Account.Currency = ? And TranDate = ? And Account = ? And Activity = ? And Source = ?" 
                       + " And ForexSettleGroupId = ? And Counterparty = ?"
                       + " And Snapshot = ?",
                       PrimaryCcy, PrimarySettleDate, PrimarySettleAccount, forexActivity, source, 
                       SettleGroupId, Counterparty.CashFlowCounterparty,
                       SetOfBooks.CachedInstance.CurrentCashFlowSnapshot);
            return Session.FindObject<CashFlow>(PersistentCriteriaEvaluationBehavior.InTransaction, criteria);
        }
        public CashFlow FindCounterCashFlow()
        {

            var forexActivity = Session.GetObjectByKey<Activity>(SetOfBooks.CachedInstance.ForexSettleActivity.Oid);
            var source = Session.GetObjectByKey<CashFlowSource>(SetOfBooks.CachedInstance.ForexSettleCashFlowSource.Oid);

            var criteria = CriteriaOperator.Parse(
                       "Account.Currency = ? And TranDate = ? And Account = ? And Activity = ? And Source = ?"
                        + " And ForexSettleGroupId = ? And Counterparty = ?"
                        + " And Snapshot = ?",
                       CounterCcy, CounterSettleDate, CounterSettleAccount, forexActivity, source, 
                       SettleGroupId, Counterparty.CashFlowCounterparty,
                       SetOfBooks.CachedInstance.CurrentCashFlowSnapshot);
            return Session.FindObject<CashFlow>(PersistentCriteriaEvaluationBehavior.InTransaction, criteria);
        }

        public void SetPrimaryCashFlowAttrs(CashFlow cashFlow)
        {
            cashFlow.Description = string.Format("{0}-{1} {2}-Leg", PrimaryCcy.Name, CounterCcy.Name, PrimaryCcy.Name);
            cashFlow.CounterCcy = CounterCcy;
            cashFlow.TranDate = PrimarySettleDate;
            cashFlow.Counterparty = Counterparty.CashFlowCounterparty;
            cashFlow.Status = CashFlowStatus.Forecast;
            cashFlow.ForexSettleType = CashFlowForexSettleType.In;
            cashFlow.Account = PrimarySettleAccount; // TODO: ensure this does not change the currency
            cashFlow.Activity = cashFlow.Session.GetObjectByKey<Activity>(SetOfBooks.CachedInstance.ForexSettleActivity.Oid);
            cashFlow.Source = cashFlow.Session.GetObjectByKey<CashFlowSource>(SetOfBooks.CachedInstance.ForexSettleCashFlowSource.Oid);
            cashFlow.ForexSettleGroupId = SettleGroupId;
        }
        public void SetCounterCashFlowAttrs(CashFlow cashFlow)
        {
            cashFlow.Description = string.Format("{0}-{1} {2}-Leg", PrimaryCcy.Name, CounterCcy.Name, CounterCcy.Name);
            cashFlow.CounterCcy = CounterCcy;
            cashFlow.TranDate = CounterSettleDate;
            cashFlow.Counterparty = Counterparty.CashFlowCounterparty;
            cashFlow.Status = CashFlowStatus.Forecast;
            cashFlow.ForexSettleType = CashFlowForexSettleType.In;
            cashFlow.Account = CounterSettleAccount;
            cashFlow.Activity = cashFlow.Session.GetObjectByKey<Activity>(SetOfBooks.CachedInstance.ForexSettleActivity.Oid);
            cashFlow.Source = cashFlow.Session.GetObjectByKey<CashFlowSource>(SetOfBooks.CachedInstance.ForexSettleCashFlowSource.Oid);
            cashFlow.ForexSettleGroupId = SettleGroupId;
        }

        public void UpdatePrimaryCashFlowAccount()
        {
            if (PrimaryCashFlow == null || PrimaryCashFlow == null) return;
            PrimaryCashFlow.Account = PrimarySettleAccount;
        }
        public void UpdateCounterCashFlowAccount()
        {
            if (CounterSettleAccount == null || CounterCashFlow == null) return;
            CounterCashFlow.Account = CounterSettleAccount;
        }

        // TODO: use same pattern as Fix Forecast? Or specify date?
        public static void BatchUpdateCashFlow(XPObjectSpace objSpace)
        {
            throw new NotImplementedException();
        }

        private long SequentialNumberToSettleGroupId
        {
            get
            {
                // since the first SequentialNumber is zero, 
                // but zero is already used to determine whether to have a
                // unique SettleGroupId
                return SequentialNumber + 1;
            }
        }
        #endregion

        #region Forex Rate Logic
        public void UpdateRate()
        {
            var obj = this;
            var fromAmt = CounterCcyAmt;
            var session = Session;
            var fromCcy = CounterCcy;

            if (obj.Rate == 0.00M && obj.PrimaryCcyAmt != 0.00M && obj.CounterCcyAmt != 0.00M)
            {
                obj.SetPropertyValue("Rate", ref obj._Rate, Math.Round(obj.CounterCcyAmt / obj.PrimaryCcyAmt, 5));
            }
            else if (obj.Rate == 0.00M && obj.PrimaryCcy != null & obj.CounterCcy != null)
            {
                obj.SetPropertyValue("Rate", ref obj._Rate, 5);
            }

            else if (obj.ValueDate != default(DateTime))
            {
                var rateObj = GetForexRateObject(session, fromCcy, SetOfBooks.CachedInstance.FunctionalCurrency, obj.ValueDate);
                if (rateObj != null)
                {
                    var value = Math.Round(fromAmt * (decimal)rateObj.ConversionRate, 2);
                    obj.SetPropertyValue("PrimaryCcyAmt", ref obj._PrimaryCcyAmt, value);
                }
            }
        }

        private void CalculatePrimaryCcyAmt(ForexTrade obj, Currency fromCcy, decimal fromAmt)
        {
            var session = obj.Session;
            var rateObj = GetForexRateObject(session, fromCcy, SetOfBooks.CachedInstance.FunctionalCurrency, obj.ValueDate);
            if (rateObj != null)
            {
                var usedRate = 1 / rateObj.ConversionRate;
                obj.SetPropertyValue("Rate", ref obj._Rate, usedRate);
                obj.PrimaryCcyAmt = Math.Round(fromAmt / usedRate, 2);
            }
        }

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
        #endregion

        #region Trade Predelivery Logic
        public ForexTradePredelivery Predeliver(decimal counterCcyAmt, DateTime valueDate, decimal rate)
        {
            var pdy = new ForexTradePredelivery(Session);
            
            pdy.FromForexTrade = this;
            pdy.CounterCcyAmt = counterCcyAmt;
            pdy.ValueDate = valueDate;
            pdy.Rate = rate;

            return pdy;
        }
        #endregion

        public void InitDefaultValues()
        {
            OrigTrade = this;
            CreationDate = DateTime.Now;
            TradeDate = DateTime.Now;
            OrigTradeDate = TradeDate;
            ValueDate = DateTime.Now.Date;
            PrimarySettleDate = ValueDate;
            CounterSettleDate = ValueDate;

            // Initialise Primary Currency and Settlement Account
            PrimaryCcy = Session.GetObjectByKey<Currency>(
                SetOfBooks.CachedInstance.FunctionalCurrency.Oid);
            // TODO: default settle account if Counterparty is UNDEFINED
            //PrimarySettleAccount = GetDefaultSettleAccount(PrimaryCcy, Counterparty);
        }

        public static void UploadToCashFlowForecast(Session session)
        {
            var maxActualDate = CashFlow.GetMaxActualTranDate(session);
            DeleteCashFlowForecasts(session);
            session.CommitTransaction();

            CriteriaOperator criteria;
            if (maxActualDate != default(DateTime))
                criteria = CriteriaOperator.Parse("PrimarySettleDate > ? Or CounterSettleDate > ?", 
                    maxActualDate, maxActualDate);
            else
                criteria = null;
            var fts = session.GetObjects(session.GetClassInfo(typeof(ForexTrade)),
                            criteria, new SortingCollection(null), 0, false, true);
            foreach (ForexTrade ft in fts)
            {
                ft.UpdateCashFlowForecast();
            }
            session.CommitTransaction();
        }

        private static void DeleteCashFlowForecasts(Session session)
        {
            var source = session.GetObjectByKey<CashFlowSource>(SetOfBooks.CachedInstance.ForexSettleCashFlowSource.Oid);
            var criteria = CriteriaOperator.Parse("Source = ? And Status = ?",
                source, CashFlowStatus.Forecast);
            var cashFlows = session.GetObjects(session.GetClassInfo(typeof(CashFlow)),
                criteria, new SortingCollection(null), 0, false, true);
            session.Delete(cashFlows);
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            if (SettleGroupId == 0)
            {
                SettleGroupId = SequentialNumberToSettleGroupId;
                if (_PrimaryCashFlow != null && _CounterCashFlow != null)
                {
                    _PrimaryCashFlow.ForexSettleGroupId = SettleGroupId;
                    _CounterCashFlow.ForexSettleGroupId = SettleGroupId;
                }
            }
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
        Reclass
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
