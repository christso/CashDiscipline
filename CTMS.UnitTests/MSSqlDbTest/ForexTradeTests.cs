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
using DevExpress.Persistent.Base;

namespace CTMS.UnitTests.MSSqlDbTest
{
    [TestFixture]
    public class ForexTradeTests : MSSqlDbTestBase
    {

        private class ForexTradeTestInclude
        {
            private ForexTradeTests _XafTest;

            public Currency CcyAud;

            public ForexTradeTestInclude(ForexTradeTests xafTest)
            {
                _XafTest = xafTest;
                CcyAud = _XafTest.ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            }
        }

        [Test]
        public void ForexTrade_UploadToCashFlow_IsCorrect()
        {
            #region Prepare 1

            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));

            // create counterparty
            var fxCounterparty = ObjectSpace.CreateObject<ForexCounterparty>();
            fxCounterparty.Name = "ANZ";
            var counterparty = ObjectSpace.FindObject<Counterparty>(CriteriaOperator.Parse("Name = ?", "ANZ"));
            if (counterparty == null)
            {
                counterparty = ObjectSpace.CreateObject<Counterparty>();
                counterparty.Name = "ANZ";
            }
            fxCounterparty.CashFlowCounterparty = counterparty;

            // create accounts
            var usdAccount = ObjectSpace.CreateObject<Account>();
            usdAccount.Name = "VHA ANZ USD";
            usdAccount.Currency = ccyUSD;

            var audAccount = ObjectSpace.CreateObject<Account>();
            audAccount.Name = "VHA ANZ AUD";
            audAccount.Currency = ccyAUD;

            // create standard settlement accounts
            var usdSsa = ObjectSpace.CreateObject<ForexStdSettleAccount>();
            usdSsa.Account = usdAccount;
            usdSsa.Counterparty = fxCounterparty;
            usdSsa.Currency = ccyUSD;

            var audSsa = ObjectSpace.CreateObject<ForexStdSettleAccount>();
            audSsa.Account = audAccount;
            audSsa.Counterparty = fxCounterparty;
            audSsa.Currency = ccyAUD;

            ObjectSpace.CommitChanges();

            #endregion

            #region Prepare Forex Trade
            // create forex trade 1
            var ft1 = ObjectSpace.CreateObject<ForexTrade>();
            ft1.CalculateEnabled = true;
            ft1.SettleGroupId = 1;
            ft1.ValueDate = new DateTime(2013, 12, 31);
            ft1.PrimaryCcy = ccyAUD;
            ft1.CounterCcy = ccyUSD;
            ft1.Rate = 0.9M;
            ft1.CounterCcyAmt = 1000;
            ft1.Counterparty = fxCounterparty;

            // create forex trade 2
            var ft2 = ObjectSpace.CreateObject<ForexTrade>();
            ft2.CalculateEnabled = true;
            ft2.SettleGroupId = 1;
            ft2.ValueDate = new DateTime(2013, 12, 31);
            ft2.PrimaryCcy = ccyAUD;
            ft2.CounterCcy = ccyUSD;
            ft2.Rate = 0.95M;
            ft2.CounterCcyAmt = 2000;
            ft2.Counterparty = fxCounterparty;
            ft2.CounterSettleAccount = usdAccount;
            ft2.PrimarySettleAccount = audAccount;

            ObjectSpace.CommitChanges();
            #endregion

            ForexTrade.UploadToCashFlowForecast(((XPObjectSpace)ObjectSpace).Session);

            Assert.AreEqual(ft1.PrimaryCashFlow, ft2.PrimaryCashFlow);
            Assert.AreEqual(ft1.CounterCashFlow, ft2.CounterCashFlow);
            
