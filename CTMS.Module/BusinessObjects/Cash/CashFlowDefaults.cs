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
using CTMS.Module.BusinessObjects.ChartOfAccounts;
using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp.Xpo;

namespace CTMS.Module.BusinessObjects.Cash
{
    public class CashFlowDefaults : BaseObject
    {
        public CashFlowDefaults(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }
        private Account _Account;
        private Activity _Activity;
        private Counterparty _Counterparty;

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
            CashFlowDefaults result = objectSpace.FindObject<CashFlowDefaults>(null);
            if (result == null)
            {
                result = new CashFlowDefaults(((XPObjectSpace)objectSpace).Session);
                result.Save();
            }
            return result;
        }
        protected override void OnDeleting()
        {
            throw new UserFriendlyException("This object cannot be deleted.");
        }
    }
}
