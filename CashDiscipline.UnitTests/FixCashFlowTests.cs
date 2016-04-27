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
            CashDiscipline.Module.DatabaseUpdate.Updater.CreateCashFlowDefaults(ObjectSpace);
        }

        public override void OnAddExportedTypes(ModuleBase module)
        {
            CashDisciplineTestHelper.AddExportedTypes(module);
        }
        #endregion

        [Test]
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

            CashDiscipline.Module.DatabaseUpdate.Updater.InitFixTags(ObjectSpace);

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

            var algoObjSpace = (XPObjectSpace)Application.CreateObjectSpace();
            var fixAlgo1 = new MemFixCashFlowsAlgorithm(algoObjSpace, paramObj);


            #endregion

            #region Assert Intermediate

            fixAlgo1.ProcessCashFlows();

            ObjectSpace.Refresh();
            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(5, cashFlows.Count());
            Assert.AreEqual(300,
                    cashFlows.Where(x =>
                        x.TranDate == new DateTime(2016, 03, 31))
                    .Sum(x => x.AccountCcyAmt));


            #endregion

            #region Assert Final

            cfFixer1 = cashFlows.Where(cf => cf.Oid == cfFixer1.Oid).FirstOrDefault();
            cfFixer1.TranDate = new DateTime(2016, 03, 28);
            cfFixer1.Save();
            ObjectSpace.CommitChanges();

            fixAlgo1.ProcessCashFlows();
            ObjectSpace.Refresh();

            var cashFlows2 = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(5, cashFlows2.Count());

            // $500+$600 should move from 31st to 28th
            Assert.AreEqual(300,
                    cashFlows2.Where(x =>
                        x.TranDate == new DateTime(2016, 03, 28))
                    .Sum(x => x.AccountCcyAmt));

            Assert.AreEqual(0,
                    cashFlows2.Where(x =>
                        x.TranDate == new DateTime(2016, 03, 31))
                    .Sum(x => x.AccountCcyAmt));
            #endregion

        }

        [Test]
        public void FixerSyncedIfFixeesSynced()
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
            cfFixee1.AccountCcyAmt = -300;
            cfFixee1.CounterCcyAmt = -300;
            cfFixee1.Activity = activity;
            cfFixee1.FixRank = 2;
            cfFixee1.Fix = schedOutFixTag;
            cfFixee1.DateUnFix = cfFixee1.TranDate;
            cfFixee1.FixActivity = cfFixee1.Activity;

            var cfFixee2 = ObjectSpace.CreateObject<CashFlow>();
            cfFixee2.TranDate = new DateTime(2016, 03, 11);
            cfFixee2.Account = usdAccount;
            cfFixee1.CounterCcy = ccyUSD;
            cfFixee2.AccountCcyAmt = -150;
            cfFixee2.CounterCcyAmt = -150;
            cfFixee2.Activity = activity;
            cfFixee2.FixRank = 2;
            cfFixee2.Fix = schedOutFixTag;
            cfFixee2.DateUnFix = cfFixee2.TranDate;
            cfFixee2.FixActivity = cfFixee2.Activity;

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

            var algoObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
            var fixAlgo = new MemFixCashFlowsAlgorithm(algoObjectSpace, paramObj);

            #endregion

            #region Assert that all cash flows have synced status

            fixAlgo.ProcessCashFlows();
            ObjectSpace.Refresh();
            var cfs = ObjectSpace.GetObjects<CashFlow>();
            Assert.AreEqual(6, cfs.Count);

            var cfsNonSynced = cfs.Where(cf =>
                ((!cf.IsFixeeSynced && !cf.IsFixerSynced) || !cf.IsFixerFixeesSynced)
                && cf.Fix.FixTagType != CashForecastFixTagType.Ignore);
            Assert.AreEqual(0, cfsNonSynced.Count());

            #endregion

            #region Assert that changing a cash flow will change fix status of related cash flows

            cfFixee1 = cfs.Where(cf => cf.Oid == cfFixee1.Oid).FirstOrDefault();
            cfFixee1.AccountCcyAmt = -350;

            ObjectSpace.Session.Save(cfs);
            ObjectSpace.CommitChanges();

            cfsNonSynced = cfs.Where(cf =>
                !cf.IsFixeeSynced
                && cf.Fix.FixTagType != CashForecastFixTagType.Ignore);
            Assert.AreEqual(1, cfsNonSynced.Count());

            cfsNonSynced = cfs.Where(cf =>
                !cf.IsFixerSynced
                && cf.Fix.FixTagType != CashForecastFixTagType.Ignore);
            Assert.AreEqual(1, cfsNonSynced.Count());

            cfsNonSynced = cfs.Where(cf =>
                !cf.IsFixerFixeesSynced
                && cf.Fix.FixTagType != CashForecastFixTagType.Ignore);
            Assert.AreEqual(1, cfsNonSynced.Count());

            #endregion

            #region Assert that changes are propragated to new objectspace

            algoObjectSpace = (XPObjectSpace)Application.CreateObjectSpace();
            var algoCfs = algoObjectSpace.GetObjects<CashFlow>();

            var algoCfsNonSynced = algoCfs.Where(cf =>
                !cf.IsFixeeSynced
                && cf.Fix.FixTagType != CashForecastFixTagType.Ignore);
            Assert.AreEqual(1, cfsNonSynced.Count());

            algoCfsNonSynced = algoCfs.Where(cf =>
                !cf.IsFixerSynced
                && cf.Fix.FixTagType != CashForecastFixTagType.Ignore);
            Assert.AreEqual(1, cfsNonSynced.Count());

            algoCfsNonSynced = algoCfs.Where(cf =>
                !cf.IsFixerFixeesSynced
                && cf.Fix.FixTagType != CashForecastFixTagType.Ignore);
            Assert.AreEqual(1, cfsNonSynced.Count());

            #endregion

            #region Assert that 2nd call of algorithm will return correct result

            fixAlgo = new MemFixCashFlowsAlgorithm(algoObjectSpace, paramObj);

            //var cfsToFix = fixAlgo.GetCashFlowsToFix();
            //Assert.AreEqual(2, cfsToFix.Count());

            fixAlgo.ProcessCashFlows();
            ObjectSpace.Refresh();

            cfs = ObjectSpace.GetObjects<CashFlow>();
            Assert.AreEqual(6, cfs.Count);

            #endregion
        }
        
    }
}
