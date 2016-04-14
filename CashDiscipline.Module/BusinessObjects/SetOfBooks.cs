using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;

// With XPO, the data model is declared by classes (so-called Persistent Objects) that will define the database structure, and consequently, the user interface (http://documentation.devexpress.com/#Xaf/CustomDocument2600).
namespace CashDiscipline.Module.BusinessObjects
{
    // Specify various UI options for your persistent class and its properties using a declarative approach via built-in attributes (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
    //[ImageName("BO_Contact")]
    //[DefaultProperty("PersistentProperty")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewAndDetailView, true, NewItemRowPosition.Top)]
    [DefaultClassOptions]
    public class SetOfBooks : BaseObject
    { // You can use a different base persistent class based on your requirements (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public SetOfBooks(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here or place it only when the IsLoading property is false.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code (check out http://documentation.devexpress.com/#Xaf/CustomDocument2834 for more details).
            FunctionalCurrency = Session.FindObject<Currency>(CriteriaOperator.Parse("[Name] = 'AUD'"));
        }

        // Fields...
        private static SetOfBooks _CachedInstance;
        public static SetOfBooks CachedInstance
        {
            get
            {
                return _CachedInstance;
            }
            set
            {
                _CachedInstance = value;
            }
        }

        private string _Description;
        private Currency _FunctionalCurrency;
        private Activity _ForexSettleActivity;
        private CashFlowSource _ForexSettleCashFlowSource;
        private CashFlowSource _BankStmtCashFlowSource;
        private CashFlowSnapshot _CurrentCashFlowSnapshot;
        private string _CashFlowPivotLayoutName;

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


        public Currency FunctionalCurrency
        {
            get
            {
                return _FunctionalCurrency;
            }
            set
            {
                SetPropertyValue("FunctionalCurrency", ref _FunctionalCurrency, value);
            }
        }

        public Activity ForexSettleActivity
        {
            get
            {
                return _ForexSettleActivity;
            }
            set
            {
                SetPropertyValue("ForexSettleActivity", ref _ForexSettleActivity, value);
            }
        }

        public CashFlowSource ForexSettleCashFlowSource
        {
            get
            {
                return _ForexSettleCashFlowSource;
            }
            set
            {
                SetPropertyValue("ForexSettleCashFlowSource", ref _ForexSettleCashFlowSource, value);
            }
        }

        public CashFlowSource BankStmtCashFlowSource
        {
            get
            {
                return _BankStmtCashFlowSource;
            }
            set
            {
                SetPropertyValue("BankStmtCashFlowSource", ref _BankStmtCashFlowSource, value);
            }
        }

        public CashFlowSnapshot CurrentCashFlowSnapshot
        {
            get
            {
                return _CurrentCashFlowSnapshot;
            }
            set
            {
                SetPropertyValue("CurrentCashFlowSnapshot", ref _CurrentCashFlowSnapshot, value);
            }
        }

        public string CashFlowPivotLayoutName
        {
            get
            {
                return _CashFlowPivotLayoutName;
            }
            set
            {
                SetPropertyValue("CashFlowPivotLayoutName", ref _CashFlowPivotLayoutName, value);
            }
        }
        #region CurrentSetOfBooks

        public static SetOfBooks GetInstance(DevExpress.Xpo.Session session)
        {
            // return previous instance if session matches
            try
            {
                if (CachedInstance != null
                    && CachedInstance.Session == session)
                {
                    return CachedInstance;
                }
            }
            catch (ObjectDisposedException)
            {
            }

            // return instance from new session
            var setOfBooks = session.FindObject<SetOfBooks>(PersistentCriteriaEvaluationBehavior.InTransaction, null);
            if (setOfBooks == null)
            {
                // create SetOfBooks if it doesn't exist
                setOfBooks = new SetOfBooks(session);
                setOfBooks.Description = "Default";
                setOfBooks.Save();
            }
            SetOfBooks.CachedInstance = setOfBooks;
            return setOfBooks;
        }

        public static SetOfBooks GetInstance(DevExpress.ExpressApp.IObjectSpace objectSpace)
        {
            var setOfBooks = GetInstance(((XPObjectSpace)objectSpace).Session);
            return setOfBooks;
        }
        public static SetOfBooks GetInstance(DevExpress.ExpressApp.XafApplication application)
        {
            var objectSpace = application.CreateObjectSpace();
            return GetInstance(((XPObjectSpace)objectSpace).Session);
        }
        #endregion

        protected override void OnSaving()
        {
            var objs = new XPCollection<SetOfBooks>(PersistentCriteriaEvaluationBehavior.InTransaction, Session, null);
            if (objs.Count > 1)
                throw new InvalidOperationException("You cannot create more than one Set Of Books");
            base.OnSaving();
        }
    }
}
