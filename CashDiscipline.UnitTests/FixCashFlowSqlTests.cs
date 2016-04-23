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
using CashDiscipline.UnitTests.TestObjects;
using CashDiscipline.Module.Logic.Cash;
using DevExpress.Xpo.DB;
using System.Data.SqlClient;

namespace CashDiscipline.UnitTests
{
    [TestFixture]
    public class FixCashFlowSqlTests : TestBase
    {
        #region Setup

        public FixCashFlowSqlTests()
        {
            SetTesterDbType(TesterDbType.MsSql);

            var tester = Tester as MSSqlDbTestBase;
            if (tester != null)
                tester.DatabaseName = Constants.TestDbName;
        }


        public override void OnSetup()
        {
            CashDiscipline.Module.DatabaseUpdate.Updater.CreateCurrencies(ObjectSpace);
            SetOfBooks.GetInstance(ObjectSpace);
            CashDiscipline.Module.DatabaseUpdate.Updater.InitSetOfBooks(ObjectSpace);
            CashDiscipline.Module.DatabaseUpdate.Updater.CreateCashFlowDefaults(ObjectSpace);
        }

        public override void OnAddExportedTypes(ModuleBase module)
        {
            CashDisciplineTestHelper.AddExportedTypes(module);
        }

        #endregion

        [Test]
        public void FixFromDifferentCurrency()
        {
            #region Arrange Dimensions

            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));

            var audAccount = ObjectSpace.CreateObject<Account>();
            audAccount.Name = "VHA ANZ AUD";
            audAccount.Currency = ccyAUD;

            var usdAccount = ObjectSpace.CreateObject<Account>();
            usdAccount.Name = "VHA ANZ USD";
            usdAccount.Currency = ccyUSD;

            var rate = ObjectSpace.CreateObject<ForexRate>();
            rate.FromCurrency = ccyAUD;
            rate.ToCurrency = ccyUSD;
            rate.ConversionDate = new DateTime(2013, 12, 31);
            rate.ConversionRate = 0.9M;

            var activity = ObjectSpace.CreateObject<Activity>();
            activity.Name = "Tech Cost";
            activity.FixActivity = activity;

            var fixActivity = ObjectSpace.CreateObject<Activity>();
            fixActivity.Name = "AP Pymt";

            ObjectSpace.CommitChanges();

            #endregion

            #region Arrange Fix Tags

            var schedOutFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            schedOutFixTag.Name = "S2";
            schedOutFixTag.FixTagType = CashForecastFixTagType.ScheduleOut;

            var allocFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            allocFixTag.Name = "B3";
            allocFixTag.FixTagType = CashForecastFixTagType.Allocate;

            var reversalFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            reversalFixTag.Name = CashDiscipline.Module.Constants.ReversalFixTag;
            reversalFixTag.FixTagType = CashForecastFixTagType.Ignore;

            var revRecFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            revRecFixTag.Name = CashDiscipline.Module.Constants.RevRecFixTag;
            revRecFixTag.FixTagType = CashForecastFixTagType.Ignore;

            var resRevRecFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            resRevRecFixTag.Name = CashDiscipline.Module.Constants.ResRevRecFixTag;
            resRevRecFixTag.FixTagType = CashForecastFixTagType.Ignore;

            #endregion

            #region Arrange Transactions

            // act
            var cfFixee1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixee1.TranDate = new DateTime(2016, 03, 11);
            cfFixee1.Account = usdAccount;
            cfFixee1.CounterCcy = ccyUSD;
            cfFixee1.AccountCcyAmt = -600;
            cfFixee1.CounterCcyAmt = -600;
            cfFixee1.Activity = activity;
            cfFixee1.FixRank = 2;
            cfFixee1.Fix = schedOutFixTag;
            cfFixee1.DateUnFix = cfFixee1.TranDate;
            cfFixee1.FixActivity = cfFixee1.Activity;

            var fixerDate = new DateTime(2016, 03, 25);
            var cfFixer1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixer1.TranDate = fixerDate;
            cfFixer1.Account = audAccount;
            cfFixer1.CounterCcy = ccyAUD;
            cfFixer1.CounterCcyAmt = -500;
            cfFixer1.AccountCcyAmt = -500;
            cfFixer1.Activity = activity;
            cfFixer1.FixRank = 3;
            cfFixer1.Fix = allocFixTag;
            cfFixer1.FixFromDate = new DateTime(2016, 03, 01);
            cfFixer1.FixToDate = new DateTime(2016, 03, 31);
            cfFixer1.DateUnFix = cfFixer1.TranDate;
            cfFixer1.FixActivity = cfFixer1.Activity;

            ObjectSpace.CommitChanges();

            #endregion

            #region Arrange Algorithm

            var paramObj = ObjectSpace.CreateObject<CashFlowFixParam>();
            paramObj.FromDate = new DateTime(2016, 01, 01);
            paramObj.ToDate = new DateTime(2016, 12, 31);
            paramObj.ApayableLockdownDate = new DateTime(2016, 03, 18);
            paramObj.ApayableNextLockdownDate = new DateTime(2016, 03, 25);
            paramObj.ApReclassActivity = fixActivity;
            paramObj.PayrollLockdownDate = new DateTime(2016, 03, 18);
            paramObj.PayrollNextLockdownDate = new DateTime(2016, 03, 25);

