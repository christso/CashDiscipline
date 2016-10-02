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
using Xafology.ExpressApp.BatchDelete;

namespace CashDiscipline.Module.BusinessObjects.Forex
{
    [ModelDefault("ImageName", "BO_List")]
    [BatchDelete(isVisible: true, isOptimized: true)]
    public class ForexSettleLink : BaseObject, IBatchDeletable
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public ForexSettleLink(Session session)
            : base(session)
        {
            calculateEnabled = true;
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
            TimeCreated = DateTime.Now;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
        }

        private bool calculateEnabled;
        [MemberDesignTimeVisibility(false), Browsable(false), NonPersistent]
        public bool CalculateEnabled
        {
            get { return calculateEnabled; }
            set
            {
                calculateEnabled = value;
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

        private CashFlow _CashFlowIn;
        [Association("CashFlowIn-ForexSettleLinks")]
        public CashFlow CashFlowIn
        {
            get
            {
                return _CashFlowIn;
            }
            set
            {
                if (SetPropertyValue("CashFlowIn", ref _CashFlowIn, value))
                {
                    //OnChanged("InDate");
                }
            }
        }

        private CashFlow _CashFlowOut;
        [Association("CashFlowOut-ForexSettleLinks")]
        public CashFlow CashFlowOut
        {
            get
            {
                return _CashFlowOut;
            }
            set
            {
                if (SetPropertyValue("CashFlowOut", ref _CashFlowOut, value))
                {
                    //OnChanged("OutDate");
                }
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

        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime InDate
        {
            get
            {
                return CashFlowIn == null ? default(DateTime) : CashFlowIn.TranDate;
            }
        }

        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime OutDate
        {
            get
            {
                return CashFlowOut == null ? default(DateTime) : CashFlowOut.TranDate; ;
            }
        }

        [DbType("decimal(19, 6)")]
        [ModelDefault("EditMask", "n6")]
        [ModelDefault("DisplayFormat", "n6")]
        public decimal ForexSettleRate
        {
            get
            {
                if (CashFlowIn == null) return 0;
                if (CashFlowIn.FunctionalCcyAmt == 0) return 0;
                return CashFlowIn.AccountCcyAmt / CashFlowIn.FunctionalCcyAmt;
            }
        }

        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal FunctionalCcyAmt
        {
            get
            {
                if (ForexSettleRate == 0) return 0;
                return AccountCcyAmt / ForexSettleRate;
            }
        }


        private DateTime _TimeCreated;
        [MemberDesignTimeVisibility(false)]
        public DateTime TimeCreated
        {
            get
            {
                return _TimeCreated;
            }
            set
            {
                SetPropertyValue("TimeCreated", ref _TimeCreated, value);
            }
        }
    }
}
