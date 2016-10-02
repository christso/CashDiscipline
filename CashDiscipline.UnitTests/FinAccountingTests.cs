using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Utils;

using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.FinAccounting;

using CashDiscipline.Module.ParamObjects.FinAccounting;
using CashDiscipline.Module.BusinessObjects;
using DevExpress.Persistent.Validation;
using CashDiscipline.Module;
using CashDiscipline.Module.Controllers.Cash;


using System.Diagnostics;
using System.Data.SqlClient;
using CashDiscipline.Module.Controllers.FinAccounting;
using DevExpress.Xpo.DB;
using CashDiscipline.Module.Logic;
using CashDiscipline.Module.DatabaseUpdate;
using CashDiscipline.Module.Logic.FinAccounting;
using Xafology.TestUtils;

namespace CashDiscipline.UnitTests
{
    [TestFixture]
    public class FinAccountingTests : TestBase
    {
        #region Setup
        public FinAccountingTests()
        {
            SetTesterDbType(TesterDbType.MsSql);

            var tester = Tester as MSSqlDbTestBase;
            if (tester != null)
                tester.DatabaseName = Constants.TestDbName;
        }


        public override void OnSetup()
        {
            Updater.CreateCurrencies(ObjectSpace);
            Updater.CreateFinAccountingDefaults(ObjectSpace);
            Updater.InitSetOfBooks(ObjectSpace);
        }
        public override void OnAddExportedTypes(ModuleBase module)
        {
            CashDisciplineTestHelper.AddExportedTypes(module);
        }
        #endregion

        // Example of how you should use a SubString mapping to avoid throwing an error
        public void GenerateJournals_OrmSubStringOutOfRange()
        {
            #region Prepare
            var journalGroup = ObjectSpace.CreateObject<FinJournalGroup>();
            journalGroup.Name = "VF Bank";

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ 70086";
            var currency = ObjectSpace.CreateObject<Currency>();
            currency.Name = "AUD";

            var glDescDateFormat = "dd-mmm-yy";
            var bankGlAccount = "210127";

            var stmtSource = ObjectSpace.GetObjectByKey<CashFlowSource>(SetOfBooks.CachedInstance.BankStmtCashFlowSource.Oid);

            #endregion

            #region Cash Flow Mapping

            var finAccount = ObjectSpace.CreateObject<FinAccount>();
            finAccount.Account = account;
            finAccount.GlAccount = bankGlAccount;
            finAccount.JournalGroup = journalGroup;

            var activity = ObjectSpace.CreateObject<Activity>();
            activity.Name = "V AMEX Rcpt";
            var amexGlAccount = "211900";

            var finActivity1 = ObjectSpace.CreateObject<FinActivity>();
            finActivity1.FromActivity = activity;
            finActivity1.ToActivity = activity;
            finActivity1.FunctionalCcyAmtExpr = "IIf(Len(TranDescription) > 80, SubString(TranDescription, 71, 9), {FA})";
            finActivity1.GlDescription = "SUMMARY POSTING - AMEX";
            finActivity1.GlDescDateFormat = glDescDateFormat;
            finActivity1.GlAccount = amexGlAccount;
            finActivity1.JournalGroup = journalGroup;
            finActivity1.TargetObject = FinJournalTargetObject.BankStmt;
            finActivity1.Algorithm = FinMapAlgorithmType.ORM;

            #endregion

            #region Transactions

            var bankStmt1 = ObjectSpace.CreateObject<BankStmt>();
            bankStmt1.TranDate = new DateTime(2016, 07, 01);
            bankStmt1.Account = account;
            bankStmt1.Activity = activity;
            bankStmt1.TranAmount = 1793881.2M;
            bankStmt1.TranDescription = "TRANSFER                                VHA AMEX DISPERSAL FROM PYC DISPERSAL";
            bankStmt1.CounterCcyAmt = bankStmt1.TranAmount;
            bankStmt1.FunctionalCcyAmt = bankStmt1.TranAmount;
            bankStmt1.CounterCcy = currency;

            var cashFlow1 = ObjectSpace.CreateObject<CashFlow>();
            cashFlow1.TranDate = bankStmt1.TranDate;
            cashFlow1.Account = account;
            cashFlow1.Activity = bankStmt1.Activity;
            cashFlow1.AccountCcyAmt = bankStmt1.TranAmount;
            cashFlow1.Source = stmtSource;
            cashFlow1.Status = CashFlowStatus.Actual;

            #endregion

            #region Generate Journals
            // Params
            var glParam = ObjectSpace.CreateObject<FinGenJournalParam>();
            glParam.FromDate = new DateTime(2016, 01, 01);
            glParam.ToDate = new DateTime(2016, 12, 31);
            var journalGroupParam = ObjectSpace.CreateObject<FinJournalGroupParam>();
            journalGroupParam.JournalGroup = journalGroup;

            ObjectSpace.CommitChanges();

            journalGroupParam.GenJournalParam = glParam;

            var jg = new ParamJournalGenerator(glParam, ObjectSpace);
            jg.Execute();
            ObjectSpace.CommitChanges();

            #endregion

            #region

            var gls = ObjectSpace.GetObjects<GenLedger>();

            decimal bankStmtResult = gls.Where(x => x.GlAccount == bankGlAccount && x.SrcBankStmt != null).Sum(x => x.FunctionalCcyAmt);
            bankStmtResult = Math.Round(bankStmtResult, 2);
            Assert.AreEqual(bankStmt1.TranAmount, bankStmtResult);

            decimal cashFlowResult = gls.Where(x => x.GlAccount == bankGlAccount && x.SrcCashFlow != null).Sum(x => x.FunctionalCcyAmt);
            cashFlowResult = Math.Round(cashFlowResult, 2);
            Assert.AreEqual(0, cashFlowResult);

            #endregion
        }

