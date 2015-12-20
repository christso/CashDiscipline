using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;

using CTMS.Module.DatabaseUpdate;
using System.Diagnostics;
using CTMS.Module.BusinessObjects.Forex;
using CTMS.Module.Controllers.Forex;

namespace CTMS.UnitTests.MSSqlDbTest
{
    [TestFixture]
    public class BankStmtDbTests : MSSqlDbTestBase
    {

        [Test]
        public void BankStmt_TranAmountAsParam_FunctionalCcyAmtIsCalculated()
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

            var bs = ObjectSpace.CreateObject<BankStmt>();
            bs.TranDate = new DateTime(2013, 12, 31);
            bs.Account = account;
            bs.TranAmount = 1000;

            Assert.AreEqual(Math.Round(bs.TranAmount / rate.ConversionRate, 2),
                Math.Round(bs.FunctionalCcyAmt, 2));
            ObjectSpace.CommitChanges();
        }

        [Test]
        // CounterCcy will change when the Account is changed
        public void BankStmt_AccountChanged_CounterCcyChanged()
        {
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));
            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ USD";
            account.Currency = ccyUSD;
            var rate = ObjectSpace.CreateObject<ForexRate>();
            rate.ConversionDate = new DateTime(2013, 12, 31);
            rate.ConversionRate = 0.9M;

            var bs = ObjectSpace.CreateObject<BankStmt>();
            bs.Account = account;
            Assert.AreEqual(ccyUSD, bs.CounterCcy);
        }

        protected override void SetupObjects()
        {
            Updater.CreateCurrencies(ObjectSpace);
            SetOfBooks.GetInstance(ObjectSpace);
            Updater.InitSetOfBooks(ObjectSpace);
        }
    }
}
