using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Diagnostics;
using System.Linq;

// With XPO, the data model is declared by classes (so-called Persistent Objects) that will define the database structure, and consequently, the user interface (http://documentation.devexpress.com/#Xaf/CustomDocument2600).
namespace CashDiscipline.Module.BusinessObjects.Forex
{
    [ModelDefault("ImageName", "BO_List")]
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

        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
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
        [ModelDefault("DisplayFormat", "n6")]
        [ModelDefault("EditMask", "n6")]
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
                var revForexRate = Session.FindObject<CashDiscipline.Module.BusinessObjects.Forex.ForexRate>(CriteriaOperator.Parse(
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

        #region Rate Utilities

        public static ForexRate GetForexRateObject(Currency fromCcy, Currency toCcy, DateTime convDate)
        {
            var session = fromCcy.Session;
            if (session != toCcy.Session)
                throw new InvalidOperationException("Both currencies must be in the same session.");

            XPQuery<ForexRate> ratesQuery = new XPQuery<ForexRate>(session);
            
            var rates = ratesQuery.Where(r => r.FromCurrency == fromCcy
                && r.ToCurrency == toCcy
                && r.ConversionDate <= convDate
                );

            //Console.WriteLine("Rate Count = {0}", rates.Count());
            //foreach (var rate in rates)
            //    Console.WriteLine("{0} | {1} | {2}", rate.FromCurrency.Name, rate.ToCurrency.Name, rate.ConversionRate);

            var maxDate = rates.Max(x => x.ConversionDate);

            if (maxDate == default(DateTime))
                return null;

            var rateObj = ratesQuery.Where(r => r.ConversionDate == maxDate
                && r.FromCurrency == fromCcy
                && r.ToCurrency == toCcy).FirstOrDefault();

            return rateObj;
        }

        public static decimal GetForexRate(Currency fromCcy, Currency toCcy, DateTime convDate)
        {
            if (fromCcy == toCcy) return 1;
            var rateObj = GetForexRateObject(fromCcy, toCcy, convDate);
            if (rateObj != null)
                return rateObj.ConversionRate;
            return 0;
        }
        #endregion
    }
}
