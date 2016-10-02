using System;
using System.ComponentModel;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp.Model;
using CashDiscipline.Module.BusinessObjects.FinAccounting;
using CashDiscipline.Module.BusinessObjects.Forex;
using DevExpress.ExpressApp.Xpo;
using CashDiscipline.Module.ParamObjects.Cash;
using System.Linq;
using System.Collections.Generic;

using System.Diagnostics;
using Xafology.ExpressApp.Xpo.Import;
using CashDiscipline.Module.BusinessObjects.BankStatement;
using CashDiscipline.Module.Attributes;

namespace CashDiscipline.Module.BusinessObjects.Cash
{
    [VisibleInReports(true)]
    [ModelDefault("DefaultListViewAllowEdit", "True")]
    [DefaultProperty("Oid")]
    [ModelDefault("IsFooterVisible", "True")]
    [AutoColumnWidth(false)]
    [ModelDefault("ImageName", "BO_List")]
    public class BankStmt : BaseObject, IXpoImportable
    {
        public BankStmt(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here or place it only when the IsLoading property is false:
            // if (!IsLoading){
            //    It is now OK to place your initialization code here.
            // }
            // or as an alternative, move your initialization code into the AfterConstruction method.

        }
        private bool _CalculateEnabled;

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
            _CalculateEnabled = true;
            TranDate = DateTime.Now;
            if (CashDiscipline.Common.AppBehaviour.UserTriggersEnabled)
                InitDefaultValues();
        }

        public void InitDefaultValues()
        {
            TranDate = DateTime.Now;

            var setOfBooks = SetOfBooks.GetInstance(Session);
            if (setOfBooks != null)
                CounterCcy = setOfBooks.FunctionalCurrency;

            var bsDefs = BankStmtDefaults.GetInstance(Session);
            if (bsDefs != null)
            {
                TranCode = bsDefs.TranCode;
            }

            var cfDefs = CashFlowDefaults.GetInstance(Session);
            if (cfDefs != null)
            {
                Counterparty = cfDefs.Counterparty;
                Account = cfDefs.Account;
                Activity = cfDefs.Activity;
            }
        }

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

        private string _OraTrxNum;
        [VisibleInLookupListView(true)]
        [ModelDefault("AllowEdit", "true")]
        public string OraTrxNum
        {
            get
            {
                return _OraTrxNum;
            }
            set
            {
                SetPropertyValue("OraTrxNum", ref _OraTrxNum, value);
            }
        }

        DateTime _TranDate;
        [VisibleInLookupListView(true)]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime TranDate
        {
            get { return _TranDate; }
            set { SetPropertyValue("TranDate", ref _TranDate, value); }
        }
        string _TranType;
        [VisibleInLookupListView(false)]
        [Size(200)]
        public string TranType
        {
            get { return _TranType; }
            set { SetPropertyValue("TranType", ref _TranType, value); }
        }

        string _TranRef;
        [VisibleInLookupListView(false)]
        [Size(200)]
        public string TranRef
        {
            get { return _TranRef; }
            set { SetPropertyValue("TranRef", ref _TranRef, value); }
        }
        decimal _TranAmount;
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal TranAmount
        {
            get { return _TranAmount; }
            set
            {
                if (SetPropertyValue("TranAmount", ref _TranAmount, Math.Round(value, 2)))
                {
                    if (!IsLoading && !IsSaving && Account != null
                        && TranDate != default(DateTime) && CalculateEnabled)
                    {
                        // TODO: update regardless of whether it's zero otherwise you only update the 1st value
                        UpdateFunctionalCcyAmt(this, value, Account.Currency);
                        UpdateCounterCcyAmt(this, value, Account.Currency);
                    }
                }
            }
        }
        string _TranDescription;
        [VisibleInLookupListView(true)]
        [VisibleInListView(true)]
        [Size(SizeAttribute.Unlimited)]
        public string TranDescription
        {
            get { return _TranDescription; }
            set { SetPropertyValue("TranDescription", ref _TranDescription, value); }
        }

        BankStmtTranCode _TranCode;
        [VisibleInLookupListView(false)]
        public BankStmtTranCode TranCode
        {
            get { return _TranCode; }
            set
            {
                SetPropertyValue("TranCode", ref _TranCode, value);
            }
        }

