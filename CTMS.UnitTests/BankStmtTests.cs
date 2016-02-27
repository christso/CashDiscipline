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
using CTMS.UnitTests.TestObjects;

namespace CTMS.UnitTests
{
    [TestFixture]
    public class BankStmtTests : TestBase
    {
        public BankStmtTests()
        {
            SetTesterDbType(TesterDbType.MsSql);
        }

        [Test]
        public void FunctionalCcyAmtIsCalculatedIfChangeTranAmount()
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
        public void CounterCcyChangedIfAccountChanged()
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
        
        [Test]
        public void UploadBankStmtToCashFlow()
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

            var forexActivity = ObjectSpace.GetObjectByKey<Activity>(SetOfBooks.CachedInstance.ForexSettleActivity.Oid);

            var outActivity = ObjectSpace.CreateObject<Activity>();
            outActivity.Name = "AP Pymt";

            var outCounterparty = ObjectSpace.CreateObject<Counterparty>();
            outCounterparty.Name = "UNDEFINED";

            var inCounterparty = ObjectSpace.CreateObject<Counterparty>();
            inCounterparty.Name = "ANZ";

            #endregion

            #region Create Bank Stmt objects

            decimal fRate1 = 0.95M;
            decimal fRate2 = 0.99M;
            decimal fRate3 = 0.87M;

            var bsCouForex1 = ObjectSpace.CreateObject<BankStmt>();
            bsCouForex1.TranDate = new DateTime(2013, 11, 16);
            bsCouForex1.Account = couAccount;
            bsCouForex1.Activity = forexActivity;
            bsCouForex1.Counterparty = outCounterparty;
            bsCouForex1.TranAmount = 100;
            bsCouForex1.ForexSettleType = CashFlowForexSettleType.In;
            bsCouForex1.SummaryDescription = "bsCouForex1";
            bsCouForex1.Save();

            var bsPriForex1 = ObjectSpace.CreateObject<BankStmt>();
            bsPriForex1.TranDate = new DateTime(2013, 11, 16);
            bsPriForex1.Account = priAccount;
            bsPriForex1.Activity = forexActivity;
            bsPriForex1.Counterparty = outCounterparty;
            bsPriForex1.TranAmount = -100 / fRate1;
            bsPriForex1.ForexSettleType = CashFlowForexSettleType.In;
            bsPriForex1.SummaryDescription = "bsPriForex1";
            bsPriForex1.Save();

            var bsCouForex2 = ObjectSpace.CreateObject<BankStmt>();
            bsCouForex2.TranDate = new DateTime(2013, 11, 30);
            bsCouForex2.Account = couAccount;
            bsCouForex2.Activity = forexActivity;
            bsCouForex2.Counterparty = outCounterparty;
            bsCouForex2.TranAmount = 50;
            bsCouForex2.ForexSettleType = CashFlowForexSettleType.In;
            bsCouForex2.SummaryDescription = "bsCouForex2";
            bsCouForex2.Save();

            var bsPriForex2 = ObjectSpace.CreateObject<BankStmt>();
            bsPriForex2.TranDate = new DateTime(2013, 11, 30);
            bsPriForex2.Account = priAccount;
            bsPriForex2.Activity = forexActivity;
            bsPriForex2.Counterparty = outCounterparty;
            bsPriForex2.TranAmount = -50 / fRate2;
            bsPriForex2.ForexSettleType = CashFlowForexSettleType.In;
            bsPriForex2.SummaryDescription = "bsPriForex2";
            bsPriForex2.Save();

            var bsCouForex3 = ObjectSpace.CreateObject<BankStmt>();
            bsCouForex3.TranDate = new DateTime(2013, 11, 30);
            bsCouForex3.Account = couAccount;
            bsCouForex3.Activity = forexActivity;
            bsCouForex3.Counterparty = outCounterparty;
            bsCouForex3.TranAmount = 30;
            bsCouForex3.ForexSettleType = CashFlowForexSettleType.In;
            bsCouForex3.SummaryDescription = "bsCouForex3";
            bsCouForex3.Save();

            var bsPriForex3 = ObjectSpace.CreateObject<BankStmt>();
            bsPriForex3.TranDate = new DateTime(2013, 11, 30);
            bsPriForex3.Account = priAccount;
            bsPriForex3.Activity = forexActivity;
            bsPriForex3.Counterparty = outCounterparty;
            bsPriForex3.TranAmount = -30 / fRate3;
            bsPriForex3.ForexSettleType = CashFlowForexSettleType.In;
            bsPriForex3.SummaryDescription = "bsPriForex3";
            bsPriForex3.Save();