        [TestCase(FinMapAlgorithmType.SQL)]
        [TestCase(FinMapAlgorithmType.ORM)]
        public void GenerateJournals_Decimal(FinMapAlgorithmType algoType)
        {
            #region Prepare
            var journalGroup = ObjectSpace.CreateObject<FinJournalGroup>();
            journalGroup.Name = "VF Bank";

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ 70086";
            var currency = ObjectSpace.CreateObject<Currency>();
            currency.Name = "AUD";

            var glDescDateFormat = "dd-mmm-yy";
            var bankGlAccount = "210127";

            var stmtSource = ObjectSpace.GetObjectByKey<CashFlowSource>(SetOfBooks.CachedInstance.BankStmtCashFlowSource.Oid);

            #endregion

            #region Cash Flow Mapping

            var finAccount = ObjectSpace.CreateObject<FinAccount>();
            finAccount.Account = account;
            finAccount.GlAccount = bankGlAccount;
            finAccount.JournalGroup = journalGroup;

            var activity = ObjectSpace.CreateObject<Activity>();
            activity.Name = "ANZ BPAY Txn Fee Pymt";
            var bankFeeGlAccount = "691090";

            var finActivity1 = ObjectSpace.CreateObject<FinActivity>();
            finActivity1.FromActivity = activity;
            finActivity1.ToActivity = activity;
            finActivity1.FunctionalCcyAmtExpr = "{FA} * 10/11";
            finActivity1.GlDescription = "BPAY TXN Fees";
            finActivity1.GlDescDateFormat = glDescDateFormat;
            finActivity1.GlAccount = bankFeeGlAccount;
            finActivity1.JournalGroup = journalGroup;
            finActivity1.TargetObject = FinJournalTargetObject.All;
            finActivity1.Algorithm = algoType;

            var finActivity2 = ObjectSpace.CreateObject<FinActivity>();
            finActivity2.FromActivity = activity;
            finActivity2.ToActivity = activity;
            finActivity2.FunctionalCcyAmtExpr = "{FA} * 1/11";
            finActivity2.GlDescription = "GST on BPAY TXN Fees";
            finActivity2.GlDescDateFormat = glDescDateFormat;
            finActivity2.GlAccount = bankFeeGlAccount;
            finActivity2.JournalGroup = journalGroup;
            finActivity2.TargetObject = FinJournalTargetObject.All;
            finActivity2.Algorithm = algoType;

            #endregion

            #region Transactions

            var bankStmt1 = ObjectSpace.CreateObject<BankStmt>();
            bankStmt1.TranDate = new DateTime(2016, 07, 01);
            bankStmt1.Account = account;
            bankStmt1.Activity = activity;
            bankStmt1.TranAmount = -250000;
            bankStmt1.TranDescription = "BPAY TXN FEES";
            bankStmt1.CounterCcyAmt = bankStmt1.TranAmount;
            bankStmt1.FunctionalCcyAmt = bankStmt1.TranAmount;
            bankStmt1.CounterCcy = currency;

            var cashFlow1 = ObjectSpace.CreateObject<CashFlow>();
            cashFlow1.TranDate = bankStmt1.TranDate;
            cashFlow1.Account = account;
            cashFlow1.Activity = bankStmt1.Activity;
            cashFlow1.AccountCcyAmt = bankStmt1.TranAmount;
            cashFlow1.Source = stmtSource;
            cashFlow1.Status = CashFlowStatus.Actual;

            #endregion

            #region Generate Journals
            // Params
            var glParam = ObjectSpace.CreateObject<FinGenJournalParam>();
            glParam.FromDate = new DateTime(2016, 01, 01);
            glParam.ToDate = new DateTime(2016, 12, 31);
            var journalGroupParam = ObjectSpace.CreateObject<FinJournalGroupParam>();
            journalGroupParam.JournalGroup = journalGroup;

            ObjectSpace.CommitChanges();

            journalGroupParam.GenJournalParam = glParam;

            var jg = new ParamJournalGenerator(glParam, ObjectSpace);
            jg.Execute();
            ObjectSpace.CommitChanges();

            #endregion

            #region

            var gls = ObjectSpace.GetObjects<GenLedger>();

            decimal bankStmtResult = gls.Where(x => x.GlAccount == bankGlAccount && x.SrcBankStmt != null).Sum(x => x.FunctionalCcyAmt);
            bankStmtResult = Math.Round(bankStmtResult);
            Assert.AreEqual(bankStmt1.TranAmount, bankStmtResult);

            decimal cashFlowResult = gls.Where(x => x.GlAccount == bankGlAccount && x.SrcCashFlow != null).Sum(x => x.FunctionalCcyAmt);
            cashFlowResult = Math.Round(cashFlowResult);
            Assert.AreEqual(0, cashFlowResult);

            #endregion
        }

        [TestCase(FinMapAlgorithmType.SQL)]
        [TestCase(FinMapAlgorithmType.ORM)]
        public void GenerateJournalsOnOverlappingSources(FinMapAlgorithmType algoType)
        {
            #region Prepare
            var journalGroup = ObjectSpace.CreateObject<FinJournalGroup>();
            journalGroup.Name = "VF Bank";

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ 94945";
            var currency = ObjectSpace.CreateObject<Currency>();
            currency.Name = "AUD";

            var glDescDateFormat = "dd-mmm-yy";
            var bankGlAccount = "210159";

            var stmtSource = ObjectSpace.GetObjectByKey<CashFlowSource>(SetOfBooks.CachedInstance.BankStmtCashFlowSource.Oid);

            #endregion

            #region Cash Flow Mapping

            var finAccount = ObjectSpace.CreateObject<FinAccount>();
            finAccount.Account = account;
            finAccount.GlAccount = bankGlAccount;
            finAccount.JournalGroup = journalGroup;

            var activity = ObjectSpace.CreateObject<Activity>();
            activity.Name = "Bad Debt Recovery Rcpt";
            var badDebtRecAccount = "211900";

            var finActivity1 = ObjectSpace.CreateObject<FinActivity>();
            finActivity1.FromActivity = activity;
            finActivity1.ToActivity = activity;
            finActivity1.FunctionalCcyAmtExpr = "{FA}";
            finActivity1.GlDescription = "Bank Fees";
            finActivity1.GlDescDateFormat = glDescDateFormat;
            finActivity1.GlAccount = badDebtRecAccount;
            finActivity1.JournalGroup = journalGroup;
            finActivity1.TargetObject = FinJournalTargetObject.All;
            finActivity1.Algorithm = algoType;

            #endregion

            #region Transactions

            var bankStmt1 = ObjectSpace.CreateObject<BankStmt>();
            bankStmt1.TranDate = new DateTime(2016,04,29);
            bankStmt1.Account = account;
            bankStmt1.Activity = activity;
            bankStmt1.TranAmount = 49113.67M;
            bankStmt1.TranDescription = "TRANSFER                                ADR29042016        FROM AUSTRALIAN DEBT";
            bankStmt1.CounterCcyAmt = bankStmt1.TranAmount;
            bankStmt1.FunctionalCcyAmt = bankStmt1.TranAmount;
            bankStmt1.CounterCcy = currency;

            var cashFlow1 = ObjectSpace.CreateObject<CashFlow>();
            cashFlow1.TranDate = bankStmt1.TranDate;
            cashFlow1.Account = account;
            cashFlow1.Activity = bankStmt1.Activity;
            cashFlow1.AccountCcyAmt = bankStmt1.TranAmount;
            cashFlow1.Source = stmtSource;
            cashFlow1.Status = CashFlowStatus.Actual;

            #endregion

            #region Generate Journals
            // Params
            var glParam = ObjectSpace.CreateObject<FinGenJournalParam>();
            glParam.FromDate = new DateTime(2016, 01, 01);
            glParam.ToDate = new DateTime(2016, 12, 31);
            var journalGroupParam = ObjectSpace.CreateObject<FinJournalGroupParam>();
            journalGroupParam.JournalGroup = journalGroup;

            ObjectSpace.CommitChanges();

            journalGroupParam.GenJournalParam = glParam;

            var jg = new ParamJournalGenerator(glParam, ObjectSpace);
            jg.Execute();
            ObjectSpace.CommitChanges();

            #endregion

            #region

            var gls = ObjectSpace.GetObjects<GenLedger>();

            Assert.AreEqual(bankStmt1.TranAmount,
                gls.Where(x => x.GlAccount == bankGlAccount && x.SrcBankStmt != null).Sum(x => x.FunctionalCcyAmt));
            Assert.AreEqual(0,
                gls.Where(x => x.GlAccount == bankGlAccount && x.SrcCashFlow != null).Sum(x => x.FunctionalCcyAmt));

            #endregion

        }

