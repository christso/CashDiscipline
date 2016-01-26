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
    [TestFixture]
    public class CashFlowTests : InMemoryDbTestBase
    {
        // CounterCcy will change to USD when Account changed to USD Account
        [Test]
        public void CashFlow_AccountIsUSD_CounterCcyIsUSD()
        {
            // arrange
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));
            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ USD";
            account.Currency = ccyUSD;

            // act
            var cf = ObjectSpace.CreateObject<CashFlow>();
            cf.Account = account;

            // assert
            Assert.AreEqual(ccyUSD, cf.CounterCcy);
        }
        
        [Test]
        public void ForexRate_GetRate_LastRateForCurrency()
        {
            // arrange
            
            var funcCcy = SetOfBooks.CachedInstance.FunctionalCurrency;
            var fromCcy = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var session = ObjectSpace.Session;
            var tranDate = new DateTime(2015, 12, 21);

            var rate = ObjectSpace.CreateObject<ForexRate>();
            rate.FromCurrency = fromCcy;
            rate.ToCurrency = funcCcy;
            rate.ConversionDate = new DateTime(2013, 12, 31);
            rate.ConversionRate = 1M;
            ObjectSpace.CommitChanges();

            // act
            
            XPQuery<ForexRate> ratesQuery = new XPQuery<ForexRate>(((XPObjectSpace)ObjectSpace).Session);
            var maxDate = ratesQuery.Where(r => r.FromCurrency == fromCcy
                && r.ToCurrency == funcCcy
                && r.ConversionDate <= tranDate
                ).Max(x => x.ConversionDate);

            var rateObj = ratesQuery.Where(r => r.ConversionDate == maxDate
                && r.FromCurrency == fromCcy
                && r.ToCurrency == funcCcy).FirstOrDefault();

            // assert

            Assert.AreEqual(maxDate, new DateTime(2013, 12, 31));
            Assert.AreEqual(maxDate, rateObj.ConversionDate);
            Assert.AreEqual(1M, rateObj.ConversionRate);
            Assert.AreEqual("AUD", rateObj.FromCurrency.Name);
            Assert.AreEqual("AUD", rateObj.ToCurrency.Name);
        }

        [Test]
        public void CashFlow_AccountIsUSD_FuncCcyAmtConverted()
        {
            // arrange

            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));
            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ USD";
            account.Currency = ccyUSD;

            var rate = ObjectSpace.CreateObject<ForexRate>();
            rate.FromCurrency = ccyAUD;
            rate.ToCurrency = ccyUSD;
            rate.ConversionDate = new DateTime(2013, 12, 31);
            rate.ConversionRate = 0.9M;
            ObjectSpace.CommitChanges();

            // act
            var cf = ObjectSpace.CreateObject<CashFlow>();
            cf.TranDate = new DateTime(2013, 12, 31);
            cf.Account = account;
            cf.AccountCcyAmt = 1000;

            // assert
            Assert.AreEqual(Math.Round(cf.AccountCcyAmt / rate.ConversionRate, 2),
                Math.Round(cf.FunctionalCcyAmt, 2));
        }

        [Test]
        public void CashFlow_AccountIsUSD_CounterCcyAmtEqualsAccountCcyAmt()
        {
            // arrange

            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));
            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ USD";
            account.Currency = ccyUSD;

            var rate = ObjectSpace.CreateObject<ForexRate>();
            rate.FromCurrency = ccyAUD;
            rate.ToCurrency = ccyUSD;
            rate.ConversionDate = new DateTime(2013, 12, 31);
            rate.ConversionRate = 0.9M;
            ObjectSpace.CommitChanges();

            // act
            var cf = ObjectSpace.CreateObject<CashFlow>();
            cf.TranDate = new DateTime(2013, 12, 31);
            cf.Account = account;
            cf.AccountCcyAmt = 1000;

            // asset
            Assert.AreEqual(Math.Round(cf.CounterCcyAmt, 2),
                Math.Round(cf.AccountCcyAmt, 2));
        }

        [Test]
        public void CashFlow_NoDate_FunctionalCcyAmtIsCalculated()
        {
            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));
            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ USD";
            account.Currency = ccyUSD;

            var rate = ObjectSpace.CreateObject<ForexRate>();
            rate.FromCurrency = ccyAUD;
            rate.ToCurrency = ccyUSD;
            rate.ConversionDate = new DateTime(2013, 12, 31);
            rate.ConversionRate = 0.9M;
            ObjectSpace.CommitChanges();

            // no date
            var cf = ObjectSpace.CreateObject<CashFlow>();
            cf.Account = account;
            cf.AccountCcyAmt = 1000;
            Assert.AreEqual(1111.11, Math.Round(cf.FunctionalCcyAmt, 2));

            // add date later
            cf.TranDate = new DateTime(2013, 12, 31);
            Assert.AreEqual(1111.11, Math.Round(cf.FunctionalCcyAmt, 2));
        }

        [Test]
        public void GetMaxActualDate()
        {
            // arrange
            var cf1 = ObjectSpace.CreateObject<CashFlow>();
            cf1.Status = CashFlowStatus.Actual;
            cf1.TranDate = new DateTime(2015, 12, 03);

            var cf2 = ObjectSpace.CreateObject<CashFlow>();
            cf2.Status = CashFlowStatus.Actual;
            cf2.TranDate = new DateTime(2015, 12, 04);

            var cf3 = ObjectSpace.CreateObject<CashFlow>();
            cf3.Status = CashFlowStatus.Forecast;
            cf3.TranDate = new DateTime(2015, 12, 05);

            ObjectSpace.CommitChanges();

            // act
            var maxActualDate = CashFlow.GetMaxActualTranDate(ObjectSpace.Session);

            // assert
            Assert.AreEqual(new DateTime(2015, 12, 04), maxActualDate);
        }

        protected override void SetupObjects()
        {
            CTMS.Module.DatabaseUpdate.Updater.CreateCurrencies(ObjectSpace);
            SetOfBooks.GetInstance(ObjectSpace);
            CTMS.Module.DatabaseUpdate.Updater.InitSetOfBooks(ObjectSpace);
        }
    }

}
