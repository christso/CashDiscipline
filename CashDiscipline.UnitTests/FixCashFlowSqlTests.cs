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
            CashDiscipline.Module.DatabaseUpdate.Updater.CreateFunctions(ObjectSpace);
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
        public void TestFixParameters()
        {
            var fixActivity = ObjectSpace.CreateObject<Activity>();
            fixActivity.Name = "AP Pymt";

            #region Arrange Fix Tags

            var schedOutFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            schedOutFixTag.Name = "S2";
            schedOutFixTag.FixTagType = CashForecastFixTagType.ScheduleOut;

            var allocFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            allocFixTag.Name = "B3";
            allocFixTag.FixTagType = CashForecastFixTagType.Allocate;

            CashDiscipline.Module.DatabaseUpdate.Updater.InitFixTags(ObjectSpace);

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

            ObjectSpace.CommitChanges();

            var fixAlgo = new SqlFixCashFlowsAlgorithm(ObjectSpace, paramObj);

            var parameters = fixAlgo.CreateParameters();
            var sqlParameters = fixAlgo.CreateSqlParameters();
            // todo: is CashFlowFixParam created?

            #endregion

            #region Assert

            var conn = ObjectSpace.Session.Connection;
            var cmd = conn.CreateCommand();
            
            foreach (var parameter in parameters)
            {
                var sqlParameter = sqlParameters
                    .Where(p => p.ParameterName == parameter.ParameterName)
                    .FirstOrDefault();

                cmd.CommandText = "SELECT " + sqlParameter.CommandText;
                var sqlValue = cmd.ExecuteScalar();
                
                Assert.AreEqual(string.Format("{0}:{1}", parameter.ParameterName, parameter.Value),
                    string.Format("{0}:{1}", parameter.ParameterName, sqlValue));
            }
            
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

            CashDiscipline.Module.DatabaseUpdate.Updater.InitFixTags(ObjectSpace);

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

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(-600 - 500, cashFlows
                 .Where(cf => cf.TranDate == new DateTime(2016, 03, 11))
                 .Sum(cf => cf.AccountCcyAmt));

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

            fixAlgo.ProcessCashFlows();
            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

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

            Assert.AreEqual(-666.67, Math.Round(cashFlows
                .Where(cf => cf.TranDate == new DateTime(2016, 03, 11))
                .Sum(cf => cf.FunctionalCcyAmt), 2));

            Assert.AreEqual(100, cashFlows
                .Where(cf => cf.TranDate == paramObj.ApayableNextLockdownDate)
                .Sum(cf => cf.AccountCcyAmt));

            Assert.AreEqual(111.11, Math.Round(cashFlows
                .Where(cf => cf.TranDate == paramObj.ApayableNextLockdownDate)
                .Sum(cf => cf.FunctionalCcyAmt), 2));
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
            audAccount.FixAccount = audAccount;

            var usdAccount = ObjectSpace.CreateObject<Account>();
            usdAccount.Name = "VHA ANZ USD";
            usdAccount.Currency = ccyUSD;
            usdAccount.FixAccount = audAccount;

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
            ObjectSpace.CommitChanges();

            var fixAlgo = new SqlFixCashFlowsAlgorithm(ObjectSpace, paramObj);

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            fixAlgo.ProcessCashFlows();
            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

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
            ObjectSpace.CommitChanges();

            var fixAlgo = new SqlFixCashFlowsAlgorithm(ObjectSpace, paramObj);

            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            Assert.AreEqual(500, cashFlows
                 .Where(cf => cf.TranDate == new DateTime(2016, 03, 12))
                 .Sum(cf => cf.AccountCcyAmt));

            fixAlgo.ProcessCashFlows();
            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();
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
            cfFixer1 = ObjectSpace.FindObject<CashFlow>(CriteriaOperator.Parse("Oid = ?", cfFixer1.Oid));
            cfFixer1.Delete();
            ObjectSpace.CommitChanges();

            fixAlgo.ProcessCashFlows();
            cashFlows = ObjectSpace.GetObjects<CashFlow>();
            Assert.AreEqual(1, cashFlows.Count);

            #endregion
        }


        [Test]
        public void IfDeleteFixeeCashFlow()
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

            var counterparty = ObjectSpace.FindObject<Counterparty>(CriteriaOperator.Parse("Name = ?", "UNDEFINED"));

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

            Assert.AreEqual(500, cashFlows
                 .Where(cf => cf.TranDate == new DateTime(2016, 03, 12))
                 .Sum(cf => cf.AccountCcyAmt));

            fixAlgo.ProcessCashFlows();
            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();
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

            cfFixee1 = new XPQuery<CashFlow>(ObjectSpace.Session)
                .Where(cf => cf.Oid == cfFixee1.Oid).FirstOrDefault();

            cfFixee1.Delete();
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
            cfFixer1.TranDate = new DateTime(2016, 03, 26);
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
                .Where(cf => cf.TranDate >= new DateTime(2016, 03, 25)
                    && cf.TranDate <= new DateTime(2016, 03, 26)
                && cf.CounterCcy == ccyUSD).Sum(cf => cf.CounterCcyAmt));

            Assert.AreEqual(-500, cashFlows
                .Where(cf => cf.TranDate >= new DateTime(2016, 03, 25)
                    && cf.TranDate <= new DateTime(2016, 03, 26)
                && cf.CounterCcy == ccyAUD).Sum(cf => cf.CounterCcyAmt));

            #endregion
        }

        [Test]
        public void MapCashFlows()
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

            var techActivity = ObjectSpace.CreateObject<Activity>();
            techActivity.Name = "Tech Cost";
            techActivity.FixActivity = techActivity;

            var handsetActivity = ObjectSpace.CreateObject<Activity>();
            handsetActivity.Name = "Handset Pchse";
            handsetActivity.FixActivity = handsetActivity;

            var fixActivity = ObjectSpace.CreateObject<Activity>();
            fixActivity.Name = "AP Pymt";

            ObjectSpace.CommitChanges();

            #endregion

            #region Arrange Fix Tags

            var schedOutFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            schedOutFixTag.Name = "S";
            schedOutFixTag.FixTagType = CashForecastFixTagType.ScheduleOut;

            var allocFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            allocFixTag.Name = "B";
            allocFixTag.FixTagType = CashForecastFixTagType.Allocate;

            var schedInFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            schedInFixTag.Name = "C";
            schedInFixTag.FixTagType = CashForecastFixTagType.ScheduleIn;

            var ignoreFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            ignoreFixTag.Name = "IG1";
            ignoreFixTag.FixTagType = CashForecastFixTagType.Ignore;

            CashDiscipline.Module.DatabaseUpdate.Updater.InitFixTags(ObjectSpace);

            #endregion

            #region Arrange Transactions


            // act
            var cf1 = ObjectSpace.CreateObject<CashFlow>();
            cf1.TranDate = new DateTime(2016, 03, 11);
            cf1.Account = usdAccount;
            cf1.CounterCcy = ccyUSD;
            cf1.AccountCcyAmt = -600;
            cf1.CounterCcyAmt = -600;
            cf1.Activity = handsetActivity;
            cf1.FixRank = 2;
            cf1.DateUnFix = cf1.TranDate;
            cf1.FixActivity = cf1.Activity;

            var cf2 = ObjectSpace.CreateObject<CashFlow>();
            cf2.TranDate = new DateTime(2016, 03, 25);
            cf2.Account = audAccount;
            cf2.CounterCcy = ccyAUD;
            cf2.CounterCcyAmt = -500;
            cf2.AccountCcyAmt = -500;
            cf2.Activity = techActivity;
            cf2.FixRank = 3;
            cf2.FixFromDate = new DateTime(2016, 03, 01);
            cf2.FixToDate = new DateTime(2016, 03, 31);
            cf2.DateUnFix = cf2.TranDate;
            cf2.FixActivity = cf2.Activity;

            var cf3 = ObjectSpace.CreateObject<CashFlow>();
            cf3.TranDate = new DateTime(2016, 03, 26);
            cf3.Account = usdAccount;
            cf3.CounterCcy = ccyUSD;
            cf3.CounterCcyAmt = -500;
            cf3.AccountCcyAmt = -500;
            cf3.Activity = techActivity;
            cf3.FixRank = 3;
            cf3.FixFromDate = new DateTime(2016, 03, 01);
            cf3.FixToDate = new DateTime(2016, 03, 31);
            cf3.DateUnFix = cf3.TranDate;
            cf3.FixActivity = cf3.Activity;

            ObjectSpace.CommitChanges();

            #endregion

            #region Arrange Mapping

            int rowIndex = 0;
            int mapStep = 0;

            mapStep += 1; // increment to next step

            var map3 = ObjectSpace.CreateObject<CashFlowFixMapping>();
            map3.CriteriaExpression = "Source.Team LIKE 'AP' AND Activity.Name LIKE 'Handset Pchse'";
            map3.CriteriaStatus = CashFlowStatus.Forecast;
            map3.FixActivity = handsetActivity;
            map3.Fix = schedOutFixTag;
            map3.FixRank = 2;
            map3.FixFromDateExpr = "TranDate";
            map3.MapStep = mapStep;
            map3.RowIndex = rowIndex++;

            var map5 = ObjectSpace.CreateObject<CashFlowFixMapping>();
            map5.CriteriaExpression = "Source.Team LIKE 'AP' AND Activity.Name LIKE 'iPhone Pchse Pymt'";
            map5.CriteriaStatus = CashFlowStatus.Forecast;
            map5.Fix = schedOutFixTag;
            map5.FixRank = 2;
            map5.FixFromDateExpr = "TranDate";
            map5.FixActivity = handsetActivity;
            map5.MapStep = mapStep;
            map5.RowIndex = rowIndex++;

            var map6 = ObjectSpace.CreateObject<CashFlowFixMapping>();
            map6.CriteriaExpression = "Activity.Name IN ('V Subs Rcpt','3 DD','V Subs DD Rcpt')";
            map6.CriteriaStatus = CashFlowStatus.Forecast;
            map6.Fix = schedInFixTag;
            map6.FixRank = 2;
            map6.FixFromDateExpr = "TranDate";
            map6.MapStep = mapStep;
            map6.RowIndex = rowIndex++;

            var map7 = ObjectSpace.CreateObject<CashFlowFixMapping>();
            map7.CriteriaExpression = "Source.Team LIKE 'Trend' AND Activity.ActivityL1 NOT LIKE 'Receipts'";
            map7.CriteriaStatus = CashFlowStatus.Forecast;
            map7.Fix = schedOutFixTag;
            map7.FixRank = 2;
            map7.FixFromDateExpr = "TranDate";
            map7.MapStep = mapStep;
            map7.RowIndex = rowIndex++;

            var map8 = ObjectSpace.CreateObject<CashFlowFixMapping>();
            map8.CriteriaExpression = "Activity.ActivityL1 LIKE 'Receipts'";
            map8.CriteriaStatus = CashFlowStatus.Forecast;
            map8.Fix = schedInFixTag;
            map8.FixRank = 2;
            map8.FixFromDateExpr = "dbo.BOMONTH(TranDate)";
            map8.FixToDateExpr = "EOMONTH(TranDate)";
            map8.MapStep = mapStep;
            map8.RowIndex = rowIndex++;

            var map9 = ObjectSpace.CreateObject<CashFlowFixMapping>();
            map9.CriteriaExpression = "Activity.ActivityL1 LIKE 'Financing Cash Flow'";
            map9.CriteriaStatus = CashFlowStatus.Forecast;
            map9.Fix = ignoreFixTag;
            map9.FixFromDateExpr = "TranDate";
            map9.MapStep = mapStep;
            map9.RowIndex = rowIndex++;

            var map1 = ObjectSpace.CreateObject<CashFlowFixMapping>();
            map1.CriteriaExpression = "Source.Team LIKE 'Treasury'";
            map1.CriteriaStatus = CashFlowStatus.Forecast;
            map1.Fix = ignoreFixTag;
            map1.FixFromDateExpr = "TranDate";
            map1.MapStep = mapStep;
            map1.RowIndex = rowIndex++;

            var map10 = ObjectSpace.CreateObject<CashFlowFixMapping>();
            map10.CriteriaExpression = "Source.Team LIKE 'Inventory'";
            map10.CriteriaStatus = CashFlowStatus.Forecast;
            map10.Fix = schedOutFixTag;
            map10.FixRank = 3;
            map10.FixFromDateExpr = "dbo.BOMONTH(TranDate)";
            map10.FixToDateExpr = "EOMONTH(TranDate)";
            map10.MapStep = mapStep;
            map10.RowIndex = rowIndex++;

            var map4 = ObjectSpace.CreateObject<CashFlowFixMapping>();
            map4.CriteriaExpression = "Else";
            map4.CriteriaStatus = CashFlowStatus.Forecast;
            map4.Fix = schedOutFixTag;
            map4.FixRank = 2;
            map4.FixFromDateExpr = "TranDate";
            map4.RowIndex = rowIndex++;
            map4.MapStep = mapStep;

            mapStep += 1; // increment to next step

            var map2 = ObjectSpace.CreateObject<CashFlowFixMapping>();
            map2.CriteriaExpression = "DATEDIFF(d, TranDate,EOMONTH(TranDate)) < 7";
            map2.CriteriaStatus = CashFlowStatus.Forecast;
            map2.FixToDateExpr = "EOMONTH(TranDate)";
            map3.RowIndex = rowIndex++;
            map2.MapStep = mapStep;

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

            var mapper = new CashFlowFixMapper(ObjectSpace);

            var fixAlgo = new SqlFixCashFlowsAlgorithm(ObjectSpace, paramObj, mapper);
            fixAlgo.ProcessCashFlows();
            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

            #endregion

            #region Assert

            // TODO


            #endregion

            #region Assert Step

            #endregion

        }


        [Test]
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

            CashDiscipline.Module.DatabaseUpdate.Updater.InitFixTags(ObjectSpace);

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
            ObjectSpace.CommitChanges();

            var fixAlgo = new SqlFixCashFlowsAlgorithm(ObjectSpace, paramObj);

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

            CashDiscipline.Module.DatabaseUpdate.Updater.InitFixTags(ObjectSpace);

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

            var fixAlgo = new SqlFixCashFlowsAlgorithm(ObjectSpace, paramObj);

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

    }
}