            var fixAlgo = new FixCashFlowsAlgorithm(ObjectSpace, paramObj);

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            //Assert.AreEqual(2, fixAlgo.GetCashFlowsToFix().Count()); // assert test data

            fixAlgo.ProcessCashFlows();

            #endregion

            #region Assert

            cashFlows = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(166.67, Math.Round(cashFlows
                .Where(cf => cf.TranDate == fixerDate)
                .Sum(cf => cf.CounterCcyAmt), 2));

            Assert.AreEqual(166.67, Math.Round(cashFlows
                     .Where(cf => cf.TranDate == fixerDate)
                     .Sum(cf => cf.FunctionalCcyAmt), 2));

            #endregion
        }

        [Test]
        public void IfDeleteFixerCashFlow()
        {
            #region Arrange Dimensions

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

            var activity = ObjectSpace.CreateObject<Activity>();
            activity.Name = "Device Purchase";
            activity.FixActivity = activity;

            var fixActivity = ObjectSpace.CreateObject<Activity>();
            fixActivity.Name = "AP Pymt";

            var counterparty = ObjectSpace.CreateObject<Counterparty>();
            counterparty.Name = "UNDEFINED";

            ObjectSpace.CommitChanges();

            #endregion

            #region Arrange Fix Tags

            var schedOutFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            schedOutFixTag.Name = "S";
            schedOutFixTag.FixTagType = CashForecastFixTagType.ScheduleOut;

            CashDiscipline.Module.DatabaseUpdate.Updater.InitFixTags(ObjectSpace);

            #endregion

            #region Arrange Transactions

            // act
            var cfFixee1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixee1.TranDate = new DateTime(2016, 03, 11);
            cfFixee1.Account = account;
            cfFixee1.AccountCcyAmt = 600;
            cfFixee1.Activity = activity;
            cfFixee1.FixRank = 2;
            cfFixee1.Fix = schedOutFixTag;
            cfFixee1.DateUnFix = cfFixee1.TranDate;
            cfFixee1.FixActivity = cfFixee1.Activity;
            cfFixee1.Counterparty = counterparty;

            var cfFixer1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixer1.TranDate = new DateTime(2016, 03, 12);
            cfFixer1.Account = account;
            cfFixer1.AccountCcyAmt = 500;
            cfFixer1.Activity = activity;
            cfFixer1.FixRank = 3;
            cfFixer1.Fix = schedOutFixTag;
            cfFixer1.FixFromDate = new DateTime(2016, 03, 01);
            cfFixer1.FixToDate = new DateTime(2016, 03, 31);
            cfFixer1.DateUnFix = cfFixer1.TranDate;
            cfFixer1.FixActivity = cfFixer1.Activity;
            cfFixer1.Counterparty = counterparty;

            ObjectSpace.CommitChanges();

            #endregion

            #region Arrange Algorithm

            var paramObj = ObjectSpace.CreateObject<CashFlowFixParam>();
            paramObj.FromDate = new DateTime(2016, 01, 01);
            paramObj.ToDate = new DateTime(2016, 12, 31);
            paramObj.ApayableLockdownDate = new DateTime(2016, 03, 18);
            paramObj.ApayableNextLockdownDate = new DateTime(2016, 03, 25);
            paramObj.ApReclassActivity = fixActivity;
            paramObj.PayrollLockdownDate = new DateTime(2016, 03, 18);
            paramObj.PayrollNextLockdownDate = new DateTime(2016, 03, 25);

            var fixAlgo = new SqlFixCashFlowsAlgorithm(ObjectSpace, paramObj);

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(2, fixAlgo.GetCashFlowsToFix().Count()); // assert test data
            Assert.AreEqual(500, cashFlows
                 .Where(cf => cf.TranDate == new DateTime(2016, 03, 12))
                 .Sum(cf => cf.AccountCcyAmt));

            fixAlgo.ProcessCashFlows();

            #endregion

            #region Assert Arrange

            cashFlows = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(-100, cashFlows
                .Where(cf => cf.TranDate == paramObj.ApayableNextLockdownDate)
                .Sum(cf => cf.AccountCcyAmt));

            Assert.AreEqual(0, cashFlows
                .Where(cf => cf.TranDate == new DateTime(2016, 03, 12))
                .Sum(cf => cf.AccountCcyAmt));

            Assert.AreEqual(3, cashFlows.Count);
            #endregion

            #region Assert Delete Cash Flow

            cfFixer1.Delete();
            ObjectSpace.CommitChanges();

            fixAlgo.ProcessCashFlows();
            cashFlows = ObjectSpace.GetObjects<CashFlow>();
            Assert.AreEqual(1, cashFlows.Count);

            #endregion
        }

        [Test]
        public void FixPrioritizesMatchingCurrency()
        {
            #region Arrange Dimensions

            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));

            var audAccount = ObjectSpace.CreateObject<Account>();
            audAccount.Name = "VHA ANZ AUD";
            audAccount.Currency = ccyAUD;

            var usdAccount = ObjectSpace.CreateObject<Account>();
            usdAccount.Name = "VHA ANZ USD";
            usdAccount.Currency = ccyUSD;

            var rate = ObjectSpace.CreateObject<ForexRate>();
            rate.FromCurrency = ccyAUD;
            rate.ToCurrency = ccyUSD;
            rate.ConversionDate = new DateTime(2013, 12, 31);
            rate.ConversionRate = 0.9M;

            var activity = ObjectSpace.CreateObject<Activity>();
            activity.Name = "Tech Cost";
            activity.FixActivity = activity;

            var fixActivity = ObjectSpace.CreateObject<Activity>();
            fixActivity.Name = "AP Pymt";

            ObjectSpace.CommitChanges();

            #endregion

            #region Arrange Fix Tags

            var schedOutFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            schedOutFixTag.Name = "S2";
            schedOutFixTag.FixTagType = CashForecastFixTagType.ScheduleOut;

            var allocFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            allocFixTag.Name = "B3";
            allocFixTag.FixTagType = CashForecastFixTagType.Allocate;

            CashDiscipline.Module.DatabaseUpdate.Updater.InitFixTags(ObjectSpace);

            #endregion

            #region Arrange Transactions

            // act
            var cfFixee1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixee1.TranDate = new DateTime(2016, 03, 11);
            cfFixee1.Account = usdAccount;
            cfFixee1.CounterCcy = ccyUSD;
            cfFixee1.AccountCcyAmt = -600;
            cfFixee1.CounterCcyAmt = -600;
            cfFixee1.Activity = activity;
            cfFixee1.FixRank = 2;
            cfFixee1.Fix = schedOutFixTag;
            cfFixee1.DateUnFix = cfFixee1.TranDate;
            cfFixee1.FixActivity = cfFixee1.Activity;

            var cfFixer1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixer1.TranDate = new DateTime(2016, 03, 25);
            cfFixer1.Account = audAccount;
            cfFixer1.CounterCcy = ccyAUD;
            cfFixer1.CounterCcyAmt = -500;
            cfFixer1.AccountCcyAmt = -500;
            cfFixer1.Activity = activity;
            cfFixer1.FixRank = 3;
            cfFixer1.Fix = allocFixTag;
            cfFixer1.FixFromDate = new DateTime(2016, 03, 01);
            cfFixer1.FixToDate = new DateTime(2016, 03, 31);
            cfFixer1.DateUnFix = cfFixer1.TranDate;
            cfFixer1.FixActivity = cfFixer1.Activity;

            var cfFixer2 = ObjectSpace.CreateObject<CashFlow>();
            cfFixer2.TranDate = new DateTime(2016, 03, 26);
            cfFixer2.Account = usdAccount;
            cfFixer2.CounterCcy = ccyUSD;
            cfFixer2.CounterCcyAmt = -500;
            cfFixer2.AccountCcyAmt = -500;
            cfFixer2.Activity = activity;
            cfFixer2.FixRank = 3;
            cfFixer2.Fix = allocFixTag;
            cfFixer2.FixFromDate = new DateTime(2016, 03, 01);
            cfFixer2.FixToDate = new DateTime(2016, 03, 31);
            cfFixer2.DateUnFix = cfFixer2.TranDate;
            cfFixer2.FixActivity = cfFixer2.Activity;

            ObjectSpace.CommitChanges();

            #endregion

            #region Arrange Algorithm

            var paramObj = ObjectSpace.CreateObject<CashFlowFixParam>();
            paramObj.FromDate = new DateTime(2016, 01, 01);
            paramObj.ToDate = new DateTime(2016, 12, 31);
            paramObj.ApayableLockdownDate = new DateTime(2016, 03, 18);
            paramObj.ApayableNextLockdownDate = new DateTime(2016, 03, 25);
            paramObj.ApReclassActivity = fixActivity;
            paramObj.PayrollLockdownDate = new DateTime(2016, 03, 18);
            paramObj.PayrollNextLockdownDate = new DateTime(2016, 03, 25);
            paramObj.Snapshot = SetOfBooks.GetInstance(ObjectSpace).CurrentCashFlowSnapshot;
            paramObj.Save();
            ObjectSpace.CommitChanges();

            var fixAlgo = new SqlFixCashFlowsAlgorithm(ObjectSpace, paramObj);
            fixAlgo.ProcessCashFlows();

            #endregion

            #region Assert


            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(100, cashFlows
                .Where(cf => cf.TranDate == new DateTime(2016, 03, 26)
                && cf.CounterCcy == ccyUSD).Sum(cf => cf.CounterCcyAmt));

            Assert.AreEqual(-500, cashFlows
                .Where(cf => cf.TranDate == new DateTime(2016, 03, 25)
                && cf.CounterCcy == ccyAUD).Sum(cf => cf.CounterCcyAmt));

            #endregion
        }

    }
}
