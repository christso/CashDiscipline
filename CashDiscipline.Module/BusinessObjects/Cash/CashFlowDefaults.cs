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

using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp.Xpo;

namespace CashDiscipline.Module.BusinessObjects.Cash
{
    [ModelDefault("ImageName", "BO_List")]
    public class CashFlowDefaults : BaseObject
    {
        public CashFlowDefaults(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        private static CashFlowDefaults _CachedInstance;
        public static CashFlowDefaults CachedInstance
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

        private Account _Account;
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

        private Activity _Activity;

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

        private Counterparty _Counterparty;
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
        public static CashFlowDefaults GetInstance(IObjectSpace objectSpace)
        {
            return CashFlowDefaults.GetInstance(((XPObjectSpace)objectSpace).Session);
        }

        public static CashFlowDefaults GetInstance(Session session)
        {
            CachedInstance = (CashFlowDefaults)BaseObjectHelper.PullCachedInstance(
                session, CachedInstance, typeof(CashFlowDefaults));
            return CachedInstance;
        }

        protected override void OnSaving()
        {
            var objs = new XPCollection<SetOfBooks>(PersistentCriteriaEvaluationBehavior.InTransaction, Session, null);
            if (objs.Count > 1)
                throw new InvalidOperationException("You cannot create more than one CashFlowDefaults instance");
            base.OnSaving();
        }
    }
}
