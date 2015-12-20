using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;

using CTMS.Module.DatabaseUpdate;
using System.Diagnostics;
using CTMS.Module.BusinessObjects.Forex;
using CTMS.Module.Controllers.Forex;
using CTMS.Module.ParamObjects.Cash;
using CTMS.UnitTests.InMemoryDbTest;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;

namespace CTMS.UnitTests.MSSqlDbTest
{
    [TestFixture]
    /*  TODO: move into InMemoryDataStore
     CashFlow_SaveSnapshot_Success has failed:
  System.NotSupportedException : Command 'DevExpress.Xpo.Helpers.CommandChannelHelper.ExplicitBeginTransaction' is not supported by DevExpress.Xpo.DB.DataSetDataStore.
  d:\CTSO\Data\Visual_Studio\Active_Projects\CTMS\Repo\CTMS.Module\BusinessObjects\Cash\CashFlow.cs(83, 0) : CTMS.Module.BusinessObjects.Cash.CashFlow.OnSaving()
  d:\CTSO\Data\Visual_Studio\Active_Projects\CTMS\Repo\CTMS.UnitTests\InMemoryDbTest\CashFlowMemTests.cs(301, 0) : CTMS.UnitTests.InMemoryDbTest.CashFlowMemTests.CashFlow_SaveSnapshot_Success()
     * */
    public class CashFlowDbTests : MSSqlDbTestBase
    {
        [Test]
        public void CashFlow_AccountSummary_IsCorrect()
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
        [Category("Coverage_3")]
        public void CashFlow_SaveSnapshot_Success()
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

        protected override void SetupObjects()
        {
            Updater.CreateCurrencies(ObjectSpace);
            SetOfBooks.GetInstance(ObjectSpace);  
            Updater.InitSetOfBooks(ObjectSpace);
        }
    }
}