        Account _Account;
        [VisibleInLookupListView(true)]
        public Account Account
        {
            get { return _Account; }
            set
            {
                if (SetPropertyValue("Account", ref _Account, value))
                {
                    if (!IsLoading && !IsSaving && value != null)
                    {
                        SetPropertyValue("CounterCcy", ref _CounterCcy, value.Currency);
                    }
                }
            }
        }
        Activity _Activity;
        [VisibleInLookupListView(false)]
        public Activity Activity
        {
            get { return _Activity; }
            set { SetPropertyValue("Activity", ref _Activity, value); }
        }
        Counterparty _Counterparty;
        [VisibleInLookupListView(false)]
        public Counterparty Counterparty
        {
            get { return _Counterparty; }
            set
            {
                SetPropertyValue("Counterparty", ref _Counterparty, value);
            }
        }
        string _SummaryDescription;
        [VisibleInLookupListView(false)]
        [Size(SizeAttribute.Unlimited)]
        public string SummaryDescription
        {
            get { return _SummaryDescription; }
            set { SetPropertyValue("SummaryDescription", ref _SummaryDescription, value); }
        }
        decimal _FunctionalCcyAmt;
        [VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal FunctionalCcyAmt
        {
            get { return _FunctionalCcyAmt; }
            set { SetPropertyValue("FunctionalCcyAmt", ref _FunctionalCcyAmt, Math.Round(value, 2)); }
        }
        decimal _CounterCcyAmt;
        [VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        public decimal CounterCcyAmt
        {
            get { return _CounterCcyAmt; }
            set { SetPropertyValue("CounterCcyAmt", ref _CounterCcyAmt, Math.Round(value, 2)); }
        }
        Currency _CounterCcy;
        [VisibleInLookupListView(false)]
        [Association(@"CounterCcy-BankStmt")]
        public Currency CounterCcy
        {
            get { return _CounterCcy; }
            set { SetPropertyValue("CounterCcy", ref _CounterCcy, value); }
        }

        DateTime _ValueDate;
        [VisibleInLookupListView(true)]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime ValueDate
        {
            get { return _ValueDate; }
            set { SetPropertyValue("ValueDate", ref _ValueDate, value); }
        }

        private DateTime _TimeEntered;
        [ModelDefault("DisplayFormat", "dd-MMM-yy hh:mm:ss")]
        public DateTime TimeEntered
        {
            get
            {
                return _TimeEntered;
            }
            set
            {
                SetPropertyValue("TimeEntered", ref _TimeEntered, value);
            }
        }

        private ActionOwner _ActionOwner;
        public ActionOwner ActionOwner
        {
            get { return _ActionOwner; }
            set { SetPropertyValue("ActionOwner", ref _ActionOwner, value); }
        }

        private CashFlow _CashFlow;
        [Association("CashFlow-BankStmts")]
        public CashFlow CashFlow
        {
            get
            {
                return _CashFlow;
            }
            set
            {
                SetPropertyValue("CashFlow", ref _CashFlow, value);
            }
        }

        private CashFlowForexSettleType _ForexSettleType;
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

        [Association("BankStmt-AustPostSettle")]
        public XPCollection<AustPostSettle> AustPostSettles
        {
            get
            {
                return GetCollection<AustPostSettle>("AustPostSettles");
            }
        }

        protected override void OnSaving()
        {
            base.OnSaving();

            CalculateAmounts();
        }

        public void CalculateAmounts()
        {
            if (IsDeleted) return;
            if (TranDate == null) return;

            if (FunctionalCcyAmt == 0.00M && TranAmount != 0.00M && Account != null)
                UpdateFunctionalCcyAmt(this, TranAmount, Account.Currency);
            if (FunctionalCcyAmt == 0.00M && CounterCcyAmt != 0.00M && CounterCcy != null)
                UpdateFunctionalCcyAmt(this, CounterCcyAmt, CounterCcy);
            if (CounterCcyAmt == 0.00M && TranAmount != 0.00M && Account != null)
                UpdateCounterCcyAmt(this, TranAmount, Account.Currency);
            if (TranAmount == 0.00M && CounterCcyAmt != 0.00M)
                UpdateAccountCcyAmt(this, CounterCcyAmt, CounterCcy);
        }

        #region Amount Calculators
        private static ForexRate GetForexRateObject(Session session, Currency fromCcy, Currency toCcy, DateTime convDate)
        {
            return ForexRate.GetForexRateObject(fromCcy, toCcy, convDate);
        }

        public static void UpdateFunctionalCcyAmt(BankStmt obj, decimal fromAmt, Currency fromCcy)
        {
            var session = obj.Session;
            if (fromCcy == null) return;
            if (SetOfBooks.CachedInstance.FunctionalCurrency.Oid == fromCcy.Oid)
                obj.FunctionalCcyAmt = fromAmt;
            else if (obj.TranDate != default(DateTime))
            {
                var rateObj = GetForexRateObject(session, fromCcy, SetOfBooks.CachedInstance.FunctionalCurrency, (DateTime)obj.TranDate);
                if (rateObj != null)
                {
                    // TODO: do not assume that Functional Currency is 'AUD'
                    var value = fromAmt * (decimal)rateObj.ConversionRate;
                    obj.SetPropertyValue("FunctionalCcyAmt", ref obj._FunctionalCcyAmt, Math.Round(value, 2));
                }
            }
        }
        public static void UpdateAccountCcyAmt(BankStmt obj, decimal fromAmt, Currency fromCcy)
        {
            if (obj.Account == null) return;
            var session = obj.Session;
            if (obj.Account.Currency == null)
                throw new UserFriendlyException("No currency for account " + obj.Account.Name);
            if (obj.Account.Currency.Oid == fromCcy.Oid)
                obj.TranAmount = fromAmt;
            else if (obj.TranDate != default(DateTime))
            {
                var rateObj = GetForexRateObject(session, fromCcy, obj.Account.Currency, (DateTime)obj.TranDate);
                if (rateObj != null)
                {
                    var value = fromAmt * (decimal)rateObj.ConversionRate;
                    obj.SetPropertyValue("TranAmount", ref obj._TranAmount, Math.Round(value, 2));
                }
            }
        }
        public static void UpdateCounterCcyAmt(BankStmt obj, decimal fromAmt, Currency fromCcy)
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

        public static void DeleteCashFlows(IObjectSpace objSpace, string criteria, params object[] parameters)
        {
            var cop = CriteriaOperator.Parse(criteria, parameters);
            var cashFlows = objSpace.GetObjects<CashFlow>(cop);
            objSpace.Delete(cashFlows);
            objSpace.CommitChanges();
        }
    }
}
