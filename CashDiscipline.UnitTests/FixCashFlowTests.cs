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
using CashDiscipline.Module.ControllerHelpers.Cash;


namespace CashDiscipline.UnitTests
{
    [TestFixture]
    public class FixCashFlowTests : TestBase
    {
        #region Set up
        public FixCashFlowTests()
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
        }

        public override void OnAddExportedTypes(ModuleBase module)
        {
            CashDisciplineTestHelper.AddExportedTypes(module);
        }
        #endregion

        [Test]
        [Category("Fix Cash Flow")]
        public void FixSingleSchedOut()
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

            var fix1 = ObjectSpace.CreateObject<CashForecastFixTag>();
            fix1.Name = "S2";
            fix1.FixTagType = CashForecastFixTagType.ScheduleOut;

            var fixActivity = ObjectSpace.CreateObject<Activity>();
            fixActivity.Name = "AP Pymt";

            ObjectSpace.CommitChanges();

            #endregion

            #region Arrange Transactions

            // act
            var cf1 = ObjectSpace.CreateObject<CashFlow>();
            cf1.TranDate = new DateTime(2016, 03, 03);
            cf1.Account = account;
            cf1.AccountCcyAmt = 1000;
            cf1.Activity = activity;
            cf1.FixRank = 2;
            cf1.Fix = fix1;
            cf1.DateUnFix = cf1.TranDate;
            cf1.FixActivity = cf1.Activity;

            var cf2 = ObjectSpace.CreateObject<CashFlow>();
            cf2.TranDate = new DateTime(2016, 03, 31);
            cf2.Account = account;
            cf2.AccountCcyAmt = 1400;
            cf2.Activity = activity;
            cf2.FixRank = 3;
            cf2.FixFromDate = new DateTime(2016, 03, 1);
            cf2.FixToDate = new DateTime(2016, 03, 31);
            cf2.Fix = fix1;
            cf2.DateUnFix = cf2.TranDate;
            cf2.FixActivity = cf2.Activity;

            ObjectSpace.CommitChanges();

            #endregion

            #region Act

            var paramObj = ObjectSpace.CreateObject<CashFlowFixParam>();
            paramObj.FromDate = new DateTime(2016, 01, 01);
            paramObj.ToDate = new DateTime(2016, 12, 31);
            paramObj.ApayableLockdownDate = new DateTime(2016, 03, 18);
            paramObj.ApayableNextLockdownDate = new DateTime(2016, 03, 25);
            paramObj.ApReclassActivity = fixActivity;
            paramObj.PayrollLockdownDate = new DateTime(2016, 03, 18);
            paramObj.PayrollNextLockdownDate = new DateTime(2016, 03, 25);

            var fixAlgo = new FixCashFlowsAlgorithm2(ObjectSpace, paramObj);

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            fixAlgo.ProcessCashFlows();

            #endregion

            #region Assert

            Assert.AreEqual(3, cashFlows.Count());

            Assert.AreEqual(1400 - 1000,
                    cashFlows.Where(x =>
                        x.TranDate == new DateTime(2016, 03, 31))
                    .Sum(x => x.AccountCcyAmt));

