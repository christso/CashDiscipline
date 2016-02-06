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

using CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.BusinessObjects.FinAccounting;
using CTMS.Module.BusinessObjects.ChartOfAccounts;
using CTMS.Module.ParamObjects.FinAccounting;
using CTMS.Module.BusinessObjects;
using DevExpress.Persistent.Validation;
using CTMS.Module;
using CTMS.Module.Controllers.Cash;


using System.Diagnostics;
using System.Data.SqlClient;
using CTMS.Module.Controllers.FinAccounting;
using DevExpress.Xpo.DB;
using CTMS.Module.ControllerHelpers;
using CTMS.Module.DatabaseUpdate;
using CTMS.Module.ControllerHelpers.FinAccounting;
using CTMS.UnitTests.Base;

namespace CTMS.UnitTests.MSSqlDbTest
{
    [TestFixture]
    public class FinAccountingDbTests : TestBase
    {
        public FinAccountingDbTests()
        {
            SetTesterDbType(TesterDbType.MsSql);
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
            finActivity1.Index = 1;

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
            finActivity2.Index = 2;

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

            jg.DeleteAutoGenLedgerItems();
            #endregion

            #region Assert

            var gls = ObjectSpace.GetObjects<GenLedger>();
            Assert.AreEqual(0, gls.Count);

            #endregion

        }

        public override void SetupObjects()
        {
            Updater.CreateCurrencies(ObjectSpace);
            Updater.CreateFinAccountingDefaults(ObjectSpace);
            Updater.InitSetOfBooks(ObjectSpace);
        }
    }

}