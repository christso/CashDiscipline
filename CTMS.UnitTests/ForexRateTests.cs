using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTMS.Module.BusinessObjects.Forex;
using CTMS.Module.BusinessObjects;
using DevExpress.Persistent.Validation;
using System.Diagnostics;
using CTMS.Module.BusinessObjects.Cash;
using DevExpress.Data.Filtering;
using CTMS.Module.Controllers.Forex;
using DevExpress.ExpressApp;
using Xafology.ExpressApp.Xpo;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using CTMS.Module.Controllers.Cash;
using CTMS.Module.DatabaseUpdate;
using DevExpress.ExpressApp.Utils;
using CTMS.Module.ParamObjects.Cash;
using CTMS.UnitTests.Base;

namespace CTMS.UnitTests
{
    public class ForexRateTests : TestBase
    {
        public ForexRateTests()
        {
            SetTesterDbType(TesterDbType.MsSql);
        }

        [Test]
        public void CreateForexRatePair()
        {
            // Forex Rates
            var rate = ObjectSpace.CreateObject<ForexRate>();
            rate.ConversionDate = new DateTime(2013, 11, 01);
            rate.FromCurrency = null;
            rate.ToCurrency = null;
            rate.ConversionRate = 0.9M;
            rate.Save();
            ObjectSpace.CommitChanges();

            var rates = ObjectSpace.GetObjects<ForexRate>();
            Assert.AreEqual(2, rates.Count);
        }

        [Test]
        public void CalculateForexRate()
        {
            #region Create Forex objects
            
            // Currencies
            var ccyAUD = ObjectSpace.CreateObject<Currency>();
            ccyAUD.Name = "AUD";
            var ccyUSD = ObjectSpace.CreateObject<Currency>();
            ccyUSD.Name = "USD";

            // Forex Rates
            var rate = ObjectSpace.CreateObject<ForexRate>();
            rate.ConversionDate = new DateTime(2013, 11, 01);
            rate.FromCurrency = ccyAUD;
            rate.ToCurrency = ccyUSD;
            rate.ConversionRate = 0.9M;
            rate.Save();
            ObjectSpace.CommitChanges();
            #endregion

            var fromDate = new DateTime(2013, 11, 16);

            var rateRetrieved = GetForexRate(ObjectSpace.Session, ccyAUD, ccyUSD, fromDate);

            Assert.AreEqual(0.9M, rateRetrieved);
        }

        private static decimal GetForexRate(Session session, Currency fromCcy, Currency toCcy, DateTime convDate)
        {
            return ForexRate.GetForexRate(session, fromCcy, toCcy, convDate);
        }
    }
}