            var bsOut1 = ObjectSpace.CreateObject<BankStmt>();
            bsOut1.TranDate = new DateTime(2013, 12, 17);
            bsOut1.Account = couAccount;
            bsOut1.Activity = outActivity;
            bsOut1.Counterparty = outCounterparty;
            bsOut1.TranAmount = -110;
            bsOut1.ForexSettleType = CashFlowForexSettleType.Out;
            bsOut1.SummaryDescription = "bsOut1";
            bsOut1.Save();

            var bsOut2 = ObjectSpace.CreateObject<BankStmt>();
            bsOut2.TranDate = new DateTime(2013, 12, 17);
            bsOut2.Account = couAccount;
            bsOut2.Activity = outActivity;
            bsOut2.Counterparty = outCounterparty;
            bsOut2.TranAmount = 105;
            bsOut2.ForexSettleType = CashFlowForexSettleType.Out;
            bsOut2.SummaryDescription = "bsOut2";
            bsOut2.Save();

            var bsOut3 = ObjectSpace.CreateObject<BankStmt>();
            bsOut3.TranDate = new DateTime(2013, 12, 17);
            bsOut3.Account = couAccount;
            bsOut3.Activity = outActivity;
            bsOut3.Counterparty = outCounterparty;
            bsOut3.TranAmount = -105;
            bsOut3.ForexSettleType = CashFlowForexSettleType.Out;
            bsOut3.SummaryDescription = "bsOut3";
            bsOut3.Save();

            var bsOut4 = ObjectSpace.CreateObject<BankStmt>();
            bsOut4.TranDate = new DateTime(2013, 12, 17);
            bsOut4.Account = couAccount;
            bsOut4.Activity = outActivity;
            bsOut4.Counterparty = outCounterparty;
            bsOut4.TranAmount = -30;
            bsOut4.ForexSettleType = CashFlowForexSettleType.Out;
            bsOut4.SummaryDescription = "bsOut4";
            bsOut4.Save();

            var bsOut5 = ObjectSpace.CreateObject<BankStmt>();
            bsOut5.TranDate = new DateTime(2013, 12, 17);
            bsOut5.Account = couAccount;
            bsOut5.Activity = outActivity;
            bsOut5.Counterparty = outCounterparty;
            bsOut5.TranAmount = 45;
            bsOut5.ForexSettleType = CashFlowForexSettleType.Out;
            bsOut5.SummaryDescription = "bsOut5";
            bsOut5.Save();

            var bsOut6 = ObjectSpace.CreateObject<BankStmt>();
            bsOut6.TranDate = new DateTime(2013, 12, 17);
            bsOut6.Account = couAccount;
            bsOut6.Activity = outActivity;
            bsOut6.Counterparty = outCounterparty;
            bsOut6.TranAmount = -45;
            bsOut6.ForexSettleType = CashFlowForexSettleType.Out;
            bsOut6.SummaryDescription = "bsOut6";
            bsOut6.Save();

            var bsOut7 = ObjectSpace.CreateObject<BankStmt>();
            bsOut7.TranDate = new DateTime(2013, 12, 17);
            bsOut7.Account = couAccount;
            bsOut7.Activity = outActivity;
            bsOut7.Counterparty = outCounterparty;
            bsOut7.TranAmount = -25;
            bsOut7.ForexSettleType = CashFlowForexSettleType.Out;
            bsOut7.SummaryDescription = "bsOut7";
            bsOut7.Save();

            var bsOut8 = ObjectSpace.CreateObject<BankStmt>();
            bsOut8.TranDate = new DateTime(2013, 12, 17);
            bsOut8.Account = couAccount;
            bsOut8.Activity = outActivity;
            bsOut8.Counterparty = outCounterparty;
            bsOut8.TranAmount = -19;
            bsOut8.ForexSettleType = CashFlowForexSettleType.Out;
            bsOut8.SummaryDescription = "bsOut8";
            bsOut8.Save();
            #endregion

            #region Act

            ObjectSpace.CommitChanges();

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();
            var fromDate = new DateTime(2013, 11, 16);
            var toDate = new DateTime(2013, 12, 17);
            var deleter = new CashFlowDeleter(ObjectSpace.Session, fromDate, toDate);
            var uploader = new BankStmtToCashFlow(ObjectSpace, fromDate, toDate, deleter);
            uploader.Process();
            uploader.Process(); // upload again to ensure previous upload is deleted

            #endregion

            #region Assert

            Assert.AreEqual(14, cashFlows.Count());
            #endregion
        }

