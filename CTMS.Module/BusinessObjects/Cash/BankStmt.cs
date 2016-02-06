using System;
using System.ComponentModel;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp.Model;
using CTMS.Module.BusinessObjects.FinAccounting;
using CTMS.Module.BusinessObjects.Forex;
using DevExpress.ExpressApp.Xpo;
using CTMS.Module.ParamObjects.Cash;
using System.Linq;
using System.Collections.Generic;

using System.Diagnostics;

namespace CTMS.Module.BusinessObjects.Cash
{
    [VisibleInReports(true)]
    [ModelDefault("DefaultListViewAllowEdit", "True")]
    [DefaultProperty("BankStmtId")]
    public class BankStmt : BaseObject
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
            if (AppSettings.UserTriggersEnabled)
                InitDefaultValues();
        }

        public void InitDefaultValues()
        {
            TranDate = DateTime.Now;

            CounterCcy = Session.GetObjectByKey<Currency>(SetOfBooks.CachedInstance.FunctionalCurrency.Oid);

            var defObj = Session.FindObject<CashFlowDefaults>(null);
            if (defObj == null) return;

            Counterparty = defObj.Counterparty;
            Account = defObj.Account;
            Activity = defObj.Activity;
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
        [ModelDefault("AllowEdit", "false")]
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

        [Association("BankStmt-GenLedgers")]
        public XPCollection<GenLedger> GenLedgers
        {
            get
            {
                return GetCollection<GenLedger>("GenLedgers");
            }
        }

        [PersistentAlias("concat('BS', ToStr(Id))")]
        public string BankStmtId
        {
            get
            {
                return Convert.ToString(EvaluateAlias("BankStmtId"));
            }
        }

        int _Id;
        [ModelDefault("DisplayFormat", "f0")]
        [ModelDefault("AllowEdit", "false")]
        public int Id
        {
            get { return _Id; }
            set { SetPropertyValue("Id", ref _Id, value); }
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
                if (SetPropertyValue("TranAmount", ref _TranAmount, Math.Round(value,2)))
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
        [Size(SizeAttribute.Unlimited)]
        public string TranDescription
        {
            get { return _TranDescription; }
            set { SetPropertyValue("TranDescription", ref _TranDescription, value); }
        }
        Account _Account;
        [VisibleInLookupListView(true)]
        [Association(@"Account-BankStmt")]
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
        [Association(@"Activity-BankStmt")]
        public Activity Activity
        {
            get { return _Activity; }
            set { SetPropertyValue("Activity", ref _Activity, value); }
        }
        Counterparty _Counterparty;
        [VisibleInLookupListView(false)]
        [Association(@"Counterparty-BankStmt")]
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

        [Association("BankStmt-AustPostSettle")]
        public XPCollection<AustPostSettle> AustPostSettles
        {
            get
            {
                return GetCollection<AustPostSettle>("AustPostSettles");
            }
        }

        //[Association("BankStmt-ForexTrades")]
        //public XPCollection<ForexTrade> ForexTrade
        //{
        //    get
        //    {
        //        return GetCollection<ForexTrade>("ForexTrade");
        //    }
        //}

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
            return ForexRate.GetForexRateObject(session, fromCcy, toCcy, convDate);
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
                    obj.SetPropertyValue("FunctionalCcyAmt", ref obj._FunctionalCcyAmt, Math.Round(value,2));
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
                    obj.SetPropertyValue("TranAmount", ref obj._TranAmount, Math.Round(value,2));
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

        #region Master Detail
        protected override void OnLoaded()
        {
            //When using "lazy" calculations it's necessary to reset cached values.
            Reset();
            base.OnLoaded();
        }
        private void Reset()
        {
            _GenLedgerTotal = null;
            GenLedgers.Reload();
        }
        #endregion

        #region GenLedger Master

        [Persistent("GenLedgerTotal")]
        private decimal? _GenLedgerTotal = null;

        [VisibleInLookupListView(false)]
        [PersistentAlias("_GenLedgerTotal")]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("AllowEdit", "false")]
        public decimal? GenLedgerFuncTotal
        {
            get
            {
                if (!IsLoading && !IsSaving && _GenLedgerTotal == null)
                    UpdateGenLedgerTotal(false);
                return _GenLedgerTotal;
            }
        }

        //Define a way to calculate and update the OrdersTotal;
        public void UpdateGenLedgerTotal(bool forceChangeEvents)
        {
            //Put your complex business logic here. Just for demo purposes, we calculate a sum here.
            decimal? oldGenLedgerTotal = _GenLedgerTotal;
            decimal tempTotal = 0;
            //Manually iterate through the Orders collection if your calculated property requires a complex business logic which cannot be expressed via criteria language.
            foreach (GenLedger detail in GenLedgers)
            {
                if (!detail.IsActivity)
                    tempTotal += detail.FunctionalCcyAmt;
            }
            _GenLedgerTotal = tempTotal;
            if (forceChangeEvents)
                OnChanged("GenLedgerTotal", oldGenLedgerTotal, _GenLedgerTotal);
        }
        #endregion

        public static void DeleteCashFlows(IObjectSpace objSpace, string criteria, params object[] parameters)
        {
            var cop = CriteriaOperator.Parse(criteria, parameters);
            var cashFlows = objSpace.GetObjects<CashFlow>(cop);
            objSpace.Delete(cashFlows);
            objSpace.CommitChanges();
        }

        public static void UploadToCashFlow(IObjectSpace objSpace, DateTime dateParam)
        {
            UploadToCashFlow(objSpace, dateParam, dateParam);
        }

        // TODO: avoid using SQL
        public static void UploadToCashFlow(IObjectSpace objSpace, DateTime fromDate, DateTime toDate)
        {
            var session = ((XPObjectSpace)objSpace).Session;

            XPQuery<BankStmt> bankStmtsQuery = new XPQuery<BankStmt>(session);
            var bankStmtsSum = from c in bankStmtsQuery
                                where c.TranDate >= fromDate && c.TranDate <= toDate
                                group c by new
                                {
                                    c.TranDate,
                                    c.Account,
                                    c.Activity,
                                    c.Counterparty,
                                    c.SummaryDescription,
                                    c.CounterCcy,
                                    c.ForexSettleType
                                } into grp
                                where Math.Round(grp.Sum(c => c.TranAmount), 2) != 0.00M
                                select new
                                {
                                    TranDate = grp.Key.TranDate,
                                    Account = grp.Key.Account,
                                    Activity = grp.Key.Activity,
                                    Counterparty = grp.Key.Counterparty,
                                    SummaryDescription = grp.Key.SummaryDescription,
                                    CounterCcy = grp.Key.CounterCcy,
                                    AccountCcyAmt = grp.Sum(c => c.TranAmount),
                                    FunctionalCcyAmt = grp.Sum(c => c.FunctionalCcyAmt),
                                    CounterCcyAmt = grp.Sum(c => c.CounterCcyAmt),
                                    ForexSettypeType = grp.Key.ForexSettleType
                                };

            var aCashFlows = new List<CashFlow>();

            foreach (var bs in bankStmtsSum)
            {
                var cf = objSpace.CreateObject<CashFlow>();
                var oldCalculateEnabled = cf.CalculateEnabled;
                cf.CalculateEnabled = false;
                cf.TranDate = bs.TranDate;
                cf.Account = bs.Account;
                cf.Activity = bs.Activity;
                cf.Counterparty = bs.Counterparty;
                cf.CounterCcy = bs.CounterCcy;
                cf.CounterCcyAmt = bs.CounterCcyAmt;
                cf.AccountCcyAmt = bs.AccountCcyAmt;
                cf.FunctionalCcyAmt = bs.FunctionalCcyAmt;
                cf.Description = bs.SummaryDescription;
                cf.Status = CashFlowStatus.Actual;
                cf.ForexSettleType = bs.ForexSettypeType;
                cf.CalculateEnabled = oldCalculateEnabled;
                aCashFlows.Add(cf);
            }
            objSpace.CommitChanges();

            session.ExplicitBeginTransaction();
            foreach (var cf in aCashFlows)
            {
                // record the CashFlow that was aggregated from BankStmt
                string bankStmtLinkSql = "UPDATE BankStmt SET CashFlow = @CashFlow WHERE "
                    + "TranDate = @TranDate"
                    + " AND Account = @Account"
                    + " AND Activity = @Activity"
                    + " AND Counterparty = @Counterparty"
                    + " AND CounterCcy = @CounterCcy"
                    + " AND ForexSettleType = @ForexSettleType";
                if (!string.IsNullOrEmpty(cf.Description))
                    bankStmtLinkSql += " AND SummaryDescription = @Description";

                session.ExecuteNonQuery(bankStmtLinkSql,
                    new string[] { "CashFlow", "TranDate", "Account", "Activity", "Counterparty", 
                        "Description", "CounterCcy", "ForexSettleType" },
                        new object[] { cf.Oid, cf.TranDate, 
                            cf.Account == null ? (Guid?)null : cf.Account.Oid, 
                            cf.Activity == null ? (Guid?)null : cf.Activity.Oid, 
                            cf.Counterparty == null ? (Guid?)null : cf.Counterparty.Oid,
                            cf.Description, 
                            cf.CounterCcy == null ? (Guid?)null : cf.CounterCcy.Oid, 
                            cf.ForexSettleType });
                // DEBUG
                //if (cf.ForexSettleType == CashFlowForexSettleType.In && cf.AccountCcyAmt == 100)
                //{
                //    Debug.Print(bankStmtLinkSql);
                //}
            }
            session.ExplicitCommitTransaction();
            
            // reload the collection as Direct SQL does not automatically update the collection
            foreach (var cf in aCashFlows)
                cf.BankStmts.Reload();
        }
    }

}
