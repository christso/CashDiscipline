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
        public void GetFixees()
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

            #endregion

            #region Assert

            var cashFlows = fixAlgo.GetCashFlowsToFix();

            var fixees1 = fixAlgo.GetFixees(cashFlows, cf1);
            var fixees2 = fixAlgo.GetFixees(cashFlows, cf2);

            Assert.AreEqual(0, fixees1.Count());
            Assert.AreEqual(1, fixees2.Count());
            Assert.AreEqual(cf1, fixees2.FirstOrDefault());

            #endregion

        }

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

            var unfixed = new XPQuery<CashFlow>(ObjectSpace.Session)
                .Where(cf =>
                (cf.Fix == null || cf.Fix.FixTagType != CashForecastFixTagType.Ignore)
                && !cf.IsFixerUpdated && !cf.IsFixeeUpdated);

            Assert.AreEqual(0, unfixed.Count());

            var unfixed2 = fixAlgo.GetCashFlowsToFix();
            Assert.AreEqual(0, unfixed2.Count());

            Assert.AreEqual(0, fixAlgo.GetCashFlowsToFix().Count());

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
            //fixAlgo.ProcessCashFlows();
            //fixee.TranDate = new Date()
            //fixee.IsFixeeUpdated = false;
            //fixAlgo.ProcessCashFlows();

        }

        [Test]
        [Category("Fix Cash Flow")]
        public void FixSchedOutsTwiceAfterFixerChange()
        {
            //fixer.TranDate = new Date()
            //fixer.IsFixerUpdated = false;
        }
    }
}
