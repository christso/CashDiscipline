using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CashDiscipline.Module.BusinessObjects.Cash
{
    [NavigationItem("Cash Data")]
    [ModelDefault("ImageName", "BO_List")]
    public class AccountBalance : BaseObject
    {
        public AccountBalance(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
   
        }

        private CashFlowSnapshot _Snapshot;
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

        private DateTime _TranDate;
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
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

        private decimal _AccountCcyAmt;
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
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

        private decimal _FunctionalCcyAmt;
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
                SetPropertyValue("FunctionalCcyAmt", ref _FunctionalCcyAmt, value);
            }
        }

    }
}
