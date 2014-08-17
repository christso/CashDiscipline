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

namespace CTMS.Module.BusinessObjects.Cash
{
    public class CashFlowFixDetail : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public CashFlowFixDetail(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
        }

        // Fields...
        private CashForecastFixTag _Fix;
        private CashFlow _CashFlowFixer;
        private CashFlowSource _Source;
        private Activity _Activity;
        private Currency _CounterCcy;
        private decimal _FunctionalCcyAmt;
        private decimal _CounterCcyAmt;
        private decimal _AccountCcyAmt;
        private DateTime _TranDate;
        private CashFlow _CashFlow;

        public CashFlow CashFlowFixee
        {
            get
            {
                return _CashFlow;
            }
            set
            {
                SetPropertyValue("CashFlowFixee", ref _CashFlow, value);
            }
        }

        public CashFlow CashFlowFixer
        {
            get
            {
                return _CashFlowFixer;
            }
            set
            {
                SetPropertyValue("CashFlowFixer", ref _CashFlowFixer, value);
            }
        }

        public DateTime TranDate
        {
            get
            {
                return _TranDate;
            }
            set
            {
                SetPropertyValue("FixTranDate", ref _TranDate, value);
            }
        }


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

        public decimal CounterCcyAmt
        {
            get
            {
                return _CounterCcyAmt;
            }
            set
            {
                SetPropertyValue("CounterCcyAmt", ref _CounterCcyAmt, value);
            }
        }


        public Currency CounterCcy
        {
            get
            {
                return _CounterCcy;
            }
            set
            {
                SetPropertyValue("CounterCcy", ref _CounterCcy, value);
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

        public CashFlowSource Source
        {
            get
            {
                return _Source;
            }
            set
            {
                SetPropertyValue("Source", ref _Source, value);
            }
        }


        public CashForecastFixTag Fix
        {
            get
            {
                return _Fix;
            }
            set
            {
                SetPropertyValue("Fix", ref _Fix, value);
            }
        }
    }
}
