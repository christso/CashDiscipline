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
using CTMS.Module.BusinessObjects.Cash;

// With XPO, the data model is declared by classes (so-called Persistent Objects) that will define the database structure, and consequently, the user interface (http://documentation.devexpress.com/#Xaf/CustomDocument2600).
namespace CTMS.Module.BusinessObjects.Market
{
    // Specify various UI options for your persistent class and its properties using a declarative approach via built-in attributes (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
    //[ImageName("BO_Contact")]
    //[DefaultProperty("PersistentProperty")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewAndDetailView, true, NewItemRowPosition.Top)]
    public class ForexRate : BaseObject
    { // You can use a different base persistent class based on your requirements (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public ForexRate(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here or place it only when the IsLoading property is false.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code (check out http://documentation.devexpress.com/#Xaf/CustomDocument2834 for more details).
            FromCurrency = Session.FindObject<Currency>(CriteriaOperator.Parse("[Name] == 'AUD'"));
        }

        // Fields...
        private decimal _ConversionRate;
        private Currency _FromCurrency;
        private Currency _ToCurrency;
        private DateTime _ConversionDate;

        private bool _IsReversedStatus;

        public DateTime ConversionDate
        {
            get
            {
                return _ConversionDate;
            }
            set
            {
                SetPropertyValue("ConversionDate", ref _ConversionDate, value);
            }
        }

        public Currency FromCurrency
        {
            get
            {
                return _FromCurrency;
            }
            set
            {
                SetPropertyValue("FromCurrency", ref _FromCurrency, value);
            }
        }

        public Currency ToCurrency
        {
            get
            {
                return _ToCurrency;
            }
            set
            {
                SetPropertyValue("ToCurrency", ref _ToCurrency, value);
            }
        }

        [DbType("decimal(19, 6)")]
        public decimal ConversionRate
        {
            get
            {
                return _ConversionRate;
            }
            set
            {
                SetPropertyValue("ConversionRate", ref _ConversionRate, value);
            }
        }
        public class FieldNames
        {
            public const string ConversionDate = "ConversionDate";
            public const string ToCurrency = "ToCurrency";
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            if (!_IsReversedStatus && !IsDeleted)
            {
    
                var revForexRate = Session.FindObject<ForexRate>(CriteriaOperator.Parse(
                    "ConversionDate = ? And FromCurrency = ? And ToCurrency = ?",
                    ConversionDate, ToCurrency, FromCurrency));
                if (revForexRate == null)
                {
                    revForexRate = new ForexRate(Session)
                    {
                        ConversionDate = this.ConversionDate,
                        FromCurrency = this.ToCurrency,
                        ToCurrency = this.FromCurrency,
                        ConversionRate = 1 / this.ConversionRate,
                        _IsReversedStatus = true
                    };
                    revForexRate.Save();
                }
            }
        }
    }
}
