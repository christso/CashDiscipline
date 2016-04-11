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

        private CashFlow _CashFlowIn;
        private CashFlow _CashFlowOut;
        private decimal _AccountCcyAmt;
        private DateTime _TimeCreated;
        private int _Step;
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
  
        [Association("CashFlowIn-ForexSettleLinks")]
        public CashFlow CashFlowIn
        {
            get
            {
                return _CashFlowIn;
            }
            set
            {
                var oldCashFlow = _CashFlowIn;
                SetPropertyValue("CashFlowIn", ref _CashFlowIn, value);
                if (!IsLoading && !IsSaving && calculateEnabled && oldCashFlow != _CashFlowIn)
                {
                    oldCashFlow = oldCashFlow ?? _CashFlowIn;
                    oldCashFlow.UpdateForexLinkedInAmt(true);
                    oldCashFlow.UpdateForexFunctionalCcyAmt(true);
                }
            }
        }
        [Association("CashFlowOut-ForexSettleLinks")]
        public CashFlow CashFlowOut
        {
            get
            {
                return _CashFlowOut;
            }
            set
            {
                var oldCashFlow = _CashFlowOut;
                SetPropertyValue("CashFlowOut", ref _CashFlowOut, value);
                if (!IsLoading && !IsSaving && calculateEnabled && oldCashFlow != _CashFlowOut)
                {
                    oldCashFlow = oldCashFlow ?? _CashFlowOut;
                    oldCashFlow.UpdateForexLinkedOutAmt(true);
                    oldCashFlow.UpdateForexFunctionalCcyAmt(true);
                }
            }
        }

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
                if (SetPropertyValue("AccountCcyAmt", ref _AccountCcyAmt, value))
                {
                    if (!IsLoading && !IsSaving && calculateEnabled)
                    {
                        if (_CashFlowOut != null)
                        {
                            _CashFlowOut.UpdateForexLinkedOutAmt(true);
                            _CashFlowOut.UpdateForexFunctionalCcyAmt(true);
                        }
                        if (_CashFlowIn != null)
                        {
                            _CashFlowIn.UpdateForexLinkedInAmt(true);
                            _CashFlowIn.UpdateForexFunctionalCcyAmt(true);
                        }
                    }
                }
            }
        }

        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime InDate
        {
            get
            {
                return CashFlowIn.TranDate;
            }
        }

        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime OutDate
        {
            get
            {
                return CashFlowOut.TranDate;
            }
        }

        public Account Account
        {
            get
            {
                return CashFlowIn.Account;
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

        [MemberDesignTimeVisibility(false)]
        public int Step
        {
            get
            {
                return _Step;
            }
            set
            {
                SetPropertyValue("Step", ref _Step, value);
            }
        }
    }
}
