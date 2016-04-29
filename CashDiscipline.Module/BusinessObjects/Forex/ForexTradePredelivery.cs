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

namespace CashDiscipline.Module.BusinessObjects.Forex
{
    [ModelDefault("ImageName", "BO_List")]
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (http://documentation.devexpress.com/#Xaf/CustomDocument2701).
    public class ForexTradePredelivery : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public ForexTradePredelivery(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).

            TradeDate = DateTime.Now;
        }

        private ForexTrade _FromForexTrade;
        private ForexTrade _ToForexTrade;
        private ForexTrade _AmendForexTrade;
        private DateTime _TradeDate;
        private DateTime _ValueDate;
        private decimal _CounterCcyAmt;
        private decimal _PrimaryCcyAmt;
        private decimal _Rate;

        // TODO: Prevent changes on this field in View
        [RuleRequiredField("ForexTradePredelivery.FromForexTrade_RuleRequiredField", DefaultContexts.Save)]
        [Association("FromForexTrade-ForexTradePredelivery")]
        public ForexTrade FromForexTrade
        {
            get
            {
                return _FromForexTrade;
            }
            set
            {
                if (SetPropertyValue("FromForexTrade", ref _FromForexTrade, value))
                {
                    if (!IsLoading && !IsSaving && _FromForexTrade != null)
                    {
                        CreateAmendForexTrade();
                        CreateToForexTrade();
                    }
                }
            }
        }

        [ModelDefault("AllowEdit", "false")]
        [Association("ToForexTrade-ForexTradePredelivery")]
        public ForexTrade ToForexTrade
        {
            get
            {
                if (!IsLoading && !IsSaving && !IsDeleted
                    && _FromForexTrade != null && _ToForexTrade == null)
                {
                    CreateToForexTrade();
                }
                return _ToForexTrade;
            }
            set
            {
                SetPropertyValue("ToForexTrade", ref _ToForexTrade, value);
            }
        }

        [ModelDefault("AllowEdit", "false")]
        [Association("AmendForexTrade-ForexTradePredelivery")]
        public ForexTrade AmendForexTrade
        {
            get
            {
                if (!IsLoading && !IsSaving && !IsDeleted
                  && _FromForexTrade != null && _AmendForexTrade == null)
                {
                    CreateAmendForexTrade();

                }
                return _AmendForexTrade;
            }
            set
            {
                SetPropertyValue("AmendForexTrade", ref _AmendForexTrade, value);
            }
        }

        [ModelDefault("EditMask", "dd-MMM-yy HH:mm:ss")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy HH:mm:ss")]
        public DateTime TradeDate
        {
            get
            {
                return _TradeDate;
            }
            set
            {
                SetPropertyValue("TradeDate", ref _TradeDate, value);
            }
        }

        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [RuleRequiredField("ForexTradePredelivery.ValueDate_RuleRequiredField", DefaultContexts.Save)]
        public DateTime ValueDate
        {
            get
            {
                return _ValueDate;
            }
            set
            {
                if (SetPropertyValue("ValueDate", ref _ValueDate, value.Date))
                {
                    if (!IsLoading && !IsSaving)
                    {
                        ToForexTrade.ValueDate = _ValueDate;
                    }
                }
            }
        }

        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        [ImmediatePostData(true)]
        public decimal CounterCcyAmt
        {
            get
            {
                return _CounterCcyAmt;
            }
            set
            {
                if (SetPropertyValue("CounterCcyAmt", ref _CounterCcyAmt, value))
                {
                    if (!IsLoading && !IsSaving)
                    {
                        AmendForexTrade.CounterCcyAmt = -_CounterCcyAmt;
                        ToForexTrade.CounterCcyAmt = _CounterCcyAmt;
                        SetPropertyValue("PrimaryCcyAmt", ref _PrimaryCcyAmt, ToForexTrade.PrimaryCcyAmt);
                    }
                }
            }
        }

        [ModelDefault("EditMask", "n5")]
        [ModelDefault("DisplayFormat", "n5")]
        [DbType("decimal(19, 6)")]
        [ImmediatePostData(true)]
        public decimal Rate
        {
            get
            {
                return _Rate;
            }
            set
            {
                if (SetPropertyValue("Rate", ref _Rate, value))
                {
                    if (!IsLoading && !IsSaving)
                    {
                        ToForexTrade.Rate = _Rate;
                        SetPropertyValue("PrimaryCcyAmt", ref _PrimaryCcyAmt, ToForexTrade.PrimaryCcyAmt);
                    }
                }
            }
        }

        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        [ModelDefault("AllowEdit", "false")]
        public decimal PrimaryCcyAmt
        {
            get
            {
                return _PrimaryCcyAmt;
            }
        }

        public void CreateAmendForexTrade()
        {
            Cloner cloner = new Cloner();
            AmendForexTrade = (ForexTrade)cloner.CloneTo(FromForexTrade, typeof(ForexTrade));
            _AmendForexTrade.EventType = ForexEventType.Predeliver;
            _AmendForexTrade.OrigTrade = _FromForexTrade;
        }

        public void CreateToForexTrade()
        {
            Cloner cloner = new Cloner();
            ToForexTrade = (ForexTrade)cloner.CloneTo(FromForexTrade, typeof(ForexTrade));
            _ToForexTrade.EventType = ForexEventType.Predeliver;
            _ToForexTrade.OrigTrade = _FromForexTrade;
        }

        protected override void OnDeleting()
        {
            if (ToForexTrade != null)
                ToForexTrade.Delete();
            if (FromForexTrade != null)
                AmendForexTrade.Delete();
            base.OnDeleting();
        }

        public static class FieldNames
        {
            public const string ValueDate = "ValueDate";
            public const string CounterCcyAmt = "CounterCcyAmt";
        }
    }
}
