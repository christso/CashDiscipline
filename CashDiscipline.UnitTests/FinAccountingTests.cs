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
using CashDiscipline.Module.BusinessObjects.ChartOfAccounts;
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
        public FinAccountingTests()
        {
            SetTesterDbType(TesterDbType.InMemory);
        }

        [Test]
        public void GenerateJournals_CashFlowReclass_MappedToJournals()
        {
            #region Prepare
            var journalGroup = ObjectSpace.CreateObject<FinJournalGroup>();
            journalGroup.Name = "VHF Bank";

            var account = ObjectSpace.CreateObject<Account>();
            account.Name = "VHF HSBC AUD";
            var currency = ObjectSpace.CreateObject<Currency>();
            currency.Name = "AUD";

            var glDescDateFormat = "dd-mmm-yy";

            var bankGlAccount = ObjectSpace.CreateObject<GlAccount>();
            bankGlAccount.Code = "210127";

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
            var bankFeeGlAccount = ObjectSpace.CreateObject<GlAccount>();
            bankFeeGlAccount.Code = "691090";

            var finActivity1 = ObjectSpace.CreateObject<FinActivity>();
            finActivity1.FromActivity = bankFeeActivity;
            finActivity1.ToActivity = bankFeeActivity;
            finActivity1.FunctionalCcyAmtExpr = "{FA}";
            finActivity1.GlDescription = "Bank Fees";
            finActivity1.GlDescDateFormat = glDescDateFormat;
            finActivity1.GlAccount = bankFeeGlAccount;
            finActivity1.JournalGroup = journalGroup;
            finActivity1.TargetObject = FinJournalTargetObject.All;
            #endregion

            #region Interest Expense
            var intExpActivity = ObjectSpace.CreateObject<Activity>();
            intExpActivity.Name = "IntExp CCS VG";
            var intExpGlAccount = ObjectSpace.CreateObject<GlAccount>();
            intExpGlAccount.Code = "691090";

            var finActivity2 = ObjectSpace.CreateObject<FinActivity>();
            finActivity2.FromActivity = intExpActivity;
            finActivity2.ToActivity = intExpActivity;
            finActivity2.FunctionalCcyAmtExpr = "{FA}";
            finActivity2.GlDescription = "Interest Expense";
            finActivity2.GlDescDateFormat = glDescDateFormat;
            finActivity2.GlAccount = intExpGlAccount;
            finActivity2.JournalGroup = journalGroup;
            finActivity2.TargetObject = FinJournalTargetObject.All;
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

            var deleter = new CashDiscipline.UnitTests.TestObjects.MockJournalDeleter(glParam);
            var jg = new JournalGenerator(glParam, deleter);
            jg.Execute();

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
        [Category("Coverage_2")]
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

            var bankGlAccount = ObjectSpace.CreateObject<GlAccount>();
            bankGlAccount.Code = "210159";

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
            var grossGlAccount = ObjectSpace.CreateObject<GlAccount>();
            grossGlAccount.Code = "211900";

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
            var commGlAccount = ObjectSpace.CreateObject<GlAccount>();
            commGlAccount.Code = "691090";

            var finActivity1 = ObjectSpace.CreateObject<FinActivity>();
            finActivity1.FromActivity = grossActivity;
            finActivity1.ToActivity = commActivity;
            finActivity1.FunctionalCcyAmtExpr = "AustPostSettles.Sum(GrossCommission)";
            finActivity1.GlDescription = "AUSTRALIA POST FEES";
            finActivity1.GlDescDateFormat = glDescDateFormat;
            finActivity1.GlAccount = commGlAccount;
            finActivity1.JournalGroup = journalGroup;
            finActivity1.TargetObject = FinJournalTargetObject.BankStmt;
            #endregion

            #region Commission GST
            var gstActivity = ObjectSpace.CreateObject<Activity>();
            gstActivity.Name = "V AustPost Comm GST";
            var gstGlAccount = ObjectSpace.CreateObject<GlAccount>();
            gstGlAccount.Code = "235815";

            var finActivity2 = ObjectSpace.CreateObject<FinActivity>();
            finActivity2.FromActivity = grossActivity;
            finActivity2.ToActivity = gstActivity;
            finActivity2.FunctionalCcyAmtExpr = "AustPostSettles.Sum(CommissionGST)";
            finActivity2.GlDescription = "GST ON AUSTRALIA POST FEES";
            finActivity2.GlDescDateFormat = glDescDateFormat;
            finActivity2.GlAccount = gstGlAccount;
            finActivity2.JournalGroup = journalGroup;
            finActivity2.TargetObject = FinJournalTargetObject.BankStmt;
            #endregion

            #region Dishonour Chequqe Fee
            var dsrFeeActivity = ObjectSpace.CreateObject<Activity>();
            dsrFeeActivity.Name = "V AustPost Dsr Chq Fee";
            var dsrFeeGlAccount = ObjectSpace.CreateObject<GlAccount>();
            dsrFeeGlAccount.Code = "671701";

            var finActivity4 = ObjectSpace.CreateObject<FinActivity>();
            finActivity4.FromActivity = grossActivity;
            finActivity4.ToActivity = dsrFeeActivity;
            finActivity4.FunctionalCcyAmtExpr = "AustPostSettles.Sum(DishonourChequeFee)";
            finActivity4.GlDescription = "DISHONOUR FEE - AUST POST";
            finActivity4.GlDescDateFormat = glDescDateFormat;
            finActivity4.GlAccount = gstGlAccount;
            finActivity4.JournalGroup = journalGroup;
            finActivity4.TargetObject = FinJournalTargetObject.BankStmt;
            #endregion

            #region DishonourChequeReversal
            var dsrRvslActivity = ObjectSpace.CreateObject<Activity>();
            dsrRvslActivity.Name = "V AustPost Dsr Chq Rvsl";
            var dsrRvslGlAccount = ObjectSpace.CreateObject<GlAccount>();
            dsrRvslGlAccount.Code = "211900";

            var finActivity5 = ObjectSpace.CreateObject<FinActivity>();
            finActivity5.FromActivity = grossActivity;
            finActivity5.ToActivity = dsrRvslActivity;
            finActivity5.FunctionalCcyAmtExpr = "AustPostSettles.Sum(DishonourChequeReversal)";
            finActivity5.GlDescription = "SUMMARY POSTING - AUSTRALIA POST";
            finActivity5.GlDescDateFormat = glDescDateFormat;
            finActivity5.GlAccount = dsrRvslGlAccount;
            finActivity5.JournalGroup = journalGroup;
            finActivity5.TargetObject = FinJournalTargetObject.BankStmt;
            #endregion

            #region NegativeCorrections
            var negActivity = ObjectSpace.CreateObject<Activity>();
            negActivity.Name = "V AustPost Neg Corr";
            var negGlAccount = ObjectSpace.CreateObject<GlAccount>();
            negGlAccount.Code = "211900";

            var finActivity6 = ObjectSpace.CreateObject<FinActivity>();
            finActivity6.FromActivity = grossActivity;
            finActivity6.ToActivity = negActivity;
            finActivity6.FunctionalCcyAmtExpr = "AustPostSettles.Sum(NegativeCorrections)";
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
            aps.GrossAmount = 634188.22M;
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

            var deleter = new CashDiscipline.UnitTests.TestObjects.MockJournalDeleter(glParam);
            var jg = new JournalGenerator(glParam, deleter);
            jg.Execute();

            #endregion

            #region Asserts
            var gls = ObjectSpace.GetObjects<GenLedger>();
            Assert.AreEqual(12, gls.Count);
            GenLedger gl = null;
            gl = gls.FirstOrDefault(x => x.Activity == grossActivity & x.GlAccount == bankGlAccount);
            Assert.AreEqual(aps.GrossAmount, gl.FunctionalCcyAmt);

            gl = gls.FirstOrDefault(x => x.Activity == grossActivity & x.GlAccount == grossGlAccount);
            Assert.AreEqual(-aps.GrossAmount, gl.FunctionalCcyAmt);
            #endregion

        }

        [Test]
        [Category("Coverage_2")]
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

            var bankGlAccount = ObjectSpace.CreateObject<GlAccount>();
            bankGlAccount.Code = "210156";

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
            var grossGlAccount = ObjectSpace.CreateObject<GlAccount>();
            grossGlAccount.Code = "211900";

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
            commActivity.Name = "V AMEX Comm GST";
            var commGlAccount = ObjectSpace.CreateObject<GlAccount>();
            commGlAccount.Code = "691030";

            var finActivity2 = ObjectSpace.CreateObject<FinActivity>();
            finActivity2.FromActivity = grossActivity;
            finActivity2.ToActivity = commActivity;
            finActivity2.FunctionalCcyAmtExpr = "{FA} - {FA(A)} * 10/11";
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
            var gstGlAccount = ObjectSpace.CreateObject<GlAccount>();
            gstGlAccount.Code = "235815";

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
            var jg = new JournalGenerator(glParam, deleter);
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

            decimal commission = Math.Round(bankStmt.TranAmount - grossAmount * 10 / 11, 2);

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

        [Test]
        [Category("Coverage_1")]
        public void GenerateJournals_FinActivityMapExprToken_MappedToJournals()
        {
            var commActivity = ObjectSpace.CreateObject<Activity>();
            commActivity.Name = "V Diners Comm";
            var gstActivity = ObjectSpace.CreateObject<Activity>();
            gstActivity.Name = "V Diners Comm GST";
            var commGlAccount = ObjectSpace.CreateObject<GlAccount>();
            commGlAccount.Code = "691090";
            var gstGlAccount = ObjectSpace.CreateObject<GlAccount>();
            gstGlAccount.Code = "235815";
            var bankGlAccount = ObjectSpace.CreateObject<GlAccount>();
            bankGlAccount.Code = "210156";

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

            // Params
            var glParam = ObjectSpace.CreateObject<FinGenJournalParam>();
            glParam.FromDate = bankStmt.TranDate;
            glParam.ToDate = bankStmt.TranDate;
            var journalGroupParam = ObjectSpace.CreateObject<FinJournalGroupParam>();
            journalGroupParam.JournalGroup = journalGroup;

            ObjectSpace.CommitChanges();

            journalGroupParam.GenJournalParam = glParam;
            var deleter = new CashDiscipline.UnitTests.TestObjects.MockJournalDeleter(glParam);
            var jg = new JournalGenerator(glParam, deleter);
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
        public void DeleteJournals()
        {
            #region Prepare

            var commActivity = ObjectSpace.CreateObject<Activity>();
            commActivity.Name = "V Diners Comm";
            var gstActivity = ObjectSpace.CreateObject<Activity>();
            gstActivity.Name = "V Diners Comm GST";
            var commGlAccount = ObjectSpace.CreateObject<GlAccount>();
            commGlAccount.Code = "691090";
            var gstGlAccount = ObjectSpace.CreateObject<GlAccount>();
            gstGlAccount.Code = "235815";
            var bankGlAccount = ObjectSpace.CreateObject<GlAccount>();
            bankGlAccount.Code = "210156";

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

            var jg = new JournalGenerator(glParam);
            jg.Execute();
            jg.Execute();

            #endregion

            #region Assert
            var gls = ObjectSpace.GetObjects<GenLedger>();
            Assert.AreEqual(4, gls.Count);
            #endregion
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
    }
}
