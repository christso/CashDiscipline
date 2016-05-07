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
using Xafology.TestUtils;
using CashDiscipline.Module.Logic.Forex;
using CashDiscipline.Module.ParamObjects.Forex;

namespace CashDiscipline.UnitTests
{
    [TestFixture]
    public class ForexLinkTests : TestBase
    {
        public ForexLinkTests()
        {
            SetTesterDbType(TesterDbType.MsSql);

            var tester = Tester as MSSqlDbTestBase;
            if (tester != null)
                tester.DatabaseName = Constants.TestDbName;
        }

        public override void OnSetup()
        {
            CashDiscipline.Module.DatabaseUpdate.Updater.CreateCurrencies(ObjectSpace);
            CashDiscipline.Module.DatabaseUpdate.Updater.InitSetOfBooks(ObjectSpace);
        }

        [TestCase(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 })]
        [TestCase(new int[] { 1, 3, 2, 9, 10, 7, 4, 8, 6, 5, 11 })]
        [TestCase(new int[] { 1, 2, 3, 9, 10, 7, 4, 8, 6, 5, 11 })]
        public void LinkCashFlowsTest1(int[] intparam)
        {
            Xafology.ExpressApp.Xpo.SequentialGuidBase.SequentialGuidBaseObject.IsSequential = true;
            Xafology.ExpressApp.Xpo.SequentialGuidBase.SequentialGuidBaseObject.OidInitializationMode = DevExpress.Persistent.BaseImpl.OidInitializationMode.OnSaving;

            #region Arrange Forex Objects

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

            #region Arrange Lookup Objects

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ USD";
            account.Currency = ccyUSD;

            var activity = ObjectSpace.CreateObject<Activity>();
            activity.Name = "AP Pymt";

            var counterparty = ObjectSpace.CreateObject<Counterparty>();
            counterparty.Name = "UNDEFINED";

            #endregion

            #region Arrange Cash Flow Objects

            var cfTable = new Dictionary<int, CashFlow>();
            int counter = 0;

            var cfIn1 = ObjectSpace.CreateObject<CashFlow>();

            cfIn1.CalculateEnabled = false;
            cfIn1.CounterCcy = ccyUSD;
            cfIn1.TranDate = new DateTime(2013, 11, 16);
            cfIn1.Account = account;
            cfIn1.Activity = activity;
            cfIn1.Counterparty = counterparty;
            cfIn1.AccountCcyAmt = 100;
            cfIn1.FunctionalCcyAmt = cfIn1.AccountCcyAmt / 0.95M;
            cfIn1.CounterCcyAmt = cfIn1.AccountCcyAmt;
            cfIn1.ForexSettleType = CashFlowForexSettleType.In;
            cfIn1.Description = "cfIn1";
            //cfIn1.BankStmts.Add(bsIn1);
            cfTable.Add(intparam[counter], cfIn1);
            counter++;

            var cfIn2 = ObjectSpace.CreateObject<CashFlow>();
            cfIn2.CalculateEnabled = false;
            cfIn2.CounterCcy = ccyUSD;
            cfIn2.TranDate = new DateTime(2013, 11, 30);
            cfIn2.Account = account;
            cfIn2.Activity = activity;
            cfIn2.Counterparty = counterparty;
            cfIn2.AccountCcyAmt = 50;
            cfIn2.FunctionalCcyAmt = cfIn2.AccountCcyAmt / 0.99M;
            cfIn2.CounterCcyAmt = cfIn2.AccountCcyAmt;
            cfIn2.ForexSettleType = CashFlowForexSettleType.In;
            cfIn2.Description = "cfIn2";
            //cfIn2.BankStmts.Add(bsIn2);
            cfTable.Add(intparam[counter], cfIn2);
            counter++;

            var cfIn3 = ObjectSpace.CreateObject<CashFlow>();
            cfIn3.CalculateEnabled = false;
            cfIn3.CounterCcy = ccyUSD;
            cfIn3.TranDate = new DateTime(2013, 11, 30);
            cfIn3.Account = account;
            cfIn3.Activity = activity;
            cfIn3.Counterparty = counterparty;
            cfIn3.AccountCcyAmt = 30;
            cfIn3.FunctionalCcyAmt = cfIn3.AccountCcyAmt / 0.87M;
            cfIn3.CounterCcyAmt = cfIn3.AccountCcyAmt;
            cfIn3.ForexSettleType = CashFlowForexSettleType.In;
            cfIn3.Description = "cfIn3";
            //cfIn3.BankStmts.Add(bsIn3);
            cfTable.Add(intparam[counter], cfIn3);
            counter++;

            var cfOut1 = ObjectSpace.CreateObject<CashFlow>();
            cfOut1.CalculateEnabled = false;
            cfOut1.CounterCcy = ccyUSD;
            cfOut1.TranDate = new DateTime(2013, 12, 17);
            cfOut1.Account = account;
            cfOut1.Activity = activity;
            cfOut1.Counterparty = counterparty;
            cfOut1.AccountCcyAmt = -110;
            cfOut1.FunctionalCcyAmt = cfOut1.AccountCcyAmt / rate.ConversionRate;
            cfOut1.CounterCcyAmt = cfOut1.AccountCcyAmt;
            cfOut1.ForexSettleType = CashFlowForexSettleType.Out;
            cfOut1.Description = "cfout1";
            //cfOut1.BankStmts.Add(bsOut1);
            cfTable.Add(intparam[counter], cfOut1);
            counter++;

            var cfOut2 = ObjectSpace.CreateObject<CashFlow>();
            cfOut2.CalculateEnabled = false;
            cfOut2.CounterCcy = ccyUSD;
            cfOut2.TranDate = new DateTime(2013, 12, 17);
            cfOut2.Account = account;
            cfOut2.Activity = activity;
            cfOut2.Counterparty = counterparty;
            cfOut2.AccountCcyAmt = 105;
            cfOut2.FunctionalCcyAmt = cfOut2.AccountCcyAmt / rate.ConversionRate;
            cfOut2.CounterCcyAmt = cfOut2.AccountCcyAmt;
            cfOut2.ForexSettleType = CashFlowForexSettleType.OutReclass;
            cfOut2.Description = "cfOut2";
            //cfOut2.BankStmts.Add(bsOut2);
            cfTable.Add(intparam[counter], cfOut2);
            counter++;

            var cfOut3 = ObjectSpace.CreateObject<CashFlow>();
            cfOut3.CalculateEnabled = false;
            cfOut3.CounterCcy = ccyUSD;
            cfOut3.TranDate = new DateTime(2013, 12, 17);
            cfOut3.Account = account;
            cfOut3.Activity = activity;
            cfOut3.Counterparty = counterparty;
            cfOut3.AccountCcyAmt = -105;
            cfOut3.FunctionalCcyAmt = cfOut3.AccountCcyAmt / rate.ConversionRate;
            cfOut3.CounterCcyAmt = cfOut3.AccountCcyAmt;
            cfOut3.ForexSettleType = CashFlowForexSettleType.OutReclass;
            cfOut3.Description = "cfOut3";
            //cfOut3.BankStmts.Add(bsOut3);
            cfTable.Add(intparam[counter], cfOut3);
            counter++;

            var cfOut4 = ObjectSpace.CreateObject<CashFlow>();
            cfOut4.CalculateEnabled = false;
            cfOut4.CounterCcy = ccyUSD;
            cfOut4.TranDate = new DateTime(2013, 12, 17);
            cfOut4.Account = account;
            cfOut4.Activity = activity;
            cfOut4.Counterparty = counterparty;
            cfOut4.AccountCcyAmt = -30;
            cfOut4.FunctionalCcyAmt = cfOut4.AccountCcyAmt / rate.ConversionRate;
            cfOut4.CounterCcyAmt = cfOut4.AccountCcyAmt;
            cfOut4.ForexSettleType = CashFlowForexSettleType.Out;
            cfOut4.Description = "cfOut4";
            //cfOut4.BankStmts.Add(bsOut4);
            cfTable.Add(intparam[counter], cfOut4);
            counter++;

            var cfOut5 = ObjectSpace.CreateObject<CashFlow>();
            cfOut5.CalculateEnabled = false;
            cfOut5.CounterCcy = ccyUSD;
            cfOut5.TranDate = new DateTime(2013, 12, 17);
            cfOut5.Account = account;
            cfOut5.Activity = activity;
            cfOut5.Counterparty = counterparty;
            cfOut5.AccountCcyAmt = 45;
            cfOut5.FunctionalCcyAmt = cfOut5.AccountCcyAmt / rate.ConversionRate;
            cfOut5.CounterCcyAmt = cfOut5.AccountCcyAmt;
            cfOut5.ForexSettleType = CashFlowForexSettleType.OutReclass;
            cfOut5.Description = "cfOut5";
            //cfOut5.BankStmts.Add(bsOut5);
            cfTable.Add(intparam[counter], cfOut5);
            counter++;

            var cfOut6 = ObjectSpace.CreateObject<CashFlow>();
            cfOut6.CalculateEnabled = false;
            cfOut6.CounterCcy = ccyUSD;
            cfOut6.TranDate = new DateTime(2013, 12, 17);
            cfOut6.Account = account;
            cfOut6.Activity = activity;
            cfOut6.Counterparty = counterparty;
            cfOut6.AccountCcyAmt = -45;
            cfOut6.FunctionalCcyAmt = cfOut6.AccountCcyAmt / rate.ConversionRate;
            cfOut6.CounterCcyAmt = cfOut6.AccountCcyAmt;
            cfOut6.ForexSettleType = CashFlowForexSettleType.OutReclass;
            cfOut6.Description = "cfOut6";
            //cfOut6.BankStmts.Add(bsOut6);
            cfTable.Add(intparam[counter], cfOut6);
            counter++;

            var cfOut7 = ObjectSpace.CreateObject<CashFlow>();
            cfOut7.CalculateEnabled = false;
            cfOut7.CounterCcy = ccyUSD;
            cfOut7.TranDate = new DateTime(2013, 12, 17);
            cfOut7.Account = account;
            cfOut7.Activity = activity;
            cfOut7.Counterparty = counterparty;
            cfOut7.AccountCcyAmt = -25;
            cfOut7.FunctionalCcyAmt = cfOut7.AccountCcyAmt / rate.ConversionRate;
            cfOut7.CounterCcyAmt = cfOut7.AccountCcyAmt;
            cfOut7.ForexSettleType = CashFlowForexSettleType.Out;
            cfOut7.Description = "cfOut7";
            //cfOut7.BankStmts.Add(bsOut7);
            cfTable.Add(intparam[counter], cfOut7);
            counter++;

            var cfOut8 = ObjectSpace.CreateObject<CashFlow>();
            cfOut8.CalculateEnabled = false;
            cfOut8.CounterCcy = ccyUSD;
            cfOut8.TranDate = new DateTime(2013, 12, 17);
            cfOut8.Account = account;
            cfOut8.Activity = activity;
            cfOut8.Counterparty = counterparty;
            cfOut8.AccountCcyAmt = -19;
            cfOut8.FunctionalCcyAmt = cfOut8.AccountCcyAmt / rate.ConversionRate;
            cfOut8.CounterCcyAmt = cfOut8.AccountCcyAmt;
            cfOut8.ForexSettleType = CashFlowForexSettleType.Out;
            cfOut8.Description = "cfOut8";
            //cfOut8.BankStmts.Add(bsOut8);
            cfTable.Add(intparam[counter], cfOut8);
            counter++;

            #endregion

            #region Act - generate OIDs

            foreach (var pair in cfTable.OrderBy(c => c.Key))
            {
                var cf = pair.Value;
                cf.GenerateOid();
            }
            ObjectSpace.CommitChanges();

            #endregion

            #region Act - run algorithm

            var paramObj = ObjectSpace.CreateObject<ForexSettleFifoParam>();
            var algo = new ForexSettleFifoAlgorithm(ObjectSpace, paramObj);
            algo.Process();
            ObjectSpace.CommitChanges();

            #endregion

            #region Assert

            var fsls = ObjectSpace.GetObjects<ForexSettleLink>();

            Assert.AreEqual(180, fsls.Sum(x => x.AccountCcyAmt));
            #endregion
        }

