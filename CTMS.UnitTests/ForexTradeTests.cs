using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Cash;
using DevExpress.Data.Filtering;
using CTMS.Module.BusinessObjects.Forex;
using DevExpress.Persistent.Validation;
using DevExpress.Persistent.Base;
using CTMS.Module.Controllers.Forex;
using DevExpress.ExpressApp.Xpo;
using System.Diagnostics;
using CTMS.UnitTests.Base;
using DevExpress.ExpressApp;

namespace CTMS.UnitTests
{
    public class ForexTradeTests : TestBase
    {
        public ForexTradeTests()
        {
            SetTesterDbType(TesterDbType.MsSql);
        }

        /// <summary>
        /// Cash Flows with an Account.Currency that equals the Functional Currency 
        /// will have ForexLinkIsClosed property set to TRUE
        /// </summary>
        [Test]
        public void ForexLinkIsClosedIfCashFlowAccountCurrencyIsFunctional()
        {
            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ 70086";
            account.Currency = ccyAUD;

            var cfIn1 = ObjectSpace.CreateObject<CashFlow>();
            cfIn1.TranDate = new DateTime(2012, 06, 01);
            cfIn1.Account = account;
            cfIn1.AccountCcyAmt = 2000000;
            cfIn1.Save();

            Assert.AreEqual(true, cfIn1.ForexLinkIsClosed);
        }
        
        [Test]
        public void InvalidIfNoValueDate()
        {
            var ccyAUD = ObjectSpace.CreateObject<Currency>();
            var ccyUSD = ObjectSpace.CreateObject<Currency>();

            // create counterparty
            var fxCounterparty = ObjectSpace.CreateObject<ForexCounterparty>();

            var ft = ObjectSpace.CreateObject<ForexTrade>();

            ft.CalculateEnabled = true;
            ft.PrimaryCcy = ccyAUD;
            ft.CounterCcy = ccyUSD;
            ft.Rate = 0.9M;
            ft.CounterCcyAmt = 1000;
            ft.Counterparty = fxCounterparty;
            ft.ValueDate = default(DateTime);

            RuleSetValidationResult result = Validator.RuleSet.ValidateTarget(ObjectSpace, ft, DefaultContexts.Save);
            Assert.AreEqual(ValidationState.Invalid, result.State);
        }

        [Test]
        public void InvalidIfNoCounterparty()
        {
            var ccyAUD = ObjectSpace.CreateObject<Currency>();
            var ccyUSD = ObjectSpace.CreateObject<Currency>();

            // create counterparty

            var ft = ObjectSpace.CreateObject<ForexTrade>();

            ft.CalculateEnabled = true;
            ft.PrimaryCcy = ccyAUD;
            ft.CounterCcy = ccyUSD;
            ft.Rate = 0.9M;
            ft.CounterCcyAmt = 1000;
            ft.ValueDate = DateTime.Now;

            RuleSetValidationResult result = Validator.RuleSet.ValidateTarget(ObjectSpace, ft, DefaultContexts.Save);
            Assert.AreEqual(ValidationState.Invalid, result.State);
        }
        

        [Test]
        public void UploadForexTradeToCashFlow()
        {
            #region Arrange

            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));

            // create counterparty
            var fxCounterparty = ObjectSpace.CreateObject<ForexCounterparty>();
            fxCounterparty.Name = "ANZ";
            var counterparty = ObjectSpace.FindObject<Counterparty>(CriteriaOperator.Parse("Name = ?", "ANZ"));
            if (counterparty == null)
            {
                counterparty = ObjectSpace.CreateObject<Counterparty>();
                counterparty.Name = "ANZ";
            }
            fxCounterparty.CashFlowCounterparty = counterparty;

            // create accounts
            var usdAccount = ObjectSpace.CreateObject<Account>();
            usdAccount.Name = "VHA ANZ USD";
            usdAccount.Currency = ccyUSD;

            var audAccount = ObjectSpace.CreateObject<Account>();
            audAccount.Name = "VHA ANZ AUD";
            audAccount.Currency = ccyAUD;

            // create standard settlement accounts
            var usdSsa = ObjectSpace.CreateObject<ForexStdSettleAccount>();
            usdSsa.Account = usdAccount;
            usdSsa.Counterparty = fxCounterparty;
            usdSsa.Currency = ccyUSD;

            var audSsa = ObjectSpace.CreateObject<ForexStdSettleAccount>();
            audSsa.Account = audAccount;
            audSsa.Counterparty = fxCounterparty;
            audSsa.Currency = ccyAUD;

            ObjectSpace.CommitChanges();

            #endregion

