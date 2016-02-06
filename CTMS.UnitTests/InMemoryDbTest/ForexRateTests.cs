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

namespace CTMS.UnitTests.InMemoryDbTest
{
    public class ForexRateTests : CTMS.UnitTests.Base.InMemoryDbTestBase
    {
        [Test]
        public void ForexRate_Create_PairCreated()
        {
            // Currencies
            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));

            // Forex Rates
            var rate = ObjectSpace.CreateObject<ForexRate>();
            rate.ConversionDate = new DateTime(2013, 11, 01);
            rate.FromCurrency = ccyAUD;
            rate.ToCurrency = ccyUSD;
            rate.ConversionRate = 0.9M;
            rate.Save();
            ObjectSpace.CommitChanges();

            var rates = ObjectSpace.GetObjects<ForexRate>();
            Assert.AreEqual(2, rates.Count);
        }
    }
}