        //[TestCase(FinMapAlgorithmType.SQL)]
        [TestCase(FinMapAlgorithmType.ORM)]
        public void GenerateJournals_CashFlowReclass_MappedToJournals(FinMapAlgorithmType algoType)
        {
            ObjectSpace.CommitChanges();
            //ObjectSpace.Session.PurgeDeletedObjects();
            #region Prepare
            var journalGroup = ObjectSpace.CreateObject<FinJournalGroup>();
            journalGroup.Name = "VHF Bank";

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHF HSBC AUD";
            var currency = ObjectSpace.CreateObject<Currency>();
            currency.Name = "AUD";

            var glDescDateFormat = "dd-mmm-yy";

            var bankGlAccount = "210127";

            var stmtSource = ObjectSpace.GetObjectByKey<CashFlowSource>(SetOfBooks.CachedInstance.BankStmtCashFlowSource.Oid);
            var reclassSource = ObjectSpace.CreateObject<CashFlowSource>();
            reclassSource.Name = "Reclass";
            #endregion

            #region Cash Flow Mapping

            #region Bank Account
            var finAccount = ObjectSpace.CreateObject<FinAccount>();
            finAccount.Account = account;
            finAccount.GlAccount = bankGlAccount;
            finAccount.JournalGroup = journalGroup;
            #endregion

            #region Bank Fees
            var bankFeeActivity = ObjectSpace.CreateObject<Activity>();
            bankFeeActivity.Name = "HSBC Bank Fee";
            var bankFeeGlAccount = "691090";

            var finActivity1 = ObjectSpace.CreateObject<FinActivity>();
            finActivity1.FromActivity = bankFeeActivity;
            finActivity1.ToActivity = bankFeeActivity;
            finActivity1.FunctionalCcyAmtExpr = "{FA}";
            finActivity1.GlDescription = "Bank Fees";
            finActivity1.GlDescDateFormat = glDescDateFormat;
            finActivity1.GlAccount = bankFeeGlAccount;
            finActivity1.JournalGroup = journalGroup;
            finActivity1.TargetObject = FinJournalTargetObject.All;
            finActivity1.Algorithm = algoType;
            #endregion

            #region Interest Expense
            var intExpActivity = ObjectSpace.CreateObject<Activity>();
            intExpActivity.Name = "IntExp CCS VG";
            var intExpGlAccount = "691090";

            var finActivity2 = ObjectSpace.CreateObject<FinActivity>();
            finActivity2.FromActivity = intExpActivity;
            finActivity2.ToActivity = intExpActivity;
            finActivity2.FunctionalCcyAmtExpr = "{FA}";
            finActivity2.GlDescription = "Interest Expense";
            finActivity2.GlDescDateFormat = glDescDateFormat;
            finActivity2.GlAccount = intExpGlAccount;
            finActivity2.JournalGroup = journalGroup;
            finActivity2.TargetObject = FinJournalTargetObject.All;
            finActivity2.Algorithm = algoType;
            #endregion

            #endregion

            #region Transactions
            var tranDate = new DateTime(2014, 03, 19);

            #region Bank Stmt
            var bankStmt = ObjectSpace.CreateObject<BankStmt>();
            bankStmt.TranDate = tranDate;
            bankStmt.Account = account;
            bankStmt.Activity = intExpActivity;
            bankStmt.TranAmount = 1944439.16M;
            bankStmt.CounterCcyAmt = bankStmt.TranAmount;
            bankStmt.FunctionalCcyAmt = bankStmt.TranAmount;
            bankStmt.CounterCcy = currency;
            #endregion

            #region Cash Flow
            var cashFlow1 = ObjectSpace.CreateObject<CashFlow>();
            cashFlow1.TranDate = tranDate;
            cashFlow1.Account = account;
            cashFlow1.Activity = intExpActivity;
            cashFlow1.AccountCcyAmt = bankStmt.TranAmount;
            cashFlow1.Source = stmtSource;
            cashFlow1.Status = CashFlowStatus.Actual;

            var cashFlow2 = ObjectSpace.CreateObject<CashFlow>();
            cashFlow2.TranDate = tranDate;
            cashFlow2.Account = account;
            cashFlow2.Activity = intExpActivity;
            cashFlow2.AccountCcyAmt = -10;
            cashFlow2.Source = reclassSource;
            cashFlow2.Status = CashFlowStatus.Actual;

            var cashFlow3 = ObjectSpace.CreateObject<CashFlow>();
            cashFlow3.TranDate = tranDate;
            cashFlow3.Account = account;
            cashFlow3.Activity = bankFeeActivity;
            cashFlow3.AccountCcyAmt = 10;
            cashFlow3.Source = reclassSource;
            cashFlow3.Status = CashFlowStatus.Actual;

            #endregion
            #endregion

            #region Generate Journals
            // Params
            var glParam = ObjectSpace.CreateObject<FinGenJournalParam>();
            glParam.FromDate = tranDate;
            glParam.ToDate = tranDate;
            var journalGroupParam = ObjectSpace.CreateObject<FinJournalGroupParam>();
            journalGroupParam.JournalGroup = journalGroup;

            ObjectSpace.CommitChanges();

            journalGroupParam.GenJournalParam = glParam;

            var jg = new ParamJournalGenerator(glParam, ObjectSpace);
            jg.Execute();
            ObjectSpace.CommitChanges();
            #endregion

            #region Asserts
            var gls = ObjectSpace.GetObjects<GenLedger>();
            Assert.AreEqual(6, gls.Count);
            decimal glAmount;
            glAmount = gls.Where(x => x.Activity == intExpActivity & x.GlAccount == bankGlAccount).Sum(x => x.FunctionalCcyAmt);
            Assert.AreEqual(bankStmt.FunctionalCcyAmt + cashFlow2.FunctionalCcyAmt, glAmount);

            glAmount = gls.Where(x => x.Activity == intExpActivity & x.GlAccount == intExpGlAccount).Sum(x => x.FunctionalCcyAmt);
            Assert.AreEqual(-(bankStmt.FunctionalCcyAmt + cashFlow2.FunctionalCcyAmt), glAmount);

            #endregion
        }