        [TestCase(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 })]
        [TestCase(new int[] { 4, 3, 2, 1, 5, 6, 7, 8, 9, 10, 11, 12 })]
        public void LinkCashFlowsTest2(int[] intparam)
        {
            Xafology.ExpressApp.Xpo.SequentialGuidBase.SequentialGuidBaseObject.IsSequential = true;
            Xafology.ExpressApp.Xpo.SequentialGuidBase.SequentialGuidBaseObject.OidInitializationMode = DevExpress.Persistent.BaseImpl.OidInitializationMode.OnSaving;

            #region Arrange Forex Objects

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

            #region Arrange Lookup Objects

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ USD";
            account.Currency = ccyUSD;

            var activity = ObjectSpace.CreateObject<Activity>();
            activity.Name = "AP Pymt";

            var counterparty = ObjectSpace.CreateObject<Counterparty>();
            counterparty.Name = "UNDEFINED";

            #endregion

            #region Arrange Cash Flow Objects

            var cfTable = new Dictionary<int, CashFlow>();
            int counter = 0;

            var cfIn1 = ObjectSpace.CreateObject<CashFlow>();

            cfIn1.CalculateEnabled = false;
            cfIn1.CounterCcy = ccyUSD;
            cfIn1.TranDate = new DateTime(2013, 11, 16);
            cfIn1.Account = account;
            cfIn1.Activity = activity;
            cfIn1.Counterparty = counterparty;
            cfIn1.AccountCcyAmt = 100;
            cfIn1.FunctionalCcyAmt = cfIn1.AccountCcyAmt / 0.95M;
            cfIn1.CounterCcyAmt = cfIn1.AccountCcyAmt;
            cfIn1.ForexSettleType = CashFlowForexSettleType.In;
            cfIn1.Description = "cfIn1";
            //cfIn1.BankStmts.Add(bsIn1);
            cfTable.Add(intparam[counter], cfIn1);
            counter++;

            var cfIn2 = ObjectSpace.CreateObject<CashFlow>();
            cfIn2.CalculateEnabled = false;
            cfIn2.CounterCcy = ccyUSD;
            cfIn2.TranDate = new DateTime(2013, 11, 30);
            cfIn2.Account = account;
            cfIn2.Activity = activity;
            cfIn2.Counterparty = counterparty;
            cfIn2.AccountCcyAmt = 50;
            cfIn2.FunctionalCcyAmt = cfIn2.AccountCcyAmt / 0.99M;
            cfIn2.CounterCcyAmt = cfIn2.AccountCcyAmt;
            cfIn2.ForexSettleType = CashFlowForexSettleType.In;
            cfIn2.Description = "cfIn2";
            //cfIn2.BankStmts.Add(bsIn2);
            cfTable.Add(intparam[counter], cfIn2);
            counter++;

            var cfIn3 = ObjectSpace.CreateObject<CashFlow>();
            cfIn3.CalculateEnabled = false;
            cfIn3.CounterCcy = ccyUSD;
            cfIn3.TranDate = new DateTime(2013, 11, 30);
            cfIn3.Account = account;
            cfIn3.Activity = activity;
            cfIn3.Counterparty = counterparty;
            cfIn3.AccountCcyAmt = 30;
            cfIn3.FunctionalCcyAmt = cfIn3.AccountCcyAmt / 0.87M;
            cfIn3.CounterCcyAmt = cfIn3.AccountCcyAmt;
            cfIn3.ForexSettleType = CashFlowForexSettleType.In;
            cfIn3.Description = "cfIn3";
            //cfIn3.BankStmts.Add(bsIn3);
            cfTable.Add(intparam[counter], cfIn3);
            counter++;

            var cfIn4 = ObjectSpace.CreateObject<CashFlow>();
            cfIn4.CalculateEnabled = false;
            cfIn4.CounterCcy = ccyUSD;
            cfIn4.TranDate = new DateTime(2013, 12, 18);
            cfIn4.Account = account;
            cfIn4.Activity = activity;
            cfIn4.Counterparty = counterparty;
            cfIn4.AccountCcyAmt = 10;
            cfIn4.FunctionalCcyAmt = cfIn4.AccountCcyAmt / 0.81M;
            cfIn4.CounterCcyAmt = cfIn4.AccountCcyAmt;
            cfIn4.ForexSettleType = CashFlowForexSettleType.In;
            cfIn4.Description = "cfIn4";
            //cfIn4.BankStmts.Add(bsIn3);
            cfTable.Add(intparam[counter], cfIn4);
            counter++;

            var cfOut1 = ObjectSpace.CreateObject<CashFlow>();
            cfOut1.CalculateEnabled = false;
            cfOut1.CounterCcy = ccyUSD;
            cfOut1.TranDate = new DateTime(2013, 12, 17);
            cfOut1.Account = account;
            cfOut1.Activity = activity;
            cfOut1.Counterparty = counterparty;
            cfOut1.AccountCcyAmt = -110;
            cfOut1.FunctionalCcyAmt = cfOut1.AccountCcyAmt / rate.ConversionRate;
            cfOut1.CounterCcyAmt = cfOut1.AccountCcyAmt;
            cfOut1.ForexSettleType = CashFlowForexSettleType.Out;
            cfOut1.Description = "cfout1";
            //cfOut1.BankStmts.Add(bsOut1);
            cfTable.Add(intparam[counter], cfOut1);
            counter++;

            var cfOut2 = ObjectSpace.CreateObject<CashFlow>();
            cfOut2.CalculateEnabled = false;
            cfOut2.CounterCcy = ccyUSD;
            cfOut2.TranDate = new DateTime(2013, 12, 17);
            cfOut2.Account = account;
            cfOut2.Activity = activity;
            cfOut2.Counterparty = counterparty;
            cfOut2.AccountCcyAmt = 105;
            cfOut2.FunctionalCcyAmt = cfOut2.AccountCcyAmt / rate.ConversionRate;
            cfOut2.CounterCcyAmt = cfOut2.AccountCcyAmt;
            cfOut2.ForexSettleType = CashFlowForexSettleType.OutReclass;
            cfOut2.Description = "cfOut2";
            //cfOut2.BankStmts.Add(bsOut2);
            cfTable.Add(intparam[counter], cfOut2);
            counter++;

            var cfOut3 = ObjectSpace.CreateObject<CashFlow>();
            cfOut3.CalculateEnabled = false;
            cfOut3.CounterCcy = ccyUSD;
            cfOut3.TranDate = new DateTime(2013, 12, 17);
            cfOut3.Account = account;
            cfOut3.Activity = activity;
            cfOut3.Counterparty = counterparty;
            cfOut3.AccountCcyAmt = -105;
            cfOut3.FunctionalCcyAmt = cfOut3.AccountCcyAmt / rate.ConversionRate;
            cfOut3.CounterCcyAmt = cfOut3.AccountCcyAmt;
            cfOut3.ForexSettleType = CashFlowForexSettleType.OutReclass;
            cfOut3.Description = "cfOut3";
            //cfOut3.BankStmts.Add(bsOut3);
            cfTable.Add(intparam[counter], cfOut3);
            counter++;

            var cfOut4 = ObjectSpace.CreateObject<CashFlow>();
            cfOut4.CalculateEnabled = false;
            cfOut4.CounterCcy = ccyUSD;
            cfOut4.TranDate = new DateTime(2013, 12, 17);
            cfOut4.Account = account;
            cfOut4.Activity = activity;
            cfOut4.Counterparty = counterparty;
            cfOut4.AccountCcyAmt = -30;
            cfOut4.FunctionalCcyAmt = cfOut4.AccountCcyAmt / rate.ConversionRate;
            cfOut4.CounterCcyAmt = cfOut4.AccountCcyAmt;
            cfOut4.ForexSettleType = CashFlowForexSettleType.Out;
            cfOut4.Description = "cfOut4";
            //cfOut4.BankStmts.Add(bsOut4);
            cfTable.Add(intparam[counter], cfOut4);
            counter++;

            var cfOut5 = ObjectSpace.CreateObject<CashFlow>();
            cfOut5.CalculateEnabled = false;
            cfOut5.CounterCcy = ccyUSD;
            cfOut5.TranDate = new DateTime(2013, 12, 17);
            cfOut5.Account = account;
            cfOut5.Activity = activity;
            cfOut5.Counterparty = counterparty;
            cfOut5.AccountCcyAmt = 45;
            cfOut5.FunctionalCcyAmt = cfOut5.AccountCcyAmt / rate.ConversionRate;
            cfOut5.CounterCcyAmt = cfOut5.AccountCcyAmt;
            cfOut5.ForexSettleType = CashFlowForexSettleType.OutReclass;
            cfOut5.Description = "cfOut5";
            //cfOut5.BankStmts.Add(bsOut5);
            cfTable.Add(intparam[counter], cfOut5);
            counter++;

            var cfOut6 = ObjectSpace.CreateObject<CashFlow>();
            cfOut6.CalculateEnabled = false;
            cfOut6.CounterCcy = ccyUSD;
            cfOut6.TranDate = new DateTime(2013, 12, 17);
            cfOut6.Account = account;
            cfOut6.Activity = activity;
            cfOut6.Counterparty = counterparty;
            cfOut6.AccountCcyAmt = -45;
            cfOut6.FunctionalCcyAmt = cfOut6.AccountCcyAmt / rate.ConversionRate;
            cfOut6.CounterCcyAmt = cfOut6.AccountCcyAmt;
            cfOut6.ForexSettleType = CashFlowForexSettleType.OutReclass;
            cfOut6.Description = "cfOut6";
            //cfOut6.BankStmts.Add(bsOut6);
            cfTable.Add(intparam[counter], cfOut6);
            counter++;

            var cfOut7 = ObjectSpace.CreateObject<CashFlow>();
            cfOut7.CalculateEnabled = false;
            cfOut7.CounterCcy = ccyUSD;
            cfOut7.TranDate = new DateTime(2013, 12, 17);
            cfOut7.Account = account;
            cfOut7.Activity = activity;
            cfOut7.Counterparty = counterparty;
            cfOut7.AccountCcyAmt = -25;
            cfOut7.FunctionalCcyAmt = cfOut7.AccountCcyAmt / rate.ConversionRate;
            cfOut7.CounterCcyAmt = cfOut7.AccountCcyAmt;
            cfOut7.ForexSettleType = CashFlowForexSettleType.Out;
            cfOut7.Description = "cfOut7";
            //cfOut7.BankStmts.Add(bsOut7);
            cfTable.Add(intparam[counter], cfOut7);
            counter++;

            var cfOut8 = ObjectSpace.CreateObject<CashFlow>();
            cfOut8.CalculateEnabled = false;
            cfOut8.CounterCcy = ccyUSD;
            cfOut8.TranDate = new DateTime(2013, 12, 17);
            cfOut8.Account = account;
            cfOut8.Activity = activity;
            cfOut8.Counterparty = counterparty;
            cfOut8.AccountCcyAmt = -19;
            cfOut8.FunctionalCcyAmt = cfOut8.AccountCcyAmt / rate.ConversionRate;
            cfOut8.CounterCcyAmt = cfOut8.AccountCcyAmt;
            cfOut8.ForexSettleType = CashFlowForexSettleType.Out;
            cfOut8.Description = "cfOut8";
            //cfOut8.BankStmts.Add(bsOut8);
            cfTable.Add(intparam[counter], cfOut8);
            counter++;

            #endregion

            #region Act - generate OIDs

            foreach (var pair in cfTable.OrderBy(c => c.Key))
            {
                var cf = pair.Value;
                cf.GenerateOid();
            }
            ObjectSpace.CommitChanges();

            #endregion

            #region Act - run algorithm

            var paramObj = ObjectSpace.CreateObject<ForexSettleFifoParam>();
            var algo = new ForexSettleFifoAlgorithm(ObjectSpace, paramObj);
            algo.Process();
            ObjectSpace.CommitChanges();

            #endregion

            #region Assert

            var fsls = ObjectSpace.GetObjects<ForexSettleLink>();

            Assert.AreEqual(184, fsls.Sum(x => x.AccountCcyAmt));
            #endregion
        }

        [TestCase(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 })]
        public void ForexSettleFifoIntegrated(int[] intparam)
        {
            Xafology.ExpressApp.Xpo.SequentialGuidBase.SequentialGuidBaseObject.IsSequential = true;
            Xafology.ExpressApp.Xpo.SequentialGuidBase.SequentialGuidBaseObject.OidInitializationMode = DevExpress.Persistent.BaseImpl.OidInitializationMode.OnSaving;

            #region Arrange Forex Objects

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

            #region Arrange Lookup Objects

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ USD";
            account.Currency = ccyUSD;

            var activity = ObjectSpace.CreateObject<Activity>();
            activity.Name = "AP Pymt";

            var counterparty = ObjectSpace.CreateObject<Counterparty>();
            counterparty.Name = "UNDEFINED";

            var stmtSource = ObjectSpace.CreateObject<CashFlowSource>();
            stmtSource.Name = "Stmt";

            #endregion

            #region Arrange Bank Stmt Objects

            var bsIn1 = ObjectSpace.CreateObject<BankStmt>();
            bsIn1.CalculateEnabled = false;
            bsIn1.CounterCcy = ccyUSD;
            bsIn1.TranDate = new DateTime(2013, 11, 16);
            bsIn1.Account = account;
            bsIn1.Activity = activity;
            bsIn1.Counterparty = counterparty;
            bsIn1.TranAmount = 100;
            bsIn1.FunctionalCcyAmt = bsIn1.TranAmount / 0.95M;
            bsIn1.ForexSettleType = CashFlowForexSettleType.In;
            bsIn1.SummaryDescription = "bsIn1";
            bsIn1.Save();

            var bsIn2 = ObjectSpace.CreateObject<BankStmt>();
            bsIn2.CalculateEnabled = false;
            bsIn2.CounterCcy = ccyUSD;
            bsIn2.TranDate = new DateTime(2013, 11, 30);
            bsIn2.Account = account;
            bsIn2.Activity = activity;
            bsIn2.Counterparty = counterparty;
            bsIn2.TranAmount = 50;
            bsIn2.FunctionalCcyAmt = bsIn2.TranAmount / 0.99M;
            bsIn2.ForexSettleType = CashFlowForexSettleType.In;
            bsIn2.SummaryDescription = "bsIn2";
            bsIn2.Save();

            var bsIn3 = ObjectSpace.CreateObject<BankStmt>();
            bsIn3.CalculateEnabled = false;
            bsIn3.CounterCcy = ccyUSD;
            bsIn3.TranDate = new DateTime(2013, 11, 30);
            bsIn3.Account = account;
            bsIn3.Activity = activity;
            bsIn3.Counterparty = counterparty;
            bsIn3.TranAmount = 30;
            bsIn3.FunctionalCcyAmt = bsIn3.TranAmount / 0.87M;
            bsIn3.ForexSettleType = CashFlowForexSettleType.In;
            bsIn3.SummaryDescription = "bsIn3";
            bsIn3.Save();

            var bsOut1 = ObjectSpace.CreateObject<BankStmt>();
            bsOut1.CalculateEnabled = false;
            bsOut1.CounterCcy = ccyUSD;
            bsOut1.TranDate = new DateTime(2013, 12, 17);
            bsOut1.Account = account;
            bsOut1.Activity = activity;
            bsOut1.Counterparty = counterparty;
            bsOut1.TranAmount = -110;
            bsOut1.ForexSettleType = CashFlowForexSettleType.Out;
            bsOut1.SummaryDescription = "bsOut1";
            bsOut1.Save();

            var bsOut2 = ObjectSpace.CreateObject<BankStmt>();
            bsOut2.CalculateEnabled = false;
            bsOut2.CounterCcy = ccyUSD;
            bsOut2.TranDate = new DateTime(2013, 12, 17);
            bsOut2.Account = account;
            bsOut2.Activity = activity;
            bsOut2.Counterparty = counterparty;
            bsOut2.TranAmount = 105;
            bsOut2.ForexSettleType = CashFlowForexSettleType.OutReclass;
            bsOut2.SummaryDescription = "bsOut2";
            bsOut2.Save();

            var bsOut3 = ObjectSpace.CreateObject<BankStmt>();
            bsOut3.CalculateEnabled = false;
            bsOut3.CounterCcy = ccyUSD;
            bsOut3.TranDate = new DateTime(2013, 12, 17);
            bsOut3.Account = account;
            bsOut3.Activity = activity;
            bsOut3.Counterparty = counterparty;
            bsOut3.TranAmount = -105;
            bsOut3.ForexSettleType = CashFlowForexSettleType.OutReclass;
            bsOut3.SummaryDescription = "bsOut3";
            bsOut3.Save();

            var bsOut4 = ObjectSpace.CreateObject<BankStmt>();
            bsOut4.CalculateEnabled = false;
            bsOut4.CounterCcy = ccyUSD;
            bsOut4.TranDate = new DateTime(2013, 12, 17);
            bsOut4.Account = account;
            bsOut4.Activity = activity;
            bsOut4.Counterparty = counterparty;
            bsOut4.TranAmount = -30;
            bsOut4.ForexSettleType = CashFlowForexSettleType.Out;
            bsOut4.SummaryDescription = "bsOut4";
            bsOut4.Save();

            var bsOut5 = ObjectSpace.CreateObject<BankStmt>();
            bsOut5.CalculateEnabled = false;
            bsOut5.CounterCcy = ccyUSD;
            bsOut5.TranDate = new DateTime(2013, 12, 17);
            bsOut5.Account = account;
            bsOut5.Activity = activity;
            bsOut5.Counterparty = counterparty;
            bsOut5.TranAmount = 45;
            bsOut5.ForexSettleType = CashFlowForexSettleType.Out;
            bsOut5.SummaryDescription = "bsOut5";
            bsOut5.Save();

            var bsOut6 = ObjectSpace.CreateObject<BankStmt>();
            bsOut6.CalculateEnabled = false;
            bsOut6.CounterCcy = ccyUSD;
            bsOut6.TranDate = new DateTime(2013, 12, 17);
            bsOut6.Account = account;
            bsOut6.Activity = activity;
            bsOut6.Counterparty = counterparty;
            bsOut6.TranAmount = -45;
            bsOut6.ForexSettleType = CashFlowForexSettleType.Out;
            bsOut6.SummaryDescription = "bsOut6";
            bsOut6.Save();

            var bsOut7 = ObjectSpace.CreateObject<BankStmt>();
            bsOut7.CalculateEnabled = false;
            bsOut7.CounterCcy = ccyUSD;
            bsOut7.TranDate = new DateTime(2013, 12, 17);
            bsOut7.Account = account;
            bsOut7.Activity = activity;
            bsOut7.Counterparty = counterparty;
            bsOut7.TranAmount = -25;
            bsOut7.ForexSettleType = CashFlowForexSettleType.Out;
            bsOut7.SummaryDescription = "bsOut7";
            bsOut7.Save();

            var bsOut8 = ObjectSpace.CreateObject<BankStmt>();
            bsOut8.CalculateEnabled = false;
            bsOut8.CounterCcy = ccyUSD;
            bsOut8.TranDate = new DateTime(2013, 12, 17);
            bsOut8.Account = account;
            bsOut8.Activity = activity;
            bsOut8.Counterparty = counterparty;
            bsOut8.TranAmount = -19;
            bsOut8.ForexSettleType = CashFlowForexSettleType.Out;
            bsOut8.SummaryDescription = "bsOut8";
            bsOut8.Save();

            #endregion

            #region Arrange Cash Flow Objects

            var cfTable = new Dictionary<int, CashFlow>();
            int counter = 0;

            var cfIn1 = ObjectSpace.CreateObject<CashFlow>();

            cfIn1.CalculateEnabled = false;
            cfIn1.CounterCcy = ccyUSD;
            cfIn1.TranDate = new DateTime(2013, 11, 16);
            cfIn1.Account = account;
            cfIn1.Activity = activity;
            cfIn1.Counterparty = counterparty;
            cfIn1.AccountCcyAmt = 100;
            cfIn1.FunctionalCcyAmt = cfIn1.AccountCcyAmt / 0.95M;
            cfIn1.CounterCcyAmt = cfIn1.AccountCcyAmt;
            cfIn1.ForexSettleType = CashFlowForexSettleType.In;
            cfIn1.Description = "cfIn1";
            cfIn1.Source = stmtSource;
            cfIn1.BankStmts.Add(bsIn1);
            cfTable.Add(intparam[counter], cfIn1);
            counter++;

            var cfIn2 = ObjectSpace.CreateObject<CashFlow>();
            cfIn2.CalculateEnabled = false;
            cfIn2.CounterCcy = ccyUSD;
            cfIn2.TranDate = new DateTime(2013, 11, 30);
            cfIn2.Account = account;
            cfIn2.Activity = activity;
            cfIn2.Counterparty = counterparty;
            cfIn2.AccountCcyAmt = 50;
            cfIn2.FunctionalCcyAmt = cfIn2.AccountCcyAmt / 0.99M;
            cfIn2.CounterCcyAmt = cfIn2.AccountCcyAmt;
            cfIn2.ForexSettleType = CashFlowForexSettleType.In;
            cfIn2.Description = "cfIn2";
            cfIn2.Source = stmtSource;
            cfIn2.BankStmts.Add(bsIn2);
            cfTable.Add(intparam[counter], cfIn2);
            counter++;

            var cfIn3 = ObjectSpace.CreateObject<CashFlow>();
            cfIn3.CalculateEnabled = false;
            cfIn3.CounterCcy = ccyUSD;
            cfIn3.TranDate = new DateTime(2013, 11, 30);
            cfIn3.Account = account;
            cfIn3.Activity = activity;
            cfIn3.Counterparty = counterparty;
            cfIn3.AccountCcyAmt = 30;
            cfIn3.FunctionalCcyAmt = cfIn3.AccountCcyAmt / 0.87M;
            cfIn3.CounterCcyAmt = cfIn3.AccountCcyAmt;
            cfIn3.ForexSettleType = CashFlowForexSettleType.In;
            cfIn3.Description = "cfIn3";
            cfIn3.Source = stmtSource;
            cfIn3.BankStmts.Add(bsIn3);
            cfTable.Add(intparam[counter], cfIn3);
            counter++;

            var cfOut1 = ObjectSpace.CreateObject<CashFlow>();
            cfOut1.CalculateEnabled = false;
            cfOut1.CounterCcy = ccyUSD;
            cfOut1.TranDate = new DateTime(2013, 12, 17);
            cfOut1.Account = account;
            cfOut1.Activity = activity;
            cfOut1.Counterparty = counterparty;
            cfOut1.AccountCcyAmt = -110;
            cfOut1.FunctionalCcyAmt = cfOut1.AccountCcyAmt / rate.ConversionRate;
            cfOut1.CounterCcyAmt = cfOut1.AccountCcyAmt;
            cfOut1.ForexSettleType = CashFlowForexSettleType.Out;
            cfOut1.Description = "cfout1";
            cfOut1.Source = stmtSource;
            cfOut1.BankStmts.Add(bsOut1);
            cfTable.Add(intparam[counter], cfOut1);
            counter++;

            var cfOut2 = ObjectSpace.CreateObject<CashFlow>();
            cfOut2.CalculateEnabled = false;
            cfOut2.CounterCcy = ccyUSD;
            cfOut2.TranDate = new DateTime(2013, 12, 17);
            cfOut2.Account = account;
            cfOut2.Activity = activity;
            cfOut2.Counterparty = counterparty;
            cfOut2.AccountCcyAmt = 105;
            cfOut2.FunctionalCcyAmt = cfOut2.AccountCcyAmt / rate.ConversionRate;
            cfOut2.CounterCcyAmt = cfOut2.AccountCcyAmt;
            cfOut2.ForexSettleType = CashFlowForexSettleType.OutReclass;
            cfOut2.Description = "cfOut2";
            cfOut2.Source = stmtSource;
            cfOut2.BankStmts.Add(bsOut2);
            cfTable.Add(intparam[counter], cfOut2);
            counter++;

            var cfOut3 = ObjectSpace.CreateObject<CashFlow>();
            cfOut3.CalculateEnabled = false;
            cfOut3.CounterCcy = ccyUSD;
            cfOut3.TranDate = new DateTime(2013, 12, 17);
            cfOut3.Account = account;
            cfOut3.Activity = activity;
            cfOut3.Counterparty = counterparty;
            cfOut3.AccountCcyAmt = -105;
            cfOut3.FunctionalCcyAmt = cfOut3.AccountCcyAmt / rate.ConversionRate;
            cfOut3.CounterCcyAmt = cfOut3.AccountCcyAmt;
            cfOut3.ForexSettleType = CashFlowForexSettleType.OutReclass;
            cfOut3.Description = "cfOut3";
            cfOut3.Source = stmtSource;
            cfOut3.BankStmts.Add(bsOut3);
            cfTable.Add(intparam[counter], cfOut3);
            counter++;

            var cfOut4 = ObjectSpace.CreateObject<CashFlow>();
            cfOut4.CalculateEnabled = false;
            cfOut4.CounterCcy = ccyUSD;
            cfOut4.TranDate = new DateTime(2013, 12, 17);
            cfOut4.Account = account;
            cfOut4.Activity = activity;
            cfOut4.Counterparty = counterparty;
            cfOut4.AccountCcyAmt = -30;
            cfOut4.FunctionalCcyAmt = cfOut4.AccountCcyAmt / rate.ConversionRate;
            cfOut4.CounterCcyAmt = cfOut4.AccountCcyAmt;
            cfOut4.ForexSettleType = CashFlowForexSettleType.Out;
            cfOut4.Description = "cfOut4";
            cfOut4.Source = stmtSource;
            cfOut4.BankStmts.Add(bsOut4);
            cfTable.Add(intparam[counter], cfOut4);
            counter++;

            var cfOut5 = ObjectSpace.CreateObject<CashFlow>();
            cfOut5.CalculateEnabled = false;
            cfOut5.CounterCcy = ccyUSD;
            cfOut5.TranDate = new DateTime(2013, 12, 17);
            cfOut5.Account = account;
            cfOut5.Activity = activity;
            cfOut5.Counterparty = counterparty;
            cfOut5.AccountCcyAmt = 45;
            cfOut5.FunctionalCcyAmt = cfOut5.AccountCcyAmt / rate.ConversionRate;
            cfOut5.CounterCcyAmt = cfOut5.AccountCcyAmt;
            cfOut5.ForexSettleType = CashFlowForexSettleType.OutReclass;
            cfOut5.Description = "cfOut5";
            cfOut5.BankStmts.Add(bsOut5);
            cfOut5.Source = stmtSource;
            cfTable.Add(intparam[counter], cfOut5);
            counter++;

            var cfOut6 = ObjectSpace.CreateObject<CashFlow>();
            cfOut6.CalculateEnabled = false;
            cfOut6.CounterCcy = ccyUSD;
            cfOut6.TranDate = new DateTime(2013, 12, 17);
            cfOut6.Account = account;
            cfOut6.Activity = activity;
            cfOut6.Counterparty = counterparty;
            cfOut6.AccountCcyAmt = -45;
            cfOut6.FunctionalCcyAmt = cfOut6.AccountCcyAmt / rate.ConversionRate;
            cfOut6.CounterCcyAmt = cfOut6.AccountCcyAmt;
            cfOut6.ForexSettleType = CashFlowForexSettleType.OutReclass;
            cfOut6.Description = "cfOut6";
            cfOut6.Source = stmtSource;
            cfOut6.BankStmts.Add(bsOut6);
            cfTable.Add(intparam[counter], cfOut6);
            counter++;

            var cfOut7 = ObjectSpace.CreateObject<CashFlow>();
            cfOut7.CalculateEnabled = false;
            cfOut7.CounterCcy = ccyUSD;
            cfOut7.TranDate = new DateTime(2013, 12, 17);
            cfOut7.Account = account;
            cfOut7.Activity = activity;
            cfOut7.Counterparty = counterparty;
            cfOut7.AccountCcyAmt = -25;
            cfOut7.FunctionalCcyAmt = cfOut7.AccountCcyAmt / rate.ConversionRate;
            cfOut7.CounterCcyAmt = cfOut7.AccountCcyAmt;
            cfOut7.ForexSettleType = CashFlowForexSettleType.Out;
            cfOut7.Description = "cfOut7";
            cfOut7.Source = stmtSource;
            cfOut7.BankStmts.Add(bsOut7);
            cfTable.Add(intparam[counter], cfOut7);
            counter++;

            var cfOut8 = ObjectSpace.CreateObject<CashFlow>();
            cfOut8.CalculateEnabled = false;
            cfOut8.CounterCcy = ccyUSD;
            cfOut8.TranDate = new DateTime(2013, 12, 17);
            cfOut8.Account = account;
            cfOut8.Activity = activity;
            cfOut8.Counterparty = counterparty;
            cfOut8.AccountCcyAmt = -19;
            cfOut8.FunctionalCcyAmt = cfOut8.AccountCcyAmt / rate.ConversionRate;
            cfOut8.CounterCcyAmt = cfOut8.AccountCcyAmt;
            cfOut8.ForexSettleType = CashFlowForexSettleType.Out;
            cfOut8.Description = "cfOut8";
            cfOut8.Source = stmtSource;
            cfOut8.BankStmts.Add(bsOut8);
            cfTable.Add(intparam[counter], cfOut8);
            counter++;

            #endregion

            #region Act - generate OIDs

            foreach (var pair in cfTable.OrderBy(c => c.Key))
            {
                var cf = pair.Value;
                cf.GenerateOid();
            }
            ObjectSpace.CommitChanges();

            #endregion

            #region Act - run FIFO algorithm

            var paramObj = ObjectSpace.CreateObject<ForexSettleFifoParam>();
            var algo = new ForexSettleFifoAlgorithm(ObjectSpace, paramObj);
            algo.LinkCashFlows();
            ObjectSpace.CommitChanges();

            #endregion

            #region Assert ForexSettleLink

            // assert that all cash flows are linked
            var fsls = ObjectSpace.GetObjects<ForexSettleLink>();
            Assert.AreEqual(180, fsls.Sum(x => x.AccountCcyAmt));

            #endregion

            #region Act - Revalue Cash Outflows

            algo.RevalueOutflows();
            ObjectSpace.CommitChanges();

            #endregion

            #region Assert Cash Outflow revaluation

            ObjectSpace.Refresh();
            cfIn1 = ObjectSpace.GetObjectByKey<CashFlow>(cfIn1.Oid);
            cfIn2 = ObjectSpace.GetObjectByKey<CashFlow>(cfIn2.Oid);
            cfIn3 = ObjectSpace.GetObjectByKey<CashFlow>(cfIn3.Oid);
            cfOut1 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut1.Oid);
            cfOut2 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut2.Oid);
            cfOut3 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut3.Oid);
            cfOut4 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut4.Oid);
            cfOut5 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut5.Oid);
            cfOut6 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut6.Oid);
            cfOut7 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut7.Oid);
            cfOut8 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut8.Oid);

            Assert.AreEqual(
                -Math.Round(cfIn1.FunctionalCcyAmt + 10 / cfIn2.AccountCcyAmt * cfIn2.FunctionalCcyAmt, 2),
                Math.Round(cfOut1.FunctionalCcyAmt,2));

            Assert.AreEqual(
                -Math.Round(30 / cfIn2.AccountCcyAmt * cfIn2.FunctionalCcyAmt, 2),
                Math.Round(cfOut4.FunctionalCcyAmt,2));

            Assert.AreEqual(
                -Math.Round(10 / cfIn2.AccountCcyAmt * cfIn2.FunctionalCcyAmt
                + 15 / cfIn3.AccountCcyAmt * cfIn3.FunctionalCcyAmt, 2),
                Math.Round(cfOut7.FunctionalCcyAmt, 2));

            // assume rate of last cfIn is used
            Assert.AreEqual(
                Math.Round(cfOut8.AccountCcyAmt * cfIn3.FunctionalCcyAmt / cfIn3.AccountCcyAmt, 2),
                Math.Round(cfOut8.FunctionalCcyAmt, 2));

            #endregion

            #region Act - Insert new Cash Inflow to fund excess outflow

            ccyUSD = ObjectSpace.GetObjectByKey<Currency>(ccyUSD.Oid);
            account = ObjectSpace.GetObjectByKey<Account>(account.Oid);
            activity = ObjectSpace.GetObjectByKey<Activity>(activity.Oid);
            counterparty = ObjectSpace.GetObjectByKey<Counterparty>(counterparty.Oid);

            // additional cash flow, not linked to bank stmt
            var cfIn4 = ObjectSpace.CreateObject<CashFlow>();
            cfIn4.CalculateEnabled = false;
            cfIn4.CounterCcy = ccyUSD;
            cfIn4.TranDate = new DateTime(2013, 12, 18);
            cfIn4.Account = account;
            cfIn4.Activity = activity;
            cfIn4.Counterparty = counterparty;
            cfIn4.AccountCcyAmt = 10;
            cfIn4.FunctionalCcyAmt = cfIn4.AccountCcyAmt / 0.6M;
            cfIn4.CounterCcyAmt = cfIn4.AccountCcyAmt;
            cfIn4.ForexSettleType = CashFlowForexSettleType.In;
            cfIn4.Description = "cfIn4";
            ObjectSpace.CommitChanges();

            algo.Process();

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

            #endregion

            #region Assert that new cash flow is linked

            fsls = ObjectSpace.GetObjects<ForexSettleLink>();
            Assert.AreEqual(184, fsls.Sum(x => x.AccountCcyAmt));

            #endregion

            #region Assert revaluation of outflow uses the new inflow

            cfIn1 = ObjectSpace.GetObjectByKey<CashFlow>(cfIn1.Oid);
            cfIn2 = ObjectSpace.GetObjectByKey<CashFlow>(cfIn2.Oid);
            cfIn3 = ObjectSpace.GetObjectByKey<CashFlow>(cfIn3.Oid);
            cfOut1 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut1.Oid);
            cfOut2 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut2.Oid);
            cfOut3 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut3.Oid);
            cfOut4 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut4.Oid);
            cfOut5 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut5.Oid);
            cfOut6 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut6.Oid);
            cfOut7 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut7.Oid);
            cfOut8 = ObjectSpace.GetObjectByKey<CashFlow>(cfOut8.Oid);

            Assert.AreEqual(
                -Math.Round(15 / cfIn3.AccountCcyAmt * cfIn3.FunctionalCcyAmt
                    + 4 / cfIn4.AccountCcyAmt * cfIn4.FunctionalCcyAmt , 2),
                Math.Round(cfOut8.FunctionalCcyAmt, 2));

            #endregion

            #region Assert Bank Statement values equal Cash Flow

            bsIn1 = ObjectSpace.GetObjectByKey<BankStmt>(bsIn1.Oid);
            bsIn2 = ObjectSpace.GetObjectByKey<BankStmt>(bsIn2.Oid);
            bsIn3 = ObjectSpace.GetObjectByKey<BankStmt>(bsIn3.Oid);
            bsOut1 = ObjectSpace.GetObjectByKey<BankStmt>(bsOut1.Oid);
            bsOut2 = ObjectSpace.GetObjectByKey<BankStmt>(bsOut2.Oid);
            bsOut3 = ObjectSpace.GetObjectByKey<BankStmt>(bsOut3.Oid);
            bsOut4 = ObjectSpace.GetObjectByKey<BankStmt>(bsOut4.Oid);
            bsOut5 = ObjectSpace.GetObjectByKey<BankStmt>(bsOut5.Oid);
            bsOut6 = ObjectSpace.GetObjectByKey<BankStmt>(bsOut6.Oid);
            bsOut7 = ObjectSpace.GetObjectByKey<BankStmt>(bsOut7.Oid);
            bsOut8 = ObjectSpace.GetObjectByKey<BankStmt>(bsOut8.Oid);

            Assert.AreEqual(cfOut1.FunctionalCcyAmt, bsOut1.FunctionalCcyAmt);
            Assert.AreEqual(cfOut2.FunctionalCcyAmt, bsOut2.FunctionalCcyAmt);
            Assert.AreEqual(cfOut3.FunctionalCcyAmt, bsOut3.FunctionalCcyAmt);
            Assert.AreEqual(cfOut4.FunctionalCcyAmt, bsOut4.FunctionalCcyAmt);
            Assert.AreEqual(cfOut5.FunctionalCcyAmt, bsOut5.FunctionalCcyAmt);
            Assert.AreEqual(cfOut6.FunctionalCcyAmt, bsOut6.FunctionalCcyAmt);
            Assert.AreEqual(cfOut7.FunctionalCcyAmt, bsOut7.FunctionalCcyAmt);
            Assert.AreEqual(cfOut8.FunctionalCcyAmt, bsOut8.FunctionalCcyAmt);

            #endregion

            #region Assert revaluation of outflow reclasses
            var cashFlows = ObjectSpace.GetObjects<CashFlow>();
            var accountAmt = cashFlows
                .Where(x => x.TranDate == cfOut2.TranDate).Sum(x => x.AccountCcyAmt);
            var funcAmt = cashFlows
                .Where(x => x.TranDate == cfOut2.TranDate).Sum(x => x.FunctionalCcyAmt);

            Assert.AreEqual(
                Math.Round(cfOut2.AccountCcyAmt * funcAmt / accountAmt, 2), cfOut2.FunctionalCcyAmt);
            Assert.AreEqual(
                Math.Round(cfOut3.AccountCcyAmt * funcAmt / accountAmt, 2), cfOut3.FunctionalCcyAmt);

            #endregion
        }

        public override void OnAddExportedTypes(ModuleBase module)
        {
            CashDisciplineTestHelper.AddExportedTypes(module);
        }
    }
}
