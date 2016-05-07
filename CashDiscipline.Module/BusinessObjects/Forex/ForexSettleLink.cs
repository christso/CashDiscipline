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

namespace CashDiscipline.Module.BusinessObjects.Forex
{
    [ModelDefault("ImageName", "BO_List")]
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (http://documentation.devexpress.com/#Xaf/CustomDocument2701).
    public class ForexSettleLink : BaseObject
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
                SetPropertyValue("CashFlowIn", ref _CashFlowIn, value);
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
                SetPropertyValue("CashFlowOut", ref _CashFlowOut, value);
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
                return CashFlowOut == null ? default(DateTime) : CashFlowOut.TranDate;
            }
        }

        [DbType("decimal(19, 6)")]
        [ModelDefault("EditMask", "n6")]
        [ModelDefault("DisplayFormat", "n6")]
        public decimal ForexSettleRate
        {
            get
            {
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