        [Test]
        public void GenerateJournals_AustPostSettle_MappedToJournals()
        {
            #region Prepare
            var journalGroup = ObjectSpace.CreateObject<FinJournalGroup>();
            journalGroup.Name = "VF Bank";

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ 94937";
            var currency = ObjectSpace.CreateObject<Currency>();
            currency.Name = "AUD";

            var glDescDateFormat = "dd-mmm-yy";

            var bankGlAccount = "210159";

            #endregion

            #region Cash Flow Mapping

            #region Bank Account
            var finAccount = ObjectSpace.CreateObject<FinAccount>();
            finAccount.Account = account;
            finAccount.GlAccount = bankGlAccount;
            finAccount.JournalGroup = journalGroup;
            #endregion

            #region Gross Amount
            var grossActivity = ObjectSpace.CreateObject<Activity>();
            grossActivity.Name = "V AustPost Rcpt";
            var grossGlAccount = "211900";

            var finActivity3 = ObjectSpace.CreateObject<FinActivity>();
            finActivity3.FromActivity = grossActivity;
            finActivity3.ToActivity = grossActivity;
            finActivity3.FunctionalCcyAmtExpr = "AustPostSettles.Sum(GrossAmount)";
            finActivity3.GlDescription = "SUMMARY POSTING - AUSTRALIA POST";
            finActivity3.GlDescDateFormat = glDescDateFormat;
            finActivity3.GlAccount = grossGlAccount;
            finActivity3.JournalGroup = journalGroup;
            finActivity3.TargetObject = FinJournalTargetObject.BankStmt;
            #endregion

            #region Commission
            var commActivity = ObjectSpace.CreateObject<Activity>();
            commActivity.Name = "V AustPost Comm";
            var commGlAccount = "691090";

            var finActivity1 = ObjectSpace.CreateObject<FinActivity>();
            finActivity1.FromActivity = grossActivity;
            finActivity1.ToActivity = commActivity;
            finActivity1.FunctionalCcyAmtExpr = "-AustPostSettles.Sum(NetCommission)";
            finActivity1.GlDescription = "AUSTRALIA POST FEES";
            finActivity1.GlDescDateFormat = glDescDateFormat;
            finActivity1.GlAccount = commGlAccount;
            finActivity1.JournalGroup = journalGroup;
            finActivity1.TargetObject = FinJournalTargetObject.BankStmt;
            #endregion

            #region Commission GST
            var gstActivity = ObjectSpace.CreateObject<Activity>();
            gstActivity.Name = "V AustPost Comm GST";
            var gstGlAccount = "235815";

            var finActivity2 = ObjectSpace.CreateObject<FinActivity>();
            finActivity2.FromActivity = grossActivity;
            finActivity2.ToActivity = gstActivity;
            finActivity2.FunctionalCcyAmtExpr = "-AustPostSettles.Sum(CommissionGST)";
            finActivity2.GlDescription = "GST ON AUSTRALIA POST FEES";
            finActivity2.GlDescDateFormat = glDescDateFormat;
            finActivity2.GlAccount = gstGlAccount;
            finActivity2.JournalGroup = journalGroup;
            finActivity2.TargetObject = FinJournalTargetObject.BankStmt;
            #endregion

            #region Dishonour Chequqe Fee
            var dsrFeeActivity = ObjectSpace.CreateObject<Activity>();
            dsrFeeActivity.Name = "V AustPost Dsr Chq Fee";
            var dsrFeeGlAccount = "671701";

            var finActivity4 = ObjectSpace.CreateObject<FinActivity>();
            finActivity4.FromActivity = grossActivity;
            finActivity4.ToActivity = dsrFeeActivity;
            finActivity4.FunctionalCcyAmtExpr = "-AustPostSettles.Sum(DishonourChequeFee)";
            finActivity4.GlDescription = "DISHONOUR FEE - AUST POST";
            finActivity4.GlDescDateFormat = glDescDateFormat;
            finActivity4.GlAccount = dsrFeeGlAccount;
            finActivity4.JournalGroup = journalGroup;
            finActivity4.TargetObject = FinJournalTargetObject.BankStmt;
            #endregion

            #region DishonourChequeReversal
            var dsrRvslActivity = ObjectSpace.CreateObject<Activity>();
            dsrRvslActivity.Name = "V AustPost Dsr Chq Rvsl";
            var dsrRvslGlAccount = "211900";

            var finActivity5 = ObjectSpace.CreateObject<FinActivity>();
            finActivity5.FromActivity = grossActivity;
            finActivity5.ToActivity = dsrRvslActivity;
            finActivity5.FunctionalCcyAmtExpr = "-AustPostSettles.Sum(DishonourChequeReversal)";
            finActivity5.GlDescription = "SUMMARY POSTING - AUSTRALIA POST";
            finActivity5.GlDescDateFormat = glDescDateFormat;
            finActivity5.GlAccount = dsrRvslGlAccount;
            finActivity5.JournalGroup = journalGroup;
            finActivity5.TargetObject = FinJournalTargetObject.BankStmt;
            #endregion

            #region NegativeCorrections
            var negActivity = ObjectSpace.CreateObject<Activity>();
            negActivity.Name = "V AustPost Neg Corr";
            var negGlAccount = "211900";

            var finActivity6 = ObjectSpace.CreateObject<FinActivity>();
            finActivity6.FromActivity = grossActivity;
            finActivity6.ToActivity = negActivity;
            finActivity6.FunctionalCcyAmtExpr = "-AustPostSettles.Sum(NegativeCorrections)";
            finActivity6.GlDescription = "SUMMARY POSTING - AUSTRALIA POST";
            finActivity6.GlDescDateFormat = glDescDateFormat;
            finActivity6.GlAccount = negGlAccount;
            finActivity6.JournalGroup = journalGroup;
            finActivity6.TargetObject = FinJournalTargetObject.BankStmt;
            #endregion

            #endregion

            #region Transactions

            var tranDate = new DateTime(2014, 03, 31);

            // Bank Stmt
            var bankStmt = ObjectSpace.CreateObject<BankStmt>();
            bankStmt.TranDate = tranDate;
            bankStmt.Account = account;
            bankStmt.Activity = grossActivity;
            bankStmt.TranAmount = 634188.22M;
            bankStmt.FunctionalCcyAmt = bankStmt.TranAmount;
            bankStmt.CounterCcyAmt = bankStmt.TranAmount;
            bankStmt.CounterCcy = currency;
            bankStmt.TranDescription = "TRANSFER                                AGC140329370952    FROM AUSTRALIA POST";

            // Australia Post Settlement
            var aps = ObjectSpace.CreateObject<AustPostSettle>();
            aps.BankStmt = bankStmt;
            aps.GrossAmount = 648322.69M;
            aps.DishonourChequeFee = 60;
            aps.NegativeCorrections = 727.52M;
            aps.DishonourChequeReversal = 612.12M;
            aps.GrossCommission = 12734.83M;
            aps.CommissionGST = 1157.71M;

            #endregion

            #region Generate Journals
            // Params
            var glParam = ObjectSpace.CreateObject<FinGenJournalParam>();
            glParam.FromDate = tranDate;
            glParam.ToDate = tranDate;
            var journalGroupParam = ObjectSpace.CreateObject<FinJournalGroupParam>();
            journalGroupParam.JournalGroup = journalGroup;

            ObjectSpace.CommitChanges();

            journalGroupParam.GenJournalParam = glParam;
            var jg = new ParamJournalGenerator(glParam, ObjectSpace);
            jg.Execute();
            ObjectSpace.CommitChanges();
            #endregion

            #region Asserts
            var gls = ObjectSpace.GetObjects<GenLedger>();
            Assert.AreEqual(12, gls.Where(x => x.IsJournal).Count());
            GenLedger gl = null;
            Assert.AreEqual(aps.GrossAmount, 
                gls.Where(x => x.Activity == grossActivity & x.GlAccount == bankGlAccount)
                .Sum(x => x.FunctionalCcyAmt));

            gl = gls.FirstOrDefault(x => x.Activity == grossActivity & x.GlAccount == grossGlAccount);
            Assert.AreEqual(-aps.GrossAmount, gl.FunctionalCcyAmt);
            #endregion

        }

