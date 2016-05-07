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
using System.Collections;

namespace CashDiscipline.UnitTests
{
    [TestFixture]
    public class CashFlowTests : TestBase
    {
        public CashFlowTests()
        {
            SetTesterDbType(TesterDbType.MsSql);

            var tester = Tester as MSSqlDbTestBase;
            if (tester != null)
                tester.DatabaseName = Constants.TestDbName;
        }

        public override void OnSetup()
        {
            CashDisciplineTestHelper.RegisterCustomFunctions();

            CashDiscipline.Module.DatabaseUpdate.Updater.CreateCurrencies(ObjectSpace);
            SetOfBooks.GetInstance(ObjectSpace);
            CashDiscipline.Module.DatabaseUpdate.Updater.InitSetOfBooks(ObjectSpace);
        }

        public override void OnAddExportedTypes(ModuleBase module)
        {
            CashDisciplineTestHelper.AddExportedTypes(module);
        }

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
            ObjectSpace.CommitChanges();

            // assert
            Assert.AreEqual(ccyUSD, cf.CounterCcy);
        }
        
        [Test]
        public void ForexRate_GetRate_LastRateForCurrency()
        {
            // arrange
            
            var funcCcy = SetOfBooks.CachedInstance.FunctionalCurrency;
            var fromCcy = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
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
            ObjectSpace.CommitChanges();

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
            ObjectSpace.CommitChanges();

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
            ObjectSpace.CommitChanges();
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

        [Test]
        public void SaveSnapshot()
        {
            #region Arrange
            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ AUD";
            account.Currency = ccyAUD;

            var activity = ObjectSpace.CreateObject<Activity>();
            activity.Name = "AR Rcpt";
            #endregion

            #region Actions
            var tranDate = new DateTime(2014, 03, 31);
            decimal amount = 1000;
            int loops = 2;

            for (int i = 0; i < loops; i++)
            {
                var cf1 = ObjectSpace.CreateObject<CashFlow>();
                cf1.TranDate = tranDate;
                cf1.Account = account;
                cf1.Activity = activity;
                cf1.AccountCcyAmt = amount;
            }

            ObjectSpace.CommitChanges();

            var snapshot = CashFlow.SaveSnapshot(ObjectSpace.Session, tranDate);
            ObjectSpace.CommitChanges();
            #endregion

            #region Asserts
            var sCfs = snapshot.CashFlows;
            Assert.AreEqual(amount * loops, sCfs.Sum(x => x.AccountCcyAmt));
            #endregion
        }

        [Test]
        public void DeleteSnapshot()
        {
            #region Arrange

            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ AUD";
            account.Currency = ccyAUD;

            var activity = ObjectSpace.CreateObject<Activity>();
            activity.Name = "AR Rcpt";
            #endregion

            #region Create Snapshot

            var snapshot = ObjectSpace.CreateObject<CashFlowSnapshot>();
            snapshot.Name = "Snapshot 1";

            var tranDate = new DateTime(2014, 03, 31);
            decimal amount = 1000;
            int loops = 2;

            for (int i = 0; i < loops; i++)
            {
                var cf1 = ObjectSpace.CreateObject<CashFlow>();
                cf1.Snapshot = snapshot;
                cf1.TranDate = tranDate;
                cf1.Account = account;
                cf1.Activity = activity;
                cf1.AccountCcyAmt = amount;
            }
            ObjectSpace.CommitChanges();
            Assert.AreEqual(loops, ObjectSpace.GetObjects<CashFlow>().Count);
            Assert.AreEqual(2, ObjectSpace.GetObjects<CashFlowSnapshot>().Count);

            #endregion

            #region Delete Snapshot

            var controller = Application.CreateController<CashFlowSnapshotViewController>();
            var view = Application.CreateDetailView(ObjectSpace, snapshot);
            controller.SetView(view);

            controller.DeleteSnapshot(snapshot);

            #endregion

            #region Assert

            Assert.AreEqual(0, ObjectSpace.GetObjects<CashFlow>().Count);
            Assert.AreEqual(1, ObjectSpace.GetObjects<CashFlowSnapshot>().Count);

            #endregion

        }

        [Test]
        public void DeleteSnapshots()
        {
            #region Arrange

            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ AUD";
            account.Currency = ccyAUD;

            var activity = ObjectSpace.CreateObject<Activity>();
            activity.Name = "AR Rcpt";
            #endregion

            #region Create Snapshot

            for (int i = 0; i < 3; i++)
            {
                // create snapshot
                var snapshot = ObjectSpace.CreateObject<CashFlowSnapshot>();
                snapshot.Name = string.Format("Snapshot {0}", i);

                // create cash flows
                for (int j = 0; j < 2; j++)
                {
                    var cf1 = ObjectSpace.CreateObject<CashFlow>();
                    cf1.Snapshot = snapshot;
                    cf1.TranDate = new DateTime(2014, 03, 31);
                    cf1.Account = account;
                    cf1.Activity = activity;
                    cf1.AccountCcyAmt = 1000;
                }
            }

            ObjectSpace.CommitChanges();
            Assert.AreEqual(3*2, ObjectSpace.GetObjects<CashFlow>().Count);
            Assert.AreEqual(4, ObjectSpace.GetObjects<CashFlowSnapshot>().Count);

            #endregion

            #region Delete Snapshot

            var controller = Application.CreateController<CashFlowSnapshotViewController>();
            var view = Application.CreateDetailView(ObjectSpace, ObjectSpace.FindObject<CashFlowSnapshot>(null));
            controller.SetView(view);

            var snapshots = ObjectSpace.GetObjects<CashFlowSnapshot>();
            controller.DeleteSnapshots(snapshots);

            ObjectSpace.Session.PurgeDeletedObjects();

            #endregion

            #region Assert

            Assert.AreEqual(0, ObjectSpace.GetObjects<CashFlow>().Count);
            Assert.AreEqual(0, ObjectSpace.GetObjects<CashFlowSnapshot>().Count);

            #endregion
        }

        [Test]
        public void FindForeignKey()
        {
            var cf = ObjectSpace.CreateObject<CashFlow>();

        }

        //[Test]
        public void CalculateAccountSummary()
        {
            #region Arrange
            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ AUD";
            account.Currency = ccyAUD;

            var activity = ObjectSpace.CreateObject<Activity>();
            activity.Name = "AR Rcpt";
            #endregion

            #region Transactions
            var prevSnapshot = ObjectSpace.CreateObject<CashFlowSnapshot>();
            prevSnapshot.Name = "Forecast";

            var actualDate = new DateTime(2014, 03, 20);

            #region Current Snapshot
            var cf5 = ObjectSpace.CreateObject<CashFlow>();
            cf5.TranDate = actualDate;
            cf5.Account = account;
            cf5.Activity = activity;
            cf5.AccountCcyAmt = 200;
            cf5.Status = CashFlowStatus.Actual;

            var cf6 = ObjectSpace.CreateObject<CashFlow>();
            cf6.TranDate = actualDate.AddDays(1);
            cf6.Account = account;
            cf6.Activity = activity;
            cf6.AccountCcyAmt = 300;
            cf6.Status = CashFlowStatus.Actual;

            var cf1 = ObjectSpace.CreateObject<CashFlow>();
            cf1.TranDate = new DateTime(2014, 03, 31);
            cf1.Account = account;
            cf1.Activity = activity;
            cf1.AccountCcyAmt = 1000;

            var cf2 = ObjectSpace.CreateObject<CashFlow>();
            cf2.TranDate = new DateTime(2014, 04, 10);
            cf2.Account = account;
            cf2.Activity = activity;
            cf2.AccountCcyAmt = 1000;
            #endregion

            #region Forecast Snapshot
            var cf3 = ObjectSpace.CreateObject<CashFlow>();
            cf3.TranDate = new DateTime(2014, 03, 25);
            cf3.Account = account;
            cf3.Activity = activity;
            cf3.AccountCcyAmt = 1000;
            cf3.Snapshot = prevSnapshot;

            var cf4 = ObjectSpace.CreateObject<CashFlow>();
            cf4.TranDate = new DateTime(2014, 04, 12);
            cf4.Account = account;
            cf4.Activity = activity;
            cf4.AccountCcyAmt = 1000;
            cf4.Snapshot = prevSnapshot;
            #endregion

            ObjectSpace.CommitChanges();
            #endregion

            #region Report Parameter
            var currSnapshot = ObjectSpace.GetObjectByKey<CashFlowSnapshot>(SetOfBooks.CachedInstance.CurrentCashFlowSnapshot.Oid);
            var reportParam = CashReportParam.GetInstance(ObjectSpace);
            reportParam.Snapshot1 = currSnapshot;
            reportParam.Snapshot1 = prevSnapshot;
            reportParam.FromDate = new DateTime(2014, 3, 1);
            reportParam.ToDate = new DateTime(2014, 6, 30);

            var snapshots = new List<CashFlowSnapshot>() { currSnapshot, prevSnapshot };
            #endregion

            #region Asserts
            AccountSummary.CalculateCashFlow(ObjectSpace, reportParam.FromDate.Date, reportParam.ToDate, snapshots);
            var startDate = AccountSummary.GetUniqueBalanceDate(ObjectSpace.Session, reportParam.FromDate);
            AccountSummary.CalculateBalance(ObjectSpace, startDate, snapshots);

            // start date is new DateTime(2014, 03, 20) if report FromDate is before
            var ass = ObjectSpace.GetObjects<AccountSummary>();
            // assert that cash balance in current snapshot exists in Account Summary
            Assert.AreEqual(1, ass.Where(x => x.LineType == AccountSummaryLineType.Balance && x.Snapshot == currSnapshot
                && x.AccountCcyAmt == cf5.AccountCcyAmt && x.TranDate == actualDate).Count());
            // Assert that actual cash balance in current snapshot is included in previous snapshot
            Assert.AreEqual(1, ass.Where(x => x.LineType == AccountSummaryLineType.Balance && x.Snapshot == prevSnapshot
                            && x.AccountCcyAmt == cf5.AccountCcyAmt && x.TranDate == actualDate).Count());
            // Assert that actual cash flow in current snapshot is included in previous snapshot
            Assert.AreEqual(1, ass.Where(x => x.LineType == AccountSummaryLineType.Flow && x.Snapshot == prevSnapshot
                            && x.AccountCcyAmt == cf6.AccountCcyAmt && x.TranDate == actualDate.AddDays(1)).Count());
            #endregion
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

            var fromDate = new DateTime(2013, 11, 16);
            var toDate = new DateTime(2013, 12, 17);
            var uploader = new BankStmtToCashFlow(ObjectSpace, fromDate, toDate, null);
            uploader.Process();

            ObjectSpace.Refresh();

            var bankStmts = ObjectSpace.GetObjects<BankStmt>();
            var cashFlows = ObjectSpace.GetObjects<CashFlow>();

            #endregion

            #region Assert

            Assert.AreEqual(bankStmts.Sum(x => x.TranAmount), cashFlows.Sum(x => x.AccountCcyAmt));

            #endregion
        }


        [Test]
        public void GetSetOfBooksInstance()
        {
            #region Arrange

            ObjectSpace.Session.Delete(new XPCollection(ObjectSpace.Session, typeof(SetOfBooks)));
            ObjectSpace.CommitChanges();

            var setOfBooks = SetOfBooks.GetInstance(ObjectSpace.Session);
            ObjectSpace.CommitChanges();

            var os = (XPObjectSpace)Application.CreateObjectSpace();

            #endregion

            #region Assert that SetOfBooks is not null

            var setOfBooks1 = ObjectSpace.GetObjects<SetOfBooks>();
            Assert.AreEqual(1, setOfBooks1.Count);

            var setOfBooks2 = os.Session.FindObject<SetOfBooks>(PersistentCriteriaEvaluationBehavior.InTransaction, null);
            Assert.NotNull(setOfBooks2);

            #endregion
        }

        [Test]
        public void GetCashFlowDefaultsInstance()
        {
            #region Arrange

            ObjectSpace.Session.Delete(new XPCollection(ObjectSpace.Session, typeof(CashFlowDefaults)));
            ObjectSpace.CommitChanges();

            var instance = CashFlowDefaults.GetInstance(ObjectSpace.Session);
            ObjectSpace.CommitChanges();

            var os = (XPObjectSpace)Application.CreateObjectSpace();

            #endregion

            #region Assert that SetOfBooks is not null

            var instance1 = ObjectSpace.GetObjects<CashFlowDefaults>();
            Assert.AreEqual(1, instance1.Count);

            var instance2 = os.Session.FindObject<CashFlowDefaults>(PersistentCriteriaEvaluationBehavior.InTransaction, null);
            Assert.NotNull(instance2);

            #endregion
        }

        [Test]
        public void EvaluateEoMonthCustomFunction()
        {
            {
                var cf = ObjectSpace.CreateObject<CashFlow>();
                cf.TranDate = new DateTime(2016, 3, 28);
                var result = cf.Evaluate(CriteriaOperator.Parse("DATEDIFFDAY(TranDate, EOMONTH(TranDate)) < 7"));
                Assert.IsTrue((bool)result);
            }

            {
                var cf = ObjectSpace.CreateObject<CashFlow>();
                cf.TranDate = new DateTime(2016, 3, 15);
                var result = cf.Evaluate(CriteriaOperator.Parse("DATEDIFFDAY(TranDate, EOMONTH(TranDate)) < 7"));
                Assert.IsFalse((bool)result);
            }
        }

    }

}