        [Test]
        public void AutoreconcileBankStmtToCashFlow()
        {
            #region Arrange Forex objects
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

            // Constants
            decimal rate1 = 0.95M;
            decimal rate3 = 0.87M;
            decimal rate2 = 0.99M;

            #endregion

            #region Arrange Lookup Objects

            var priAccount = ObjectSpace.CreateObject<Account>();
            priAccount.Name = "VHA ANZ 70086";
            priAccount.Currency = ccyAUD;

            var couAccount = ObjectSpace.CreateObject<Account>();
            couAccount.Name = "VHA ANZ USD";
            couAccount.Currency = ccyUSD;

            var forexActivity = ObjectSpace.GetObjectByKey<Activity>(SetOfBooks.CachedInstance.ForexSettleActivity.Oid);

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

            #region Create Cash Flow Forex Trade Objects

            var cfCouForex1 = ObjectSpace.CreateObject<CashFlow>();
            cfCouForex1.CalculateEnabled = false;
            cfCouForex1.TranDate = new DateTime(2013, 11, 16);
            cfCouForex1.Account = couAccount;
            cfCouForex1.Activity = forexActivity;
            cfCouForex1.Counterparty = inCounterparty;
            cfCouForex1.AccountCcyAmt = 100;
            cfCouForex1.FunctionalCcyAmt = 100 / rate1;
            cfCouForex1.CounterCcyAmt = 100;
            cfCouForex1.CounterCcy = ccyUSD;
            cfCouForex1.ForexSettleType = CashFlowForexSettleType.In;
            cfCouForex1.Description = "cfCouForex1";
            cfCouForex1.Save();

            var cfPriForex1 = ObjectSpace.CreateObject<CashFlow>();
            cfPriForex1.CalculateEnabled = false;
            cfPriForex1.TranDate = new DateTime(2013, 11, 16);
            cfPriForex1.Account = priAccount;
            cfPriForex1.Activity = forexActivity;
            cfPriForex1.Counterparty = inCounterparty;
            cfPriForex1.AccountCcyAmt = -100 / rate1;
            cfPriForex1.FunctionalCcyAmt = -100 / rate1;
            cfPriForex1.CounterCcyAmt = 100;
            cfPriForex1.CounterCcy = ccyUSD;
            cfPriForex1.ForexSettleType = CashFlowForexSettleType.In;
            cfPriForex1.Description = "cfPriForex1";
            cfPriForex1.Save();

            var cfCouForex2 = ObjectSpace.CreateObject<CashFlow>();
            cfCouForex2.CalculateEnabled = false;
            cfCouForex2.TranDate = new DateTime(2013, 11, 30);
            cfCouForex2.Account = couAccount;
            cfCouForex2.Activity = forexActivity;
            cfCouForex2.Counterparty = inCounterparty;
            cfCouForex2.AccountCcyAmt = 50;
            cfCouForex2.FunctionalCcyAmt = 50 / rate2;
            cfCouForex2.CounterCcyAmt = 50;
            cfCouForex2.CounterCcy = ccyUSD;
            cfCouForex2.ForexSettleType = CashFlowForexSettleType.In;
            cfCouForex2.Description = "cfCouForex2";
            cfCouForex2.Save();

            var cfPriForex2 = ObjectSpace.CreateObject<CashFlow>();
            cfPriForex2.CalculateEnabled = false;
            cfPriForex2.TranDate = new DateTime(2013, 11, 30);
            cfPriForex2.Account = priAccount;
            cfPriForex2.Activity = forexActivity;
            cfPriForex2.Counterparty = inCounterparty;
            cfPriForex2.AccountCcyAmt = -50 / rate2;
            cfPriForex2.FunctionalCcyAmt = -50 / rate2;
            cfPriForex2.CounterCcyAmt = -50;
            cfPriForex2.CounterCcy = ccyUSD;
            cfPriForex2.ForexSettleType = CashFlowForexSettleType.In;
            cfPriForex2.Description = "cfPriForex2";
            cfPriForex2.Save();

            var cfCouForex3 = ObjectSpace.CreateObject<CashFlow>();
            cfCouForex3.CalculateEnabled = false;
            cfCouForex3.TranDate = new DateTime(2013, 11, 30);
            cfCouForex3.Account = couAccount;
            cfCouForex3.Activity = forexActivity;
            cfCouForex3.Counterparty = inCounterparty;
            cfCouForex3.AccountCcyAmt = 30;
            cfCouForex3.FunctionalCcyAmt = 30 / rate3;
            cfCouForex3.CounterCcyAmt = 30;
            cfCouForex3.CounterCcy = ccyUSD;
            cfCouForex3.ForexSettleType = CashFlowForexSettleType.In;
            cfCouForex3.Description = "cfCouForex3";
            cfCouForex3.Save();

            var cfPriForex3 = ObjectSpace.CreateObject<CashFlow>();
            cfPriForex3.CalculateEnabled = false;
            cfPriForex3.TranDate = new DateTime(2013, 11, 30);
            cfPriForex3.Account = priAccount;
            cfPriForex3.Activity = forexActivity;
            cfPriForex3.Counterparty = inCounterparty;
            cfPriForex3.AccountCcyAmt = -30 / rate3;
            cfPriForex3.FunctionalCcyAmt = -30 / rate3;
            cfPriForex3.CounterCcyAmt = -30;
            cfPriForex3.CounterCcy = ccyUSD;
            cfPriForex3.ForexSettleType = CashFlowForexSettleType.In;
            cfPriForex3.Description = "cfPriForex3";
            cfPriForex3.Save();

            #endregion

            #region Arrange Bank Stmt Forex Trade objects

            var bsCouForex1 = ObjectSpace.CreateObject<BankStmt>();
            bsCouForex1.TranDate = new DateTime(2013, 11, 16);
            bsCouForex1.Account = couAccount;
            bsCouForex1.Activity = forexActivity;
            bsCouForex1.Counterparty = outCounterparty;
            bsCouForex1.TranAmount = 100;
            bsCouForex1.ForexSettleType = CashFlowForexSettleType.In;
            bsCouForex1.SummaryDescription = "bsCouForex1";
            bsCouForex1.Save();

            var bsPriForex1 = ObjectSpace.CreateObject<BankStmt>();
            bsPriForex1.TranDate = new DateTime(2013, 11, 16);
            bsPriForex1.Account = priAccount;
            bsPriForex1.Activity = forexActivity;
            bsPriForex1.Counterparty = outCounterparty;
            bsPriForex1.TranAmount = -100 / rate1;
            bsPriForex1.ForexSettleType = CashFlowForexSettleType.In;
            bsPriForex1.SummaryDescription = "bsPriForex1";
            bsPriForex1.Save();

            var bsCouForex2 = ObjectSpace.CreateObject<BankStmt>();
            bsCouForex2.TranDate = new DateTime(2013, 11, 30);
            bsCouForex2.Account = couAccount;
            bsCouForex2.Activity = forexActivity;
            bsCouForex2.Counterparty = outCounterparty;
            bsCouForex2.TranAmount = 50;
            bsCouForex2.ForexSettleType = CashFlowForexSettleType.In;
            bsCouForex2.SummaryDescription = "bsCouForex2";
            bsCouForex2.Save();

            var bsPriForex2 = ObjectSpace.CreateObject<BankStmt>();
            bsPriForex2.TranDate = new DateTime(2013, 11, 30);
            bsPriForex2.Account = priAccount;
            bsPriForex2.Activity = forexActivity;
            bsPriForex2.Counterparty = outCounterparty;
            bsPriForex2.TranAmount = -50 / rate2;
            bsPriForex2.ForexSettleType = CashFlowForexSettleType.In;
            bsPriForex2.SummaryDescription = "bsPriForex2";
            bsPriForex2.Save();

            var bsCouForex3 = ObjectSpace.CreateObject<BankStmt>();
            bsCouForex3.TranDate = new DateTime(2013, 11, 30);
            bsCouForex3.Account = couAccount;
            bsCouForex3.Activity = forexActivity;
            bsCouForex3.Counterparty = outCounterparty;
            bsCouForex3.TranAmount = 30;
            bsCouForex3.ForexSettleType = CashFlowForexSettleType.In;
            bsCouForex3.SummaryDescription = "bsCouForex3";
            bsCouForex3.Save();

            var bsPriForex3 = ObjectSpace.CreateObject<BankStmt>();
            bsPriForex3.TranDate = new DateTime(2013, 11, 30);
            bsPriForex3.Account = priAccount;
            bsPriForex3.Activity = forexActivity;
            bsPriForex3.Counterparty = outCounterparty;
            bsPriForex3.TranAmount = -30 / rate3;
            bsPriForex3.ForexSettleType = CashFlowForexSettleType.In;
            bsPriForex3.SummaryDescription = "bsPriForex3";
            bsPriForex3.Save();

            #endregion

            #region Act Autoreconciliation

            ObjectSpace.CommitChanges();
            var bankStmts = ObjectSpace.GetObjects<BankStmt>();
            BankStmtViewController.AutoreconcileForexTrades(bankStmts);

            #endregion

            #region Assert
            Assert.AreEqual(0, bankStmts.Sum(x => x.FunctionalCcyAmt));
            Assert.AreEqual(6, bankStmts.Where(x => x.Activity.Name == forexActivity.Name).Count());
            Assert.AreEqual(6, bankStmts.Where(x => x.Counterparty.Name == inCounterparty.Name).Count());
            #endregion
        }

        public override void SetupObjects()
        {
            CTMS.Module.DatabaseUpdate.Updater.CreateCurrencies(ObjectSpace);
            SetOfBooks.GetInstance(ObjectSpace);
            CTMS.Module.DatabaseUpdate.Updater.InitSetOfBooks(ObjectSpace);
        }
    }
}