        [Test]
        public void GenerateJournals_FinActivityMapSubStringExpr_MappedToJournals()
        {
            #region Prepare
            var journalGroup = ObjectSpace.CreateObject<FinJournalGroup>();
            journalGroup.Name = "VF Bank";

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ 94881";
            var currency = ObjectSpace.CreateObject<Currency>();
            currency.Name = "AUD";

            var glDescDateFormat = "dd-mmm-yy";
            var bankGlAccount = "210156";

            #endregion

            #region Cash Flow Mapping

            #region Bank Account
            var finAccount = ObjectSpace.CreateObject<FinAccount>();
            finAccount.Account = account;
            finAccount.GlAccount = bankGlAccount;
            finAccount.JournalGroup = journalGroup;
            #endregion

            #region Gross Amount
            var grossActivity = ObjectSpace.CreateObject<Activity>();
            grossActivity.Name = "V AMEX Rcpt";
            var grossGlAccount = "211900";

            var finActivity1 = ObjectSpace.CreateObject<FinActivity>();
            finActivity1.FromActivity = grossActivity;
            finActivity1.ToActivity = grossActivity;
            finActivity1.FunctionalCcyAmtExpr = "SubString(TranDescription, 71, 9)";
            finActivity1.GlDescription = "SUMMARY POSTING - AMEX";
            finActivity1.Token = "Gross";
            finActivity1.RowIndex = 1;
            finActivity1.GlDescDateFormat = glDescDateFormat;
            finActivity1.GlAccount = grossGlAccount;
            finActivity1.JournalGroup = journalGroup;
            finActivity1.TargetObject = FinJournalTargetObject.BankStmt;
            #endregion

            #region Commission
            var commActivity = ObjectSpace.CreateObject<Activity>();
            commActivity.Name = "V AMEX Comm";
            var commGlAccount = "691030";

            var finActivity2 = ObjectSpace.CreateObject<FinActivity>();
            finActivity2.FromActivity = grossActivity;
            finActivity2.ToActivity = commActivity;
            finActivity2.FunctionalCcyAmtExpr = "({FA} - {FA(Gross)}) * 10/11";
            finActivity2.GlDescription = "AMEX COMMISSION CHARGES";
            finActivity2.Token = "Comm";
            finActivity2.GlDescDateFormat = glDescDateFormat;
            finActivity2.GlAccount = commGlAccount;
            finActivity2.RowIndex = 2;
            finActivity2.JournalGroup = journalGroup;
            finActivity2.TargetObject = FinJournalTargetObject.BankStmt;
            #endregion

            #region Commission GST
            var gstActivity = ObjectSpace.CreateObject<Activity>();
            gstActivity.Name = "V AMEX Comm GST";
            var gstGlAccount = "235815";

            var finActivity3 = ObjectSpace.CreateObject<FinActivity>();
            finActivity3.FromActivity = grossActivity;
            finActivity3.ToActivity = gstActivity;
            finActivity3.FunctionalCcyAmtExpr = "{FA(Comm)} * 0.1";
            finActivity3.GlDescription = "GST ON AMEX COMMISSION CHARGES";
            finActivity3.Token = "C";
            finActivity3.RowIndex = 3;
            finActivity3.GlDescDateFormat = glDescDateFormat;
            finActivity3.GlAccount = gstGlAccount;
            finActivity3.JournalGroup = journalGroup;
            finActivity3.TargetObject = FinJournalTargetObject.BankStmt;

            #endregion
            #endregion

            #region Transactions
            var tranDate = new DateTime(2014, 03, 31);
            decimal grossAmount = 122441.99M;

            // Bank Stmt
            var bankStmt = ObjectSpace.CreateObject<BankStmt>();
            bankStmt.TranDate = tranDate;
            bankStmt.Account = account;
            bankStmt.Activity = grossActivity;
            bankStmt.TranAmount = 119546.24M;
            bankStmt.FunctionalCcyAmt = bankStmt.TranAmount;
            bankStmt.CounterCcyAmt = bankStmt.TranAmount;
            bankStmt.CounterCcy = currency;
            bankStmt.TranDescription = string.Format("TRANSFER                                9796724284         FROM AMEX GR{0}", grossAmount);

            #endregion

            #region Generate Journals
            // Params
            var glParam = ObjectSpace.CreateObject<FinGenJournalParam>();
            glParam.FromDate = bankStmt.TranDate;
            glParam.ToDate = bankStmt.TranDate;
            var journalGroupParam = ObjectSpace.CreateObject<FinJournalGroupParam>();
            journalGroupParam.JournalGroup = journalGroup;

            ObjectSpace.CommitChanges();

            journalGroupParam.GenJournalParam = glParam;
            var deleter = new CashDiscipline.UnitTests.TestObjects.MockJournalDeleter(glParam);
            var jg = new ParamJournalGenerator(glParam, ObjectSpace);
            jg.Execute();

            #endregion

            #region Asserts
            var gls = ObjectSpace.GetObjects<GenLedger>();
            Assert.AreEqual(6, gls.Count);
            GenLedger gl = null;
            gl = gls.FirstOrDefault(x => x.Activity == grossActivity & x.GlAccount == bankGlAccount);
            Assert.AreEqual(grossAmount, gl.FunctionalCcyAmt);

            gl = gls.FirstOrDefault(x => x.Activity == grossActivity & x.GlAccount == grossGlAccount);
            Assert.AreEqual(-grossAmount, gl.FunctionalCcyAmt);

            decimal commission = Math.Round((bankStmt.TranAmount - grossAmount) * 10 / 11, 2);

            gl = gls.FirstOrDefault(x => x.Activity == commActivity & x.GlAccount == bankGlAccount);
            Assert.AreEqual(commission, Math.Round(gl.FunctionalCcyAmt, 2));

            gl = gls.FirstOrDefault(x => x.Activity == commActivity & x.GlAccount == commGlAccount);
            Assert.AreEqual(-commission, Math.Round(gl.FunctionalCcyAmt, 2));

            gl = gls.FirstOrDefault(x => x.Activity == gstActivity & x.GlAccount == bankGlAccount);
            Assert.AreEqual(Math.Round(commission * 0.1M, 2), Math.Round(gl.FunctionalCcyAmt, 2));

            gl = gls.FirstOrDefault(x => x.Activity == gstActivity & x.GlAccount == gstGlAccount);
            Assert.AreEqual(-Math.Round(commission * 0.1M, 2), Math.Round(gl.FunctionalCcyAmt, 2));
            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GenerateJournals_FinActivityMapExprToken_MappedToJournals()
        {
            var commActivity = ObjectSpace.CreateObject<Activity>();
            commActivity.Name = "V Diners Comm";
            var gstActivity = ObjectSpace.CreateObject<Activity>();
            gstActivity.Name = "V Diners Comm GST";
            var commGlAccount = "691090";
            var gstGlAccount = "235815";
            var bankGlAccount = "210156";

            var journalGroup = ObjectSpace.CreateObject<FinJournalGroup>();
            journalGroup.Name = "VF Bank";
            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ 94881";
            var currency = ObjectSpace.CreateObject<Currency>();
            currency.Name = "AUD";

            var bankStmt = ObjectSpace.CreateObject<BankStmt>();
            bankStmt.TranDate = new DateTime(2013, 08, 02);
            bankStmt.Account = account;
            bankStmt.Activity = commActivity;
            bankStmt.TranAmount = -39.49M;
            bankStmt.FunctionalCcyAmt = bankStmt.TranAmount;
            bankStmt.CounterCcyAmt = bankStmt.TranAmount;
            bankStmt.CounterCcy = currency;
            bankStmt.TranDescription = "PAYMENT                                 000002689166680    TO   DINERS   1563.01";

            // bank account
            var finAccount = ObjectSpace.CreateObject<FinAccount>();
            finAccount.Account = account;
            finAccount.GlAccount = bankGlAccount;
            finAccount.JournalGroup = journalGroup;

            // commission
            var finActivity1 = ObjectSpace.CreateObject<FinActivity>();
            finActivity1.FromActivity = commActivity;
            finActivity1.ToActivity = commActivity;
            finActivity1.Token = "A";
            finActivity1.FunctionalCcyAmtExpr = "{FA} * 10/11";
            finActivity1.GlDescription = "DINERS COMMISSION CHARGES";
            finActivity1.GlDescDateFormat = "dd-mmm-yy";
            finActivity1.GlAccount = commGlAccount;
            finActivity1.JournalGroup = journalGroup;
            finActivity1.RowIndex = 1;
            finActivity1.Algorithm = FinMapAlgorithmType.ORM;

            // commission GST
            var finActivity2 = ObjectSpace.CreateObject<FinActivity>();
            finActivity2.FromActivity = commActivity;
            finActivity2.ToActivity = gstActivity;
            finActivity2.Token = "B";
            finActivity2.FunctionalCcyAmtExpr = "{FA(A)} * 0.1";
            finActivity2.GlDescription = "DINERS COMMISSION CHARGES";
            finActivity2.GlDescDateFormat = "dd-mmm-yy";
            finActivity2.GlAccount = gstGlAccount;
            finActivity2.JournalGroup = journalGroup;
            finActivity2.RowIndex = 2;
            finActivity2.Algorithm = FinMapAlgorithmType.ORM;

            // Params
            var glParam = ObjectSpace.CreateObject<FinGenJournalParam>();
            glParam.FromDate = bankStmt.TranDate;
            glParam.ToDate = bankStmt.TranDate;
            var journalGroupParam = ObjectSpace.CreateObject<FinJournalGroupParam>();
            journalGroupParam.JournalGroup = journalGroup;

            ObjectSpace.CommitChanges();

            journalGroupParam.GenJournalParam = glParam;
            var deleter = new CashDiscipline.UnitTests.TestObjects.MockJournalDeleter(glParam);
            var jg = new ParamJournalGenerator(glParam, ObjectSpace);
            jg.Execute();


            // validate result
            var gls = ObjectSpace.GetObjects<GenLedger>();
            Assert.AreEqual(4, gls.Count);

            GenLedger gl = null;
            gl = gls.FirstOrDefault(x => x.Activity == commActivity & x.GlAccount == bankGlAccount);
            Assert.AreEqual(-35.9, gl.FunctionalCcyAmt);

            gl = gls.FirstOrDefault(x => x.Activity == commActivity & x.GlAccount == commGlAccount);
            Assert.AreEqual(35.9, gl.FunctionalCcyAmt);

            gl = gls.FirstOrDefault(x => x.Activity == gstActivity & x.GlAccount == bankGlAccount);
            Assert.AreEqual(-3.59, gl.FunctionalCcyAmt);

            gl = gls.FirstOrDefault(x => x.Activity == gstActivity & x.GlAccount == gstGlAccount);
            Assert.AreEqual(3.59, gl.FunctionalCcyAmt);
        }

        [Test]
        public void GenerateJournals_FinActivityMapTypeCombo()
        {
            #region Prepare
            var journalGroup = ObjectSpace.CreateObject<FinJournalGroup>();
            journalGroup.Name = "VF Bank";

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ 94881";
            var currency = ObjectSpace.CreateObject<Currency>();
            currency.Name = "AUD";

            var glDescDateFormat = "dd-mmm-yy";

            var bankGlAccount = "210156";

            #endregion

            #region Cash Flow Mapping

            #region Bank Account
            var finAccount = ObjectSpace.CreateObject<FinAccount>();
            finAccount.Account = account;
            finAccount.GlAccount = bankGlAccount;
            finAccount.JournalGroup = journalGroup;
            #endregion

            #region Gross Amount
            var grossActivity = ObjectSpace.CreateObject<Activity>();
            grossActivity.Name = "V AMEX Rcpt";
            var grossGlAccount = "211900";

            var finActivity1 = ObjectSpace.CreateObject<FinActivity>();
            finActivity1.FromActivity = grossActivity;
            finActivity1.ToActivity = grossActivity;
            finActivity1.FunctionalCcyAmtExpr = "SubString(TranDescription, 71, 9)";
            finActivity1.GlDescription = "SUMMARY POSTING - AMEX";
            finActivity1.Token = "A";
            finActivity1.RowIndex = 1;
            finActivity1.GlDescDateFormat = glDescDateFormat;
            finActivity1.GlAccount = grossGlAccount;
            finActivity1.JournalGroup = journalGroup;
            finActivity1.TargetObject = FinJournalTargetObject.BankStmt;
            #endregion

            #region Commission
            var commActivity = ObjectSpace.CreateObject<Activity>();
            commActivity.Name = "V AMEX Comm";
            var commGlAccount = "691030";

            var finActivity2 = ObjectSpace.CreateObject<FinActivity>();
            finActivity2.FromActivity = grossActivity;
            finActivity2.ToActivity = commActivity;
            finActivity2.FunctionalCcyAmtExpr = "({FA} - {FA(A)}) * 10/11";
            finActivity2.GlDescription = "AMEX COMMISSION CHARGES";
            finActivity2.Token = "B";
            finActivity2.GlDescDateFormat = glDescDateFormat;
            finActivity2.GlAccount = commGlAccount;
            finActivity2.RowIndex = 2;
            finActivity2.JournalGroup = journalGroup;
            finActivity2.TargetObject = FinJournalTargetObject.BankStmt;
            #endregion

            #region Commission GST
            var gstActivity = ObjectSpace.CreateObject<Activity>();
            gstActivity.Name = "V AMEX Comm GST";
            var gstGlAccount = "235815";

            var finActivity3 = ObjectSpace.CreateObject<FinActivity>();
            finActivity3.FromActivity = grossActivity;
            finActivity3.ToActivity = gstActivity;
            finActivity3.FunctionalCcyAmtExpr = "{FA(B)} * 0.1";
            finActivity3.GlDescription = "GST ON AMEX COMMISSION CHARGES";
            finActivity3.Token = "C";
            finActivity3.RowIndex = 3;
            finActivity3.GlDescDateFormat = glDescDateFormat;
            finActivity3.GlAccount = gstGlAccount;
            finActivity3.JournalGroup = journalGroup;
            finActivity3.TargetObject = FinJournalTargetObject.BankStmt;

            #endregion

            #region Bank Fees
            var bankFeeActivity = ObjectSpace.CreateObject<Activity>();
            bankFeeActivity.Name = "HSBC Bank Fee";
            var bankFeeGlAccount = "691090";

            var finActivity4 = ObjectSpace.CreateObject<FinActivity>();
            finActivity4.FromActivity = bankFeeActivity;
            finActivity4.ToActivity = bankFeeActivity;
            finActivity4.FunctionalCcyAmtExpr = "{FA}";
            finActivity4.GlDescription = "Bank Fees";
            finActivity4.GlDescDateFormat = glDescDateFormat;
            finActivity4.GlAccount = bankFeeGlAccount;
            finActivity4.JournalGroup = journalGroup;
            finActivity4.TargetObject = FinJournalTargetObject.All;
            finActivity4.Algorithm = FinMapAlgorithmType.SQL;
            #endregion

            #endregion

            #region Transactions
            var tranDate = new DateTime(2014, 03, 31);
            decimal grossAmount = 122441.99M;

            // Bank Stmt
            var bankStmt1 = ObjectSpace.CreateObject<BankStmt>();
            bankStmt1.TranDate = tranDate;
            bankStmt1.Account = account;
            bankStmt1.Activity = grossActivity;
            bankStmt1.TranAmount = 119546.24M;
            bankStmt1.FunctionalCcyAmt = bankStmt1.TranAmount;
            bankStmt1.CounterCcyAmt = bankStmt1.TranAmount;
            bankStmt1.CounterCcy = currency;
            bankStmt1.TranDescription = string.Format("TRANSFER                                9796724284         FROM AMEX GR{0}", grossAmount);

            var bankStmt2 = ObjectSpace.CreateObject<BankStmt>();
            bankStmt2.TranDate = tranDate;
            bankStmt2.Account = account;
            bankStmt2.Activity = bankFeeActivity;
            bankStmt2.TranAmount = -150M;
            bankStmt2.FunctionalCcyAmt = bankStmt2.TranAmount;
            bankStmt2.CounterCcyAmt = bankStmt2.TranAmount;
            bankStmt2.CounterCcy = currency;

            #endregion

            #region Generate Journals
            // Params
            var glParam = ObjectSpace.CreateObject<FinGenJournalParam>();
            glParam.FromDate = bankStmt1.TranDate;
            glParam.ToDate = bankStmt1.TranDate;
            var journalGroupParam = ObjectSpace.CreateObject<FinJournalGroupParam>();
            journalGroupParam.JournalGroup = journalGroup;
            journalGroupParam.GenJournalParam = glParam;

            ObjectSpace.CommitChanges();
            Assert.AreEqual(1, glParam.JournalGroupParams.Count);

            var jg = new ParamJournalGenerator(glParam, ObjectSpace);
            jg.Execute();
            ObjectSpace.CommitChanges();

            #endregion

            #region Asserts
            var gls = ObjectSpace.GetObjects<GenLedger>();

            {
                var accountGls = gls.Where(x => x.Activity == grossActivity && x.GlAccount == bankGlAccount);
                Assert.AreEqual(grossAmount, accountGls.Sum(x => x.FunctionalCcyAmt));
            }
            {
                var gl = gls.FirstOrDefault(x => x.Activity == grossActivity & x.GlAccount == grossGlAccount);
                Assert.AreEqual(-grossAmount, gl.FunctionalCcyAmt);
            }

            {
                GenLedger gl = null;
                decimal commission = Math.Round((bankStmt1.TranAmount - grossAmount) * 10 / 11, 2);

                gl = gls.FirstOrDefault(x => x.Activity == commActivity & x.GlAccount == commGlAccount);
                Assert.AreEqual(-commission, Math.Round(gl.FunctionalCcyAmt, 2));

                gl = gls.FirstOrDefault(x => x.Activity == gstActivity & x.GlAccount == gstGlAccount);
                Assert.AreEqual(-Math.Round(commission * 0.1M, 2), Math.Round(gl.FunctionalCcyAmt, 2));
            }
            {
                var gl = gls.FirstOrDefault(x => x.Activity == bankFeeActivity & x.GlAccount == bankFeeGlAccount);
                Assert.AreEqual(-Math.Round(-150M, 2), Math.Round(gl.FunctionalCcyAmt, 2));
            }
            #endregion
        }

        [Test]
        public void DeleteJournals()
        {
            #region Prepare

            var commActivity = ObjectSpace.CreateObject<Activity>();
            commActivity.Name = "V Diners Comm";
            var gstActivity = ObjectSpace.CreateObject<Activity>();
            gstActivity.Name = "V Diners Comm GST";
            var commGlAccount = "691090";
            var gstGlAccount = "235815";
            var bankGlAccount = "210156";

            var journalGroup = ObjectSpace.CreateObject<FinJournalGroup>();
            journalGroup.Name = "VF Bank";
            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHA ANZ 94881";
            var currency = ObjectSpace.CreateObject<Currency>();
            currency.Name = "AUD";

            // create bank statement line
            var bankStmt = ObjectSpace.CreateObject<BankStmt>();
            bankStmt.TranDate = new DateTime(2013, 08, 02);
            bankStmt.Account = account;
            bankStmt.Activity = commActivity;
            bankStmt.TranAmount = -39.49M;
            bankStmt.FunctionalCcyAmt = bankStmt.TranAmount;
            bankStmt.CounterCcyAmt = bankStmt.TranAmount;
            bankStmt.CounterCcy = currency;
            bankStmt.TranDescription = "PAYMENT                                 000002689166680    TO   DINERS   1563.01";

            // bank account Gl
            var finAccount = ObjectSpace.CreateObject<FinAccount>();
            finAccount.Account = account;
            finAccount.GlAccount = bankGlAccount;
            finAccount.JournalGroup = journalGroup;

            // commission
            var finActivity1 = ObjectSpace.CreateObject<FinActivity>();
            finActivity1.FromActivity = commActivity;
            finActivity1.ToActivity = commActivity;
            finActivity1.Token = "A";
            finActivity1.FunctionalCcyAmtExpr = "{FA} * 10/11";
            finActivity1.GlDescription = "DINERS COMMISSION CHARGES";
            finActivity1.GlDescDateFormat = "dd-mmm-yy";
            finActivity1.GlAccount = commGlAccount;
            finActivity1.JournalGroup = journalGroup;
            finActivity1.RowIndex = 1;

            // commission GST
            var finActivity2 = ObjectSpace.CreateObject<FinActivity>();
            finActivity2.FromActivity = commActivity;
            finActivity2.ToActivity = gstActivity;
            finActivity2.Token = "B";
            finActivity2.FunctionalCcyAmtExpr = "{FA(A)} * 0.1";
            finActivity2.GlDescription = "DINERS COMMISSION CHARGES";
            finActivity2.GlDescDateFormat = "dd-mmm-yy";
            finActivity2.GlAccount = gstGlAccount;
            finActivity2.JournalGroup = journalGroup;
            finActivity2.RowIndex = 2;

            #endregion

            #region Act

            // Params
            var glParam = ObjectSpace.CreateObject<FinGenJournalParam>();
            glParam.FromDate = bankStmt.TranDate;
            glParam.ToDate = bankStmt.TranDate;
            var journalGroupParam = ObjectSpace.CreateObject<FinJournalGroupParam>();
            journalGroupParam.JournalGroup = journalGroup;

            ObjectSpace.CommitChanges();

            journalGroupParam.GenJournalParam = glParam;

            var jg = new ParamJournalGenerator(glParam, ObjectSpace);
            jg.Execute();
            jg.Execute();
            ObjectSpace.CommitChanges();
            #endregion

            #region Assert
            var gls = ObjectSpace.GetObjects<GenLedger>();
            Assert.AreEqual(4, gls.Count);
            #endregion
        }

    }
}