            var cashFlows = ObjectSpace.GetObjects<CashFlow>();
            var couCf1 = cashFlows.FirstOrDefault(x => x == ft1.CounterCashFlow);
            var priCf1 = cashFlows.FirstOrDefault(x => x == ft1.PrimaryCashFlow);
            Assert.AreEqual(-ft1.PrimaryCcyAmt - ft2.PrimaryCcyAmt, priCf1.AccountCcyAmt);
            Assert.AreEqual(ft1.CounterCcyAmt + ft2.CounterCcyAmt, couCf1.CounterCcyAmt);
        }

        [Test]
        [Category("Coverage_3")]
        public void ForexTrade_ChangeDate_CashFlowUpdated()
        {
            #region Prepare 1

            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));

            // create counterparty
            var fxCounterparty = ObjectSpace.CreateObject<ForexCounterparty>();
            fxCounterparty.Name = "ANZ";
            var counterparty = ObjectSpace.FindObject<Counterparty>(CriteriaOperator.Parse("Name = ?", "ANZ"));
            if (counterparty == null)
            {
                counterparty = ObjectSpace.CreateObject<Counterparty>();
                counterparty.Name = "ANZ";
            }
            fxCounterparty.CashFlowCounterparty = counterparty;

            // create accounts
            var usdAccount = ObjectSpace.CreateObject<Account>();
            usdAccount.Name = "VHA ANZ USD";
            usdAccount.Currency = ccyUSD;

            var audAccount = ObjectSpace.CreateObject<Account>();
            audAccount.Name = "VHA ANZ AUD";
            audAccount.Currency = ccyAUD;

            // create standard settlement accounts
            var usdSsa = ObjectSpace.CreateObject<ForexStdSettleAccount>();
            usdSsa.Account = usdAccount;
            usdSsa.Counterparty = fxCounterparty;
            usdSsa.Currency = ccyUSD;

            var audSsa = ObjectSpace.CreateObject<ForexStdSettleAccount>();
            audSsa.Account = audAccount;
            audSsa.Counterparty = fxCounterparty;
            audSsa.Currency = ccyAUD;

            ObjectSpace.CommitChanges();

            #endregion

            #region Prepare Forex Trade
            // create forex trade 1
            var ft1 = ObjectSpace.CreateObject<ForexTrade>();
            ft1.CalculateEnabled = true;
            ft1.ValueDate = new DateTime(2013, 12, 31);
            ft1.PrimaryCcy = ccyAUD;
            ft1.CounterCcy = ccyUSD;
            ft1.Rate = 0.9M;
            ft1.CounterCcyAmt = 1000;
            ft1.Counterparty = fxCounterparty;

            // create forex trade 2
            var ft2 = ObjectSpace.CreateObject<ForexTrade>();
            ft2.CalculateEnabled = true;
            ft2.ValueDate = new DateTime(2013, 12, 31);
            ft2.PrimaryCcy = ccyAUD;
            ft2.CounterCcy = ccyUSD;
            ft2.Rate = 0.95M;
            ft2.CounterCcyAmt = 2000;
            ft2.Counterparty = fxCounterparty;
            ft2.CounterSettleAccount = usdAccount;
            ft2.PrimarySettleAccount = audAccount;

            ObjectSpace.CommitChanges();
            #endregion

            #region Action
            ft1.ValueDate = new DateTime(2014, 01, 15);
            ObjectSpace.CommitChanges();
            #endregion

            #region Assert

            var cfs = ObjectSpace.GetObjects<CashFlow>();
            Assert.NotNull(cfs.FirstOrDefault(x => x.Oid == ft1.CounterCashFlow.Oid));
            Assert.NotNull(cfs.FirstOrDefault(x => x.Oid == ft1.PrimaryCashFlow.Oid));
            Assert.NotNull(cfs.FirstOrDefault(x => x.Oid == ft2.CounterCashFlow.Oid));
            Assert.NotNull(cfs.FirstOrDefault(x => x.Oid == ft2.PrimaryCashFlow.Oid));
            Assert.AreEqual(4, cfs.Count);
            #endregion
        }

        [Test]
        [Category("Coverage_2")]
        public void ForexTrade_CreateAndPredeliver_IsCalculatedAndValid()
        {

            #region Prepare
            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));

            // create counterparty
            var fxCounterparty = ObjectSpace.CreateObject<ForexCounterparty>();
            fxCounterparty.Name = "ANZ";
            var counterparty = ObjectSpace.FindObject<Counterparty>(CriteriaOperator.Parse("Name = ?", "ANZ"));
            if (counterparty == null)
            {
                counterparty = ObjectSpace.CreateObject<Counterparty>();
                counterparty.Name = "ANZ";
            }
            fxCounterparty.CashFlowCounterparty = counterparty;

            // create accounts
            var usdAccount = ObjectSpace.CreateObject<Account>();
            usdAccount.Name = "VHA ANZ USD";
            usdAccount.Currency = ccyUSD;

            var audAccount = ObjectSpace.CreateObject<Account>();
            audAccount.Name = "VHA ANZ AUD";
            audAccount.Currency = ccyAUD;

            // create standard settlement accounts
            var usdSsa = ObjectSpace.CreateObject<ForexStdSettleAccount>();
            usdSsa.Account = usdAccount;
            usdSsa.Counterparty = fxCounterparty;
            usdSsa.Currency = ccyUSD;

            var audSsa = ObjectSpace.CreateObject<ForexStdSettleAccount>();
            audSsa.Account = audAccount;
            audSsa.Counterparty = fxCounterparty;
            audSsa.Currency = ccyAUD;

            ObjectSpace.CommitChanges();

            #endregion

            #region Forex Trade
            // create forex trade 1
            var ft1 = ObjectSpace.CreateObject<ForexTrade>();
            ft1.CalculateEnabled = true;
            ft1.ValueDate = new DateTime(2013, 12, 31);
            ft1.PrimaryCcy = ccyAUD;
            ft1.CounterCcy = ccyUSD;
            ft1.Rate = 0.9M;
            ft1.CounterCcyAmt = 1000;
            ft1.Counterparty = fxCounterparty;

            ObjectSpace.CommitChanges();

            RuleSetValidationResult result = Validator.RuleSet.ValidateTarget(ObjectSpace, ft1, DefaultContexts.Save);
            Assert.AreEqual(ValidationState.Valid, result.State);

            Assert.AreEqual(audAccount, ft1.PrimarySettleAccount);
            Assert.AreEqual(usdAccount, ft1.CounterSettleAccount);

            decimal targetPrimaryCcyAmt = Math.Round(1000 / 0.9M, 2);
            Assert.AreEqual(targetPrimaryCcyAmt, ft1.PrimaryCcyAmt);

            // assert cash flow
            var cfs = ObjectSpace.GetObjects<CashFlow>(null);
            var couCf = cfs.FirstOrDefault(x => x.Oid == ft1.CounterCashFlow.Oid);
            var priCf = cfs.FirstOrDefault(x => x.Oid == ft1.PrimaryCashFlow.Oid);
            Assert.AreEqual(targetPrimaryCcyAmt, couCf.FunctionalCcyAmt);
            Assert.AreEqual(ft1.PrimaryCcyAmt, couCf.FunctionalCcyAmt);
            Assert.AreEqual(ft1.CounterCcyAmt, couCf.AccountCcyAmt);
            Assert.AreEqual(ft1.CounterCcy, couCf.CounterCcy);

            Assert.AreEqual(-targetPrimaryCcyAmt, priCf.FunctionalCcyAmt);
            Assert.AreEqual(-ft1.CounterCcyAmt, priCf.CounterCcyAmt);
            Assert.AreEqual(-ft1.PrimaryCcyAmt, priCf.AccountCcyAmt);
            Assert.AreEqual(ft1.CounterCcy, priCf.CounterCcy);
            #endregion

            #region Predelivery 1

            decimal pdCounterCcyAmt1 = 1000;
            DateTime pdValueDate1 = new DateTime(2013, 11, 30);
            decimal pdRate1 = 0.92M;
            decimal pdPrimaryCcyAmt1 = Math.Round(pdCounterCcyAmt1 / pdRate1, 2);
            var pdy1 = ft1.Predeliver(pdCounterCcyAmt1, pdValueDate1, pdRate1);

            ObjectSpace.CommitChanges();

            Assert.AreEqual(ft1, pdy1.FromForexTrade);

            Assert.AreEqual(pdCounterCcyAmt1, pdy1.ToForexTrade.CounterCcyAmt);
            Assert.AreEqual(pdRate1, pdy1.ToForexTrade.Rate);
            Assert.AreEqual(pdPrimaryCcyAmt1, pdy1.ToForexTrade.PrimaryCcyAmt);
            Assert.AreEqual(pdValueDate1, pdy1.ToForexTrade.ValueDate);

            Assert.AreEqual(-pdCounterCcyAmt1, pdy1.AmendForexTrade.CounterCcyAmt);
            Assert.AreEqual(ft1.Rate, pdy1.AmendForexTrade.Rate);
            Assert.AreEqual(Math.Round(-pdCounterCcyAmt1 / ft1.Rate, 2), pdy1.AmendForexTrade.PrimaryCcyAmt);

            var cfs2 = ObjectSpace.GetObjects<CashFlow>();
            var couCf2 = cfs2.FirstOrDefault(x => pdy1.ToForexTrade.CounterCashFlow == x);
            var priCf2 = cfs2.FirstOrDefault(x => pdy1.ToForexTrade.PrimaryCashFlow == x);
            
            Assert.AreEqual(pdPrimaryCcyAmt1, couCf2.FunctionalCcyAmt);
            Assert.AreEqual(-pdPrimaryCcyAmt1, priCf2.FunctionalCcyAmt);

            #endregion
            //decimal pdCounterCcyAmt2 = 400;
            //DateTime pdValueDate2 = new DateTime(2013, 11, 30);
            //decimal pdRate2 = 0.92M;
            //decimal pdPrimaryCcyAmt2 = Math.Round(pdCounterCcyAmt2 / pdRate2, 2);
            //var pdy2 = ft1.Predeliver(pdCounterCcyAmt2, pdValueDate2, pdRate2);

            //ObjectSpace.CommitChanges();

            //Assert.AreEqual(ft1, pdy2.FromForexTrade);

            //Assert.AreEqual(pdCounterCcyAmt2, pdy2.ToForexTrade.CounterCcyAmt);
            //Assert.AreEqual(pdRate2, pdy2.ToForexTrade.Rate);
            //Assert.AreEqual(pdPrimaryCcyAmt2, pdy2.ToForexTrade.PrimaryCcyAmt);
            //Assert.AreEqual(pdValueDate2, pdy2.ToForexTrade.ValueDate);

            //Assert.AreEqual(-pdCounterCcyAmt2, pdy2.AmendForexTrade.CounterCcyAmt);
            //Assert.AreEqual(ft1.Rate, pdy2.AmendForexTrade.Rate);
            //Assert.AreEqual(Math.Round(-pdCounterCcyAmt2 / ft1.Rate, 2), pdy2.AmendForexTrade.PrimaryCcyAmt);

            #region Predelivery 2

            #endregion
        }

        [Test]
        [Category("Coverage_1")]
        public void ForexTrade_Clone_CashFlowIsCorrect()
        {
            #region Prepare
            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));

            // create counterparty
            var fxCounterparty = ObjectSpace.CreateObject<ForexCounterparty>();
            fxCounterparty.Name = "ANZ";
            var counterparty = ObjectSpace.FindObject<Counterparty>(CriteriaOperator.Parse("Name = ?", "ANZ"));
            if (counterparty == null)
            {
                counterparty = ObjectSpace.CreateObject<Counterparty>();
                counterparty.Name = "ANZ";
            }
            fxCounterparty.CashFlowCounterparty = counterparty;

            // create accounts
            var usdAccount = ObjectSpace.CreateObject<Account>();
            usdAccount.Name = "VHA ANZ USD";
            usdAccount.Currency = ccyUSD;

            var audAccount = ObjectSpace.CreateObject<Account>();
            audAccount.Name = "VHA ANZ AUD";
            audAccount.Currency = ccyAUD;

            // create standard settlement accounts
            var usdSsa = ObjectSpace.CreateObject<ForexStdSettleAccount>();
            usdSsa.Account = usdAccount;
            usdSsa.Counterparty = fxCounterparty;
            usdSsa.Currency = ccyUSD;

            var audSsa = ObjectSpace.CreateObject<ForexStdSettleAccount>();
            audSsa.Account = audAccount;
            audSsa.Counterparty = fxCounterparty;
            audSsa.Currency = ccyAUD;

            ObjectSpace.CommitChanges();

            #endregion

            #region Forex Trade
            // create forex trade 1
            var ft1 = ObjectSpace.CreateObject<ForexTrade>();
            //ft1.SettleGroupId = 1;
            ft1.ValueDate = new DateTime(2013, 12, 31);
            ft1.PrimaryCcy = ccyAUD;
            ft1.CounterCcy = ccyUSD;
            ft1.Rate = 0.9M;
            ft1.CounterCcyAmt = 1000;
            ft1.Counterparty = fxCounterparty;

            ObjectSpace.CommitChanges();

            #endregion

            #region Clone
            Cloner cloner = new Cloner();
            var ft2 = (ForexTrade)cloner.CloneTo(ft1, typeof(ForexTrade));
            ObjectSpace.CommitChanges();
            Assert.AreNotEqual(ft1.Oid, ft2.Oid);
            Assert.AreNotEqual(ft1.SequentialNumber, ft2.SequentialNumber);
            Assert.AreEqual(ft1.ValueDate, ft2.ValueDate);
            Assert.AreEqual(ft1.CounterCashFlow, ft2.CounterCashFlow);
            Assert.AreEqual(ft1.PrimaryCashFlow, ft2.PrimaryCashFlow);
            #endregion
        }

        [Test]
        [Category("Coverage_1")]
        public void ForexTrade_NoValueDate_ExceptionThrown()
        {
            var ccyAUD = ObjectSpace.CreateObject<Currency>();
            var ccyUSD = ObjectSpace.CreateObject<Currency>();

            // create counterparty
            var fxCounterparty = ObjectSpace.CreateObject<ForexCounterparty>();

            var ft = ObjectSpace.CreateObject<ForexTrade>();

            ft.CalculateEnabled = true;
            ft.PrimaryCcy = ccyAUD;
            ft.CounterCcy = ccyUSD;
            ft.Rate = 0.9M;
            ft.CounterCcyAmt = 1000;
            ft.Counterparty = fxCounterparty;
            ft.ValueDate = default(DateTime);

            RuleSetValidationResult result = Validator.RuleSet.ValidateTarget(ObjectSpace, ft, DefaultContexts.Save);
            Assert.AreEqual(ValidationState.Invalid, result.State);
        }

        [Test]
        [Category("Coverage_1")]
        public void ForexTrade_NoCounterparty_ExceptionThrown()
        {
            var ccyAUD = ObjectSpace.CreateObject<Currency>();
            var ccyUSD = ObjectSpace.CreateObject<Currency>();

            // create counterparty

            var ft = ObjectSpace.CreateObject<ForexTrade>();

            ft.CalculateEnabled = true;
            ft.PrimaryCcy = ccyAUD;
            ft.CounterCcy = ccyUSD;
            ft.Rate = 0.9M;
            ft.CounterCcyAmt = 1000;
            ft.ValueDate = DateTime.Now;

            RuleSetValidationResult result = Validator.RuleSet.ValidateTarget(ObjectSpace, ft, DefaultContexts.Save);
            Assert.AreEqual(ValidationState.Invalid, result.State);
        }

        [Test]
        [Category("ForexTradeToCashFlow")]
        [Category("Coverage_1")]
        public void ForexTrade_Create_IsCombinedInCashFlow()
        {
            #region Prepare
            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));

            // create counterparty
            var fxCounterparty = ObjectSpace.CreateObject<ForexCounterparty>();
            fxCounterparty.Name = "ANZ";
            var counterparty = ObjectSpace.FindObject<Counterparty>(CriteriaOperator.Parse("Name = ?", "ANZ"));
            if (counterparty == null)
            {
                counterparty = ObjectSpace.CreateObject<Counterparty>();
                counterparty.Name = "ANZ";
            }
            fxCounterparty.CashFlowCounterparty = counterparty;

            // create accounts

            var usdAccount = ObjectSpace.CreateObject<Account>();
            usdAccount.Name = "VHA ANZ USD";
            usdAccount.Currency = ccyUSD;

            var audAccount = ObjectSpace.CreateObject<Account>();
            audAccount.Name = "VHA ANZ AUD";
            audAccount.Currency = ccyAUD;

            #endregion

            #region Action
            // create forex trade 1
            var ft1 = ObjectSpace.CreateObject<ForexTrade>();
            ft1.CalculateEnabled = true;
            ft1.ValueDate = new DateTime(2013, 12, 31);
            ft1.PrimaryCcy = ccyAUD;
            ft1.CounterCcy = ccyUSD;
            ft1.Rate = 0.9M;
            ft1.CounterCcyAmt = 1000;
            ft1.Counterparty = fxCounterparty;
            ft1.CounterSettleAccount = usdAccount;
            ft1.PrimarySettleAccount = audAccount;
            ft1.SettleGroupId = 1;

            // create forex trade 2
            var ft2 = ObjectSpace.CreateObject<ForexTrade>();
            ft2.CalculateEnabled = true;
            ft2.ValueDate = new DateTime(2013, 12, 31);
            ft2.PrimaryCcy = ccyAUD;
            ft2.CounterCcy = ccyUSD;
            ft2.Rate = 0.95M;
            ft2.CounterCcyAmt = 2000;
            ft2.Counterparty = fxCounterparty;
            ft2.CounterSettleAccount = usdAccount;
            ft2.PrimarySettleAccount = audAccount;
            ft2.SettleGroupId = 1;

            ObjectSpace.CommitChanges();
            #endregion

            #region Assert
            var cfs = ObjectSpace.GetObjects<CashFlow>();
            Assert.AreEqual(2, cfs.Count);
            
            CashFlow couCf1 = ft1.CounterCashFlow;
            CashFlow priCf1 = ft1.PrimaryCashFlow;
            CashFlow couCf2 = ft2.CounterCashFlow;
            CashFlow priCf2 = ft2.PrimaryCashFlow;

            Assert.AreEqual(couCf1, couCf2);
            Assert.AreEqual(priCf1, priCf2);
            #endregion
        }

        /// <summary>
        /// Test if deletion of Forex Trade will reduce amount of Cash Flow
        /// </summary>
        [Test]
        [Category("ForexTradeToCashFlow")]
        [Category("Coverage_3")]
        public void ForexTrade_Delete_CashFlowIsDecremented()
        {
            #region Prepare
            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));

            // create counterparty
            var fxCounterparty = ObjectSpace.CreateObject<ForexCounterparty>();
            fxCounterparty.Name = "ANZ";
            var counterparty = ObjectSpace.FindObject<Counterparty>(CriteriaOperator.Parse("Name = ?", "ANZ"));
            if (counterparty == null)
            {
                counterparty = ObjectSpace.CreateObject<Counterparty>();
                counterparty.Name = "ANZ";
            }
            fxCounterparty.CashFlowCounterparty = counterparty;

            // create accounts
            
            var usdAccount = ObjectSpace.CreateObject<Account>();
            usdAccount.Name = "VHA ANZ USD";
            usdAccount.Currency = ccyUSD;

            var audAccount = ObjectSpace.CreateObject<Account>();
            audAccount.Name = "VHA ANZ AUD";
            audAccount.Currency = ccyAUD;

            // create forex trade 1
            var ft1 = ObjectSpace.CreateObject<ForexTrade>();
            ft1.CalculateEnabled = true;
            ft1.ValueDate = new DateTime(2013, 12, 31);
            ft1.PrimaryCcy = ccyAUD;
            ft1.CounterCcy = ccyUSD;
            ft1.Rate = 0.9M;
            ft1.CounterCcyAmt = 1000;
            ft1.Counterparty = fxCounterparty;
            ft1.CounterSettleAccount = usdAccount;
            ft1.PrimarySettleAccount = audAccount;

            // create forex trade 2
            var ft2 = ObjectSpace.CreateObject<ForexTrade>();
            ft2.CalculateEnabled = true;
            ft2.ValueDate = new DateTime(2013, 12, 31);
            ft2.PrimaryCcy = ccyAUD;
            ft2.CounterCcy = ccyUSD;
            ft2.Rate = 0.95M;
            ft2.CounterCcyAmt = 2000;
            ft2.Counterparty = fxCounterparty;
            ft2.CounterSettleAccount = usdAccount;
            ft2.PrimarySettleAccount = audAccount;
            ObjectSpace.CommitChanges();
            #endregion

            #region Action
            ft1.Delete();
            ObjectSpace.CommitChanges();

            var objSpace = ObjectSpace;
            objSpace.Refresh();
            #endregion

            #region Assert
            
            var cashFlows = objSpace.GetObjects<CashFlow>();
            
            Assert.AreEqual(2, cashFlows.Count);
            Assert.AreEqual(Math.Round(2000 - 2000/0.95M, 2), 
                Math.Round(cashFlows.Sum(x => x.AccountCcyAmt), 2));
            Assert.AreEqual(0, cashFlows.Sum(x => x.FunctionalCcyAmt));
            Assert.AreEqual(0, cashFlows.Sum(x => x.CounterCcyAmt));
            #endregion
        }

        [Test]
        [Category("ForexTradeToCashFlow")]
        [Category("Coverage_3")]
        public void ForexTrade_Delete_CashFlowIsDeleted()
        {
            #region Prepare
            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));
            var ccyUSD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "USD"));

            // create counterparty
            var fxCounterparty = ObjectSpace.CreateObject<ForexCounterparty>();
            fxCounterparty.Name = "ANZ";
            var counterparty = ObjectSpace.FindObject<Counterparty>(CriteriaOperator.Parse("Name = ?", "ANZ"));
            if (counterparty == null)
            {
                counterparty = ObjectSpace.CreateObject<Counterparty>();
                counterparty.Name = "ANZ";
            }
            fxCounterparty.CashFlowCounterparty = counterparty;

            // create accounts

            var usdAccount = ObjectSpace.CreateObject<Account>();
            usdAccount.Name = "VHA ANZ USD";
            usdAccount.Currency = ccyUSD;

            var audAccount = ObjectSpace.CreateObject<Account>();
            audAccount.Name = "VHA ANZ AUD";
            audAccount.Currency = ccyAUD;

            // create forex trade 1
            var ft1 = ObjectSpace.CreateObject<ForexTrade>();
            ft1.CalculateEnabled = true;
            ft1.ValueDate = new DateTime(2013, 12, 31);
            ft1.PrimaryCcy = ccyAUD;
            ft1.CounterCcy = ccyUSD;
            ft1.Rate = 0.9M;
            ft1.CounterCcyAmt = 1000;
            ft1.Counterparty = fxCounterparty;
            ft1.CounterSettleAccount = usdAccount;
            ft1.PrimarySettleAccount = audAccount;
            ft1.Save();
            ObjectSpace.CommitChanges();

            #endregion

            #region Action
            ft1.Delete();
            ft1.Save();
            ObjectSpace.CommitChanges();

            var objSpace = ObjectSpace;
            objSpace.Refresh();
            #endregion

            #region Assert

            var cashFlows = objSpace.GetObjects<CashFlow>();
            Assert.AreEqual(0, cashFlows.Count);
            #endregion
        }

        /// <summary>
        /// Cash Flows with an Account.Currency that equals the Functional Currency 
        /// will have ForexLinkIsClosed property set to TRUE
        /// </summary>
        [Test]
        [Category("Coverage_1")]
        public void CashFlow_AccountCurrencyIsFunctional_ForexLinkIsClosed()
        {
            var ccyAUD = ObjectSpace.FindObject<Currency>(CriteriaOperator.Parse("Name = ?", "AUD"));

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ 70086";
            account.Currency = ccyAUD;

            var cfIn1 = ObjectSpace.CreateObject<CashFlow>();
            cfIn1.TranDate = new DateTime(2012, 06, 01);
            cfIn1.Account = account;
            cfIn1.AccountCcyAmt = 2000000;
            cfIn1.Save();

            Assert.AreEqual(true, cfIn1.ForexLinkIsClosed);
        }


        protected override void SetupObjects()
        {
            CTMS.Module.DatabaseUpdate.Updater.CreateCurrencies(ObjectSpace);
            CTMS.Module.DatabaseUpdate.Updater.InitSetOfBooks(ObjectSpace);
        }
    }
}
