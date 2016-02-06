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

namespace CTMS.UnitTests.InMemoryDbTest
{
    [TestFixture]
    public class CashFlowTests : TestBase
    {
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

            // assert
            Assert.AreEqual(ccyUSD, cf.CounterCcy);
        }
        
        [Test]
        public void ForexRate_GetRate_LastRateForCurrency()
        {
            // arrange
            
            var funcCcy = SetOfBooks.CachedInstance.FunctionalCurrency;
            var fromCcy = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var session = ObjectSpace.Session;
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

        public override void SetupObjects()
        {
            CTMS.Module.DatabaseUpdate.Updater.CreateCurrencies(ObjectSpace);
            SetOfBooks.GetInstance(ObjectSpace);
            CTMS.Module.DatabaseUpdate.Updater.InitSetOfBooks(ObjectSpace);
        }
    }

}
