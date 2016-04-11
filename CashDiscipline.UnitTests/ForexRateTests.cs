using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.BusinessObjects;
using DevExpress.Persistent.Validation;
using System.Diagnostics;
using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.Data.Filtering;
using CashDiscipline.Module.Controllers.Forex;
using DevExpress.ExpressApp;
using Xafology.ExpressApp.Xpo;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using CashDiscipline.Module.Controllers.Cash;
using CashDiscipline.Module.DatabaseUpdate;
using DevExpress.ExpressApp.Utils;
using CashDiscipline.Module.ParamObjects.Cash;
using Xafology.TestUtils;

namespace CashDiscipline.UnitTests
{
    public class ForexRateTests : TestBase
    {
        public ForexRateTests()
        {
            SetTesterDbType(TesterDbType.InMemory);
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

        public override void OnAddExportedTypes(ModuleBase module)
        {
            CashDisciplineTestHelper.AddExportedTypes(module);
        }
    }
}