            #endregion

        }

        [Test]
        [Category("Fix Cash Flow")]
        public void FixSchedOuts()
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

            var fix1 = ObjectSpace.CreateObject<CashForecastFixTag>();
            fix1.Name = "S2";
            fix1.FixTagType = CashForecastFixTagType.ScheduleOut;

            var fixActivity = ObjectSpace.CreateObject<Activity>();
            fixActivity.Name = "AP Pymt";

            ObjectSpace.CommitChanges();

            #endregion

            #region Arrange Transactions

            // act
            var cfFixee1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixee1.TranDate = new DateTime(2016, 03, 03);
            cfFixee1.Account = account;
            cfFixee1.AccountCcyAmt = 600;
            cfFixee1.Activity = activity;
            cfFixee1.FixRank = 2;
            cfFixee1.Fix = fix1;
            cfFixee1.DateUnFix = cfFixee1.TranDate;
            cfFixee1.FixActivity = cfFixee1.Activity;

            var cfFixee2 = ObjectSpace.CreateObject<CashFlow>();
            cfFixee2.TranDate = new DateTime(2016, 03, 03);
            cfFixee2.Account = account;
            cfFixee2.AccountCcyAmt = 500;
            cfFixee2.Activity = activity;
            cfFixee2.FixRank = 2;
            cfFixee2.Fix = fix1;
            cfFixee2.DateUnFix = cfFixee2.TranDate;
            cfFixee2.FixActivity = cfFixee2.Activity;

            var cfFixer1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixer1.TranDate = new DateTime(2016, 03, 31);
            cfFixer1.Account = account;
            cfFixer1.AccountCcyAmt = 1400;
            cfFixer1.Activity = activity;
            cfFixer1.FixRank = 3;
            cfFixer1.FixFromDate = new DateTime(2016, 03, 1);
            cfFixer1.FixToDate = new DateTime(2016, 03, 31);
            cfFixer1.Fix = fix1;
            cfFixer1.DateUnFix = cfFixer1.TranDate;
            cfFixer1.FixActivity = cfFixer1.Activity;

            ObjectSpace.CommitChanges();

            #endregion

            #region Act

            var paramObj = ObjectSpace.CreateObject<CashFlowFixParam>();
            paramObj.FromDate = new DateTime(2016, 01, 01);
            paramObj.ToDate = new DateTime(2016, 12, 31);
            paramObj.ApayableLockdownDate = new DateTime(2016, 03, 18);
            paramObj.ApayableNextLockdownDate = new DateTime(2016, 03, 25);
            paramObj.ApReclassActivity = fixActivity;
            paramObj.PayrollLockdownDate = new DateTime(2016, 03, 18);
            paramObj.PayrollNextLockdownDate = new DateTime(2016, 03, 25);

            var fixAlgo = new FixCashFlowsAlgorithm2(ObjectSpace, paramObj);

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            fixAlgo.ProcessCashFlows();

            #endregion

            #region Assert

            Assert.AreEqual(5, cashFlows.Count());

            Assert.AreEqual(300,
                    cashFlows.Where(x =>
                        x.TranDate == new DateTime(2016, 03, 31))
                    .Sum(x => x.AccountCcyAmt));

            #endregion
        }

        // This test will fail if the right fixes have not been deleted or created
        [Test]
        [Category("Fix Cash Flow")]
        public void FixSchedOutsTwice()
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

            ObjectSpace.CommitChanges();

            #endregion

            #region Arrange Fix Tags

            var schedOutFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            schedOutFixTag.Name = "S2";
            schedOutFixTag.FixTagType = CashForecastFixTagType.ScheduleOut;

            var reversalFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            reversalFixTag.Name = "R";
            reversalFixTag.FixTagType = CashForecastFixTagType.Ignore;

            #endregion

            #region Arrange Transactions

            // act
            var cfFixee1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixee1.TranDate = new DateTime(2016, 03, 03);
            cfFixee1.Account = account;
            cfFixee1.AccountCcyAmt = 600;
            cfFixee1.Activity = activity;
            cfFixee1.FixRank = 2;
            cfFixee1.Fix = schedOutFixTag;
            cfFixee1.DateUnFix = cfFixee1.TranDate;
            cfFixee1.FixActivity = cfFixee1.Activity;

            var cfFixee2 = ObjectSpace.CreateObject<CashFlow>();
            cfFixee2.TranDate = new DateTime(2016, 03, 03);
            cfFixee2.Account = account;
            cfFixee2.AccountCcyAmt = 500;
            cfFixee2.Activity = activity;
            cfFixee2.FixRank = 2;
            cfFixee2.Fix = schedOutFixTag;
            cfFixee2.DateUnFix = cfFixee2.TranDate;
            cfFixee2.FixActivity = cfFixee2.Activity;

            var cfFixer1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixer1.TranDate = new DateTime(2016, 03, 31);
            cfFixer1.Account = account;
            cfFixer1.AccountCcyAmt = 1400;
            cfFixer1.Activity = activity;
            cfFixer1.FixRank = 3;
            cfFixer1.FixFromDate = new DateTime(2016, 03, 1);
            cfFixer1.FixToDate = new DateTime(2016, 03, 31);
            cfFixer1.Fix = schedOutFixTag;
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

            var fixAlgo = new FixCashFlowsAlgorithm2(ObjectSpace, paramObj);

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(3, fixAlgo.GetCashFlowsToFix().Count()); // assert test data

            #endregion

            #region Assert 1st run

            fixAlgo.ProcessCashFlows();

            CashFlowSnapshot currentSnapshot = CashFlowHelper.GetCurrentSnapshot(ObjectSpace.Session);

            #endregion

            #region Assert 2nd run

            fixAlgo.ProcessCashFlows();

            Assert.AreEqual(5, cashFlows.Count());

            Assert.AreEqual(300,
                    cashFlows.Where(x =>
                        x.TranDate == new DateTime(2016, 03, 31))
                    .Sum(x => x.AccountCcyAmt));

            #endregion
        }

        [Test]
        [Category("Fix Cash Flow")]
        public void FixSchedOutsTwiceAfterFixeeChange()
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

            ObjectSpace.CommitChanges();

            #endregion

            #region Arrange Fix Tags

            var schedOutFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            schedOutFixTag.Name = "S2";
            schedOutFixTag.FixTagType = CashForecastFixTagType.ScheduleOut;

            var reversalFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            reversalFixTag.Name = "R";
            reversalFixTag.FixTagType = CashForecastFixTagType.Ignore;

            #endregion

            #region Arrange Transactions

            // act
            var cfFixee1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixee1.TranDate = new DateTime(2016, 03, 03);
            cfFixee1.Account = account;
            cfFixee1.AccountCcyAmt = 600;
            cfFixee1.Activity = activity;
            cfFixee1.FixRank = 2;
            cfFixee1.Fix = schedOutFixTag;
            cfFixee1.DateUnFix = cfFixee1.TranDate;
            cfFixee1.FixActivity = cfFixee1.Activity;

            var cfFixee2 = ObjectSpace.CreateObject<CashFlow>();
            cfFixee2.TranDate = new DateTime(2016, 03, 03);
            cfFixee2.Account = account;
            cfFixee2.AccountCcyAmt = 500;
            cfFixee2.Activity = activity;
            cfFixee2.FixRank = 2;
            cfFixee2.Fix = schedOutFixTag;
            cfFixee2.DateUnFix = cfFixee2.TranDate;
            cfFixee2.FixActivity = cfFixee2.Activity;

            var cfFixer1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixer1.TranDate = new DateTime(2016, 03, 31);
            cfFixer1.Account = account;
            cfFixer1.AccountCcyAmt = 1400;
            cfFixer1.Activity = activity;
            cfFixer1.FixRank = 3;
            cfFixer1.FixFromDate = new DateTime(2016, 03, 1);
            cfFixer1.FixToDate = new DateTime(2016, 03, 31);
            cfFixer1.Fix = schedOutFixTag;
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

            var fixAlgo1 = new FixCashFlowsAlgorithm2(ObjectSpace, paramObj);

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(3, fixAlgo1.GetCashFlowsToFix().Count()); // assert test data

            #endregion

            #region Assert 1st run

            fixAlgo1.ProcessCashFlows();

            Assert.AreEqual(5, cashFlows.Count());
            Assert.AreEqual(300,
                    cashFlows.Where(x =>
                        x.TranDate == new DateTime(2016, 03, 31))
                    .Sum(x => x.AccountCcyAmt));

            #endregion

            #region Assert 2nd run

            // change Tran Date and simulate setting flag to false
            cfFixee1.TranDate = new DateTime(2016, 03, 05);
            cfFixee1.AccountCcyAmt = 700;
            ObjectSpace.CommitChanges();

            fixAlgo1.ProcessCashFlows();

            var cashFlows2 = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(5, cashFlows2.Count());
            Assert.AreEqual(200,
                    cashFlows2.Where(x =>
                        x.TranDate == new DateTime(2016, 03, 31))
                    .Sum(x => x.AccountCcyAmt));

            #endregion
        }

        [Test]
        [Category("Fix Cash Flow")]
        public void FixSchedOutsTwiceAfterFixerChange()
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

            ObjectSpace.CommitChanges();

            #endregion

            #region Arrange Fix Tags

            var schedOutFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            schedOutFixTag.Name = "S2";
            schedOutFixTag.FixTagType = CashForecastFixTagType.ScheduleOut;

            var reversalFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            reversalFixTag.Name = "R";
            reversalFixTag.FixTagType = CashForecastFixTagType.Ignore;

            #endregion

            #region Arrange Transactions

            // act
            var cfFixee1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixee1.TranDate = new DateTime(2016, 03, 03);
            cfFixee1.Account = account;
            cfFixee1.AccountCcyAmt = 600;
            cfFixee1.Activity = activity;
            cfFixee1.FixRank = 2;
            cfFixee1.Fix = schedOutFixTag;
            cfFixee1.DateUnFix = cfFixee1.TranDate;
            cfFixee1.FixActivity = cfFixee1.Activity;

            var cfFixee2 = ObjectSpace.CreateObject<CashFlow>();
            cfFixee2.TranDate = new DateTime(2016, 03, 03);
            cfFixee2.Account = account;
            cfFixee2.AccountCcyAmt = 500;
            cfFixee2.Activity = activity;
            cfFixee2.FixRank = 2;
            cfFixee2.Fix = schedOutFixTag;
            cfFixee2.DateUnFix = cfFixee2.TranDate;
            cfFixee2.FixActivity = cfFixee2.Activity;

            var cfFixer1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixer1.TranDate = new DateTime(2016, 03, 31);
            cfFixer1.Account = account;
            cfFixer1.AccountCcyAmt = 1400;
            cfFixer1.Activity = activity;
            cfFixer1.FixRank = 3;
            cfFixer1.FixFromDate = new DateTime(2016, 03, 1);
            cfFixer1.FixToDate = new DateTime(2016, 03, 31);
            cfFixer1.Fix = schedOutFixTag;
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

            var fixAlgo1 = new FixCashFlowsAlgorithm2(ObjectSpace, paramObj);

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(3, fixAlgo1.GetCashFlowsToFix().Count()); // assert test data

            #endregion

            #region Assert Intermediate

            fixAlgo1.ProcessCashFlows();

            Assert.AreEqual(5, cashFlows.Count());
            Assert.AreEqual(300,
                    cashFlows.Where(x =>
                        x.TranDate == new DateTime(2016, 03, 31))
                    .Sum(x => x.AccountCcyAmt));


            #endregion

            #region Assert Final

            cfFixer1.TranDate = new DateTime(2016, 03, 28);
            ObjectSpace.CommitChanges();

            fixAlgo1.ProcessCashFlows();

            var cashFlows2 = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(5, cashFlows2.Count());
            Assert.AreEqual(0,
                    cashFlows2.Where(x =>
                        x.TranDate == new DateTime(2016, 03, 31))
                    .Sum(x => x.AccountCcyAmt));
            Assert.AreEqual(300,
                    cashFlows2.Where(x =>
                        x.TranDate == new DateTime(2016, 03, 28))
                    .Sum(x => x.AccountCcyAmt));
            #endregion

        }

        [Test]
        [Category("Fix Cash Flow")]
        public void FixSchedOutLockdown()
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

            ObjectSpace.CommitChanges();

            #endregion

            #region Arrange Fix Tags

            var schedOutFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            schedOutFixTag.Name = "S";
            schedOutFixTag.FixTagType = CashForecastFixTagType.ScheduleOut;

            var reversalFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            reversalFixTag.Name = CashDiscipline.Module.Constants.ReversalFixTag;
            reversalFixTag.FixTagType = CashForecastFixTagType.Ignore;

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

            var fixAlgo = new FixCashFlowsAlgorithm2(ObjectSpace, paramObj);

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(2, fixAlgo.GetCashFlowsToFix().Count()); // assert test data
            Assert.AreEqual(500, cashFlows
                 .Where(cf => cf.TranDate == new DateTime(2016, 03, 12))
                 .Sum(cf => cf.AccountCcyAmt));

            fixAlgo.ProcessCashFlows();

            #endregion

            #region Assert

            cashFlows = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(-100, cashFlows
                .Where(cf => cf.TranDate == paramObj.ApayableNextLockdownDate)
                .Sum(cf => cf.AccountCcyAmt));

            Assert.AreEqual(0, cashFlows
                .Where(cf => cf.TranDate == new DateTime(2016, 03, 12))
                .Sum(cf => cf.AccountCcyAmt));
            #endregion
        }

        [Test]
        [Category("Fix Cash Flow")]
        public void FixAllocLockdown()
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
            cfFixee1.Account = account;
            cfFixee1.AccountCcyAmt = -600;
            cfFixee1.Activity = activity;
            cfFixee1.FixRank = 2;
            cfFixee1.Fix = schedOutFixTag;
            cfFixee1.DateUnFix = cfFixee1.TranDate;
            cfFixee1.FixActivity = cfFixee1.Activity;

            var cfFixer1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixer1.TranDate = new DateTime(2016, 03, 11);
            cfFixer1.Account = account;
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

            var fixAlgo = new FixCashFlowsAlgorithm2(ObjectSpace, paramObj);

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(2, fixAlgo.GetCashFlowsToFix().Count()); // assert test data
            Assert.AreEqual(-600 - 500, cashFlows
                 .Where(cf => cf.TranDate == new DateTime(2016, 03, 11))
                 .Sum(cf => cf.AccountCcyAmt));

            fixAlgo.ProcessCashFlows();

            #endregion

            #region Assert

            cashFlows = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(7, cashFlows.Count);

            Assert.AreEqual(-500, cashFlows
                .Where(cf => cf.TranDate >= new DateTime(2016, 03, 01)
                && cf.TranDate <= new DateTime(2016, 4, 30))
                .Sum(cf => cf.AccountCcyAmt));

            Assert.AreEqual(-600, cashFlows
                .Where(cf => cf.TranDate == new DateTime(2016, 03, 11))
                .Sum(cf => cf.AccountCcyAmt));

            Assert.AreEqual(600 - 500, cashFlows
                .Where(cf => cf.TranDate == paramObj.ApayableNextLockdownDate)
                .Sum(cf => cf.AccountCcyAmt));
            #endregion
        }

        [Test]
        public void NotFixedIfDateNotMatched()
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

            ObjectSpace.CommitChanges();

            #endregion

            #region Arrange Fix Tags

            var schedOutFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            schedOutFixTag.Name = "S2";
            schedOutFixTag.FixTagType = CashForecastFixTagType.ScheduleOut;

            var reversalFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            reversalFixTag.Name = CashDiscipline.Module.Constants.ReversalFixTag;
            reversalFixTag.FixTagType = CashForecastFixTagType.Ignore;

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

            var cfFixer1 = ObjectSpace.CreateObject<CashFlow>();
            cfFixer1.TranDate = new DateTime(2016, 03, 12);
            cfFixer1.Account = account;
            cfFixer1.AccountCcyAmt = 500;
            cfFixer1.Activity = activity;
            cfFixer1.FixRank = 3;
            cfFixer1.Fix = schedOutFixTag;
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

            var fixAlgo = new FixCashFlowsAlgorithm2(ObjectSpace, paramObj);

            var cashFlowsPre = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(2, fixAlgo.GetCashFlowsToFix().Count()); // assert test data

            fixAlgo.ProcessCashFlows();

            #endregion

            #region Assert

            var cashFlowsPost = ObjectSpace.GetObjects<CashFlow>();
            Assert.AreEqual(2, cashFlowsPost.Count);

            #endregion
        }

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

            var fixAlgo = new FixCashFlowsAlgorithm2(ObjectSpace, paramObj);

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(2, fixAlgo.GetCashFlowsToFix().Count()); // assert test data

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

            var fixAlgo = new FixCashFlowsAlgorithm2(ObjectSpace, paramObj);

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            fixAlgo.ProcessCashFlows();

            #endregion

            #region Assert

            cashFlows = ObjectSpace.GetObjects<CashFlow>();

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
