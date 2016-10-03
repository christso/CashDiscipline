using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;

// With XPO, the data model is declared by classes (so-called Persistent Objects) that will define the database structure, and consequently, the user interface (http://documentation.devexpress.com/#Xaf/CustomDocument2600).
namespace CashDiscipline.Module.BusinessObjects
{
    [ModelDefault("ImageName", "BO_List")]
    [DefaultClassOptions]
    public class SetOfBooks : BaseObject
    {
        public SetOfBooks(Session session)
            : base(session)
        {

        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();

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
        private Activity _UnrealFxActivity;
        private CashFlowSource _ForexSettleCashFlowSource;
        private CashFlowSource _FcaRevalCashFlowSource;
        private CashFlowSource _BankStmtCashFlowSource;
        private CashFlowSnapshot _CurrentCashFlowSnapshot;

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

        public Activity UnrealFxActivity
        {
            get
            {
                return _UnrealFxActivity;
            }
            set
            {
                SetPropertyValue("UnrealFxActivity", ref _UnrealFxActivity, value);
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

        public CashFlowSource FcaRevalCashFlowSource
        {
            get
            {
                return _FcaRevalCashFlowSource;
            }
            set
            {
                SetPropertyValue("FcaRevalCashFlowSource", ref _FcaRevalCashFlowSource, value);
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

        private CashSnapshotReported _CashSnapshotReported;
        public CashSnapshotReported CashSnapshotReported
        {
            get
            {
                return _CashSnapshotReported;
            }
            set
            {
                SetPropertyValue("CashSnapshotReported", ref _CashSnapshotReported, value);
            }
        }

        private Activity _AustPostSettleActivity;
        public Activity AustPostSettleActivity
        {
            get
            {
                return _AustPostSettleActivity;
            }
            set
            {
                SetPropertyValue("AustPostSettleActivity", ref _AustPostSettleActivity, value);
            }
        }


        #region CurrentSetOfBooks

        public static SetOfBooks GetInstance(DevExpress.Xpo.Session session)
        {
            CachedInstance = (SetOfBooks)BaseObjectHelper.PullCachedInstance(
                session, CachedInstance, typeof(SetOfBooks));
            return CachedInstance;
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