            #region Act
            // create forex trade 1
            var ft1 = ObjectSpace.CreateObject<ForexTrade>();
            ft1.CalculateEnabled = true;
            ft1.SettleGroupId = 1;
            ft1.ValueDate = new DateTime(2013, 12, 31);
            ft1.PrimaryCcy = ccyAUD;
            ft1.CounterCcy = ccyUSD;
            ft1.Rate = 0.9M;
            ft1.CounterCcyAmt = 1000;
            ft1.Counterparty = fxCounterparty;

            // create forex trade 2
            var ft2 = ObjectSpace.CreateObject<ForexTrade>();
            ft2.CalculateEnabled = true;
            ft2.SettleGroupId = 1;
            ft2.ValueDate = new DateTime(2013, 12, 31);
            ft2.PrimaryCcy = ccyAUD;
            ft2.CounterCcy = ccyUSD;
            ft2.Rate = 0.95M;
            ft2.CounterCcyAmt = 2000;
            ft2.Counterparty = fxCounterparty;
            ft2.CounterSettleAccount = usdAccount;
            ft2.PrimarySettleAccount = audAccount;

            ObjectSpace.CommitChanges();

            var uploader = new ForexTradeBatchUploaderImpl(ObjectSpace);
            uploader.UploadToCashFlowForecast();
            #endregion

            #region Assert
            Assert.AreEqual(ft1.PrimaryCashFlow, ft2.PrimaryCashFlow);
            Assert.AreEqual(ft1.CounterCashFlow, ft2.CounterCashFlow);

            Assert.AreEqual(audAccount, ft1.PrimarySettleAccount);
            Assert.AreEqual(usdAccount, ft1.CounterSettleAccount);

            decimal targetPrimaryCcyAmt = Math.Round(1000 / 0.9M, 2);
            Assert.AreEqual(targetPrimaryCcyAmt, ft1.PrimaryCcyAmt);

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            var priCf1 = cashFlows.FirstOrDefault(x => x.Oid == ft1.PrimaryCashFlow.Oid);
            Assert.AreEqual(-ft1.PrimaryCcyAmt - ft2.PrimaryCcyAmt, priCf1.AccountCcyAmt);
            Assert.AreEqual(-ft1.PrimaryCcyAmt - ft2.PrimaryCcyAmt, priCf1.FunctionalCcyAmt);
            Assert.AreEqual(-ft1.CounterCcyAmt - ft2.CounterCcyAmt, priCf1.CounterCcyAmt);
            Assert.AreEqual("USD", priCf1.CounterCcy.Name);

            var couCf1 = cashFlows.FirstOrDefault(x => x.Oid == ft1.CounterCashFlow.Oid);
            Assert.AreEqual(ft1.CounterCcyAmt + ft2.CounterCcyAmt, couCf1.CounterCcyAmt);
            Assert.AreEqual(ft1.PrimaryCcyAmt + ft2.PrimaryCcyAmt, couCf1.FunctionalCcyAmt);
            Assert.AreEqual(ft1.CounterCcyAmt + ft2.CounterCcyAmt, couCf1.CounterCcyAmt);
            Assert.AreEqual("USD", couCf1.CounterCcy.Name);

            #endregion
        }

        [Test]
        public void ForexTradeIsValid()
        {
            #region Prepare
            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));

            // create counterparty
            var fxCounterparty = ObjectSpace.CreateObject<ForexCounterparty>();
            fxCounterparty.Name = "ANZ";
            var counterparty = ObjectSpace.FindObject<Counterparty>(CriteriaOperator.Parse("Name = ?", "ANZ"));
            if (counterparty == null)
            {
                counterparty = ObjectSpace.CreateObject<Counterparty>();
                counterparty.Name = "ANZ";
            }
            fxCounterparty.CashFlowCounterparty = counterparty;

            // create accounts
            var usdAccount = ObjectSpace.CreateObject<Account>();
            usdAccount.Name = "VHA ANZ USD";
            usdAccount.Currency = ccyUSD;

            var audAccount = ObjectSpace.CreateObject<Account>();
            audAccount.Name = "VHA ANZ AUD";
            audAccount.Currency = ccyAUD;

            // create standard settlement accounts
            var usdSsa = ObjectSpace.CreateObject<ForexStdSettleAccount>();
            usdSsa.Account = usdAccount;
            usdSsa.Counterparty = fxCounterparty;
            usdSsa.Currency = ccyUSD;

            var audSsa = ObjectSpace.CreateObject<ForexStdSettleAccount>();
            audSsa.Account = audAccount;
            audSsa.Counterparty = fxCounterparty;
            audSsa.Currency = ccyAUD;

            ObjectSpace.CommitChanges();

            #endregion

            #region Act
            // create forex trade 1
            var ft1 = ObjectSpace.CreateObject<ForexTrade>();
            ft1.CalculateEnabled = true;
            ft1.ValueDate = new DateTime(2013, 12, 31);
            ft1.PrimaryCcy = ccyAUD;
            ft1.CounterCcy = ccyUSD;
            ft1.Rate = 0.9M;
            ft1.CounterCcyAmt = 1000;
            ft1.Counterparty = fxCounterparty;

            ObjectSpace.CommitChanges();

            #endregion

            #region Assert
            RuleSetValidationResult result = Validator.RuleSet.ValidateTarget(ObjectSpace, ft1, DefaultContexts.Save);
            Assert.AreEqual(ValidationState.Valid, result.State);
            #endregion
        }

        [Test]
        public void PredeliverForexTrade()
        {
            #region Arrange
            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));

            // create counterparty
            var fxCounterparty = ObjectSpace.CreateObject<ForexCounterparty>();
            fxCounterparty.Name = "ANZ";
            var counterparty = ObjectSpace.FindObject<Counterparty>(CriteriaOperator.Parse("Name = ?", "ANZ"));
            if (counterparty == null)
            {
                counterparty = ObjectSpace.CreateObject<Counterparty>();
                counterparty.Name = "ANZ";
            }
            fxCounterparty.CashFlowCounterparty = counterparty;

            // create accounts
            var usdAccount = ObjectSpace.CreateObject<Account>();
            usdAccount.Name = "VHA ANZ USD";
            usdAccount.Currency = ccyUSD;

            var audAccount = ObjectSpace.CreateObject<Account>();
            audAccount.Name = "VHA ANZ AUD";
            audAccount.Currency = ccyAUD;

            // create standard settlement accounts
            var usdSsa = ObjectSpace.CreateObject<ForexStdSettleAccount>();
            usdSsa.Account = usdAccount;
            usdSsa.Counterparty = fxCounterparty;
            usdSsa.Currency = ccyUSD;

            var audSsa = ObjectSpace.CreateObject<ForexStdSettleAccount>();
            audSsa.Account = audAccount;
            audSsa.Counterparty = fxCounterparty;
            audSsa.Currency = ccyAUD;

            // create forex trade 1
            var ft1 = ObjectSpace.CreateObject<ForexTrade>();
            ft1.CalculateEnabled = true;
            ft1.ValueDate = new DateTime(2013, 12, 31);
            ft1.PrimaryCcy = ccyAUD;
            ft1.CounterCcy = ccyUSD;
            ft1.CounterSettleAccount = usdAccount;
            ft1.PrimarySettleAccount = audAccount;
            ft1.Rate = 0.9M;
            ft1.CounterCcyAmt = 1000;
            ft1.Counterparty = fxCounterparty;

            ObjectSpace.CommitChanges();

            #endregion

            #region Act

            const decimal pdCounterCcyAmt1 = 1000;
            DateTime pdValueDate1 = new DateTime(2013, 11, 30);
            decimal pdRate1 = 0.92M;
            decimal pdPrimaryCcyAmt1 = Math.Round(pdCounterCcyAmt1 / pdRate1, 2);
            var pdy1 = ft1.Predeliver(pdCounterCcyAmt1, pdValueDate1, pdRate1);

            ObjectSpace.CommitChanges();

            var uploader = new ForexTradeBatchUploaderImpl(ObjectSpace);
            uploader.UploadToCashFlowForecast();

            #endregion

            #region Assert

            // Assert predelivery object values

            Assert.AreEqual(ft1, pdy1.FromForexTrade);
            Assert.AreEqual(pdCounterCcyAmt1, pdy1.ToForexTrade.CounterCcyAmt);
            Assert.AreEqual(pdRate1, pdy1.ToForexTrade.Rate);
            Assert.AreEqual(pdPrimaryCcyAmt1, pdy1.ToForexTrade.PrimaryCcyAmt);
            Assert.AreEqual(pdValueDate1, pdy1.ToForexTrade.ValueDate);
            Assert.AreEqual(-pdCounterCcyAmt1, pdy1.AmendForexTrade.CounterCcyAmt);
            Assert.AreEqual(ft1.Rate, pdy1.AmendForexTrade.Rate);
            Assert.AreEqual(Math.Round(-pdCounterCcyAmt1 / ft1.Rate, 2), pdy1.AmendForexTrade.PrimaryCcyAmt);

            // Assert cashflow object values


            var cfs2 = ObjectSpace.GetObjects<CashFlow>();
            var couCf2 = cfs2.FirstOrDefault(x => x == pdy1.ToForexTrade.CounterCashFlow);
            var priCf2 = cfs2.FirstOrDefault(x => x == pdy1.ToForexTrade.PrimaryCashFlow);

            Assert.AreEqual(pdPrimaryCcyAmt1, couCf2.FunctionalCcyAmt);
            Assert.AreEqual(-pdPrimaryCcyAmt1, priCf2.FunctionalCcyAmt);

            #endregion
        }

        [Test]
        public void UploadForexTradeToCashFlow2()
        {
            #region Create Forex objects
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
            #endregion

            #region Create Lookup Objects

            var priAccount = ObjectSpace.CreateObject<Account>();
            priAccount.Name = "VHA ANZ 70086";
            priAccount.Currency = ccyAUD;

            var couAccount = ObjectSpace.CreateObject<Account>();
            couAccount.Name = "VHA ANZ USD";
            couAccount.Currency = ccyUSD;

            var outActivity = ObjectSpace.CreateObject<Activity>();
            outActivity.Name = "AP Pymt";

            var outCounterparty = ObjectSpace.CreateObject<Counterparty>();
            outCounterparty.Name = "UNDEFINED";

            var inCounterparty = ObjectSpace.CreateObject<Counterparty>();
            inCounterparty.Name = "ANZ";

            var forexCounterparty = ObjectSpace.CreateObject<ForexCounterparty>();
            forexCounterparty.Name = "ANZ";
            forexCounterparty.CashFlowCounterparty = inCounterparty;
            #endregion

            #region Create Forex Trades

            var ft1 = ObjectSpace.CreateObject<ForexTrade>();
            ft1.ValueDate = new DateTime(2013, 11, 16);
            ft1.Counterparty = forexCounterparty;
            ft1.PrimarySettleAccount = priAccount;
            ft1.CounterSettleAccount = couAccount;
            ft1.PrimaryCcy = ccyAUD;
            ft1.CounterCcy = ccyUSD;
            ft1.Rate = 0.95M;
            ft1.CounterCcyAmt = 100;
            ft1.SettleGroupId = 1;

            var ft2 = ObjectSpace.CreateObject<ForexTrade>();
            ft2.ValueDate = new DateTime(2013, 11, 30);
            ft2.Counterparty = forexCounterparty;
            ft2.PrimarySettleAccount = priAccount;
            ft2.CounterSettleAccount = couAccount;
            ft2.PrimaryCcy = ccyAUD;
            ft2.CounterCcy = ccyUSD;
            ft2.Rate = 0.99M;
            ft2.CounterCcyAmt = 50;
            ft2.SettleGroupId = 2;

            var ft3 = ObjectSpace.CreateObject<ForexTrade>();
            ft3.ValueDate = new DateTime(2013, 11, 30);
            ft3.Counterparty = forexCounterparty;
            ft3.PrimarySettleAccount = priAccount;
            ft3.CounterSettleAccount = couAccount;
            ft3.PrimaryCcy = ccyAUD;
            ft3.CounterCcy = ccyUSD;
            ft3.Rate = 0.87M;
            ft3.CounterCcyAmt = 30;
            ft3.SettleGroupId = 3;

            ObjectSpace.CommitChanges();
            #endregion

            #region Act

            var uploader = new ForexTradeBatchUploaderImpl(ObjectSpace);
            uploader.UploadToCashFlowForecast();

            #endregion

            #region Assert

            var fts = ObjectSpace.GetObjects<ForexTrade>();

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();
            Assert.AreEqual(6, cashFlows.Count);
            Assert.AreEqual(0, Math.Round(cashFlows.Sum(x => x.FunctionalCcyAmt), 2));

            var cfAUD = cashFlows.Where(c => c.Account.Name == priAccount.Name);
            Assert.AreEqual(fts.Sum(f => f.PrimaryCcyAmt), -cfAUD.Sum(c => c.FunctionalCcyAmt));

            var cfUSD = cashFlows.Where(c => c.Account.Name == couAccount.Name);
            Assert.AreEqual(fts.Sum(f => f.CounterCcyAmt), cfUSD.Sum(c => c.CounterCcyAmt));
            #endregion
        }

        public override void OnSetup()
        {
            CTMS.Module.DatabaseUpdate.Updater.CreateCurrencies(ObjectSpace);
            SetOfBooks.GetInstance(ObjectSpace);
            CTMS.Module.DatabaseUpdate.Updater.InitSetOfBooks(ObjectSpace);
        }

        public override void OnAddExportedTypes(ModuleBase module)
        {
            CTMSTestHelper.AddExportedTypes(module);
        }
    }
}
