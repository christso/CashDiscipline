using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using CashDiscipline.Module.Controllers.Cash;
using Xafology.TestUtils;
using CashDiscipline.Module.ParamObjects.Cash;
using CashDiscipline.Module.Logic.Cash;
using DevExpress.ExpressApp.Xpo;
using CashDiscipline.Module.BusinessObjects.AccountsPayable;
using System.Reflection;
using System.IO;


namespace CashDiscipline.UnitTests
{
    [TestFixture]
    public class ApPmtDistnTests : TestBase
    {
        public ApPmtDistnTests()
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
            CashDiscipline.Module.DatabaseUpdate.Updater.CreateFunctions(ObjectSpace);
        }
        public override void OnAddExportedTypes(ModuleBase module)
        {
            CashDisciplineTestHelper.AddExportedTypes(module);
        }

        [Test]
        public void MapCounterpartyExpr()
        {
            #region Arrange

            string vendorName = "Tech Mahindra Business Services Limited";

            var cp1 = ObjectSpace.CreateObject<Counterparty>();
            cp1.Name = vendorName;

            var cp2 = ObjectSpace.CreateObject<Counterparty>();
            cp2.Name = "AGGREGATE";

            var pmt = ObjectSpace.CreateObject<ApPmtDistn>();
            pmt.PaymentDate = new DateTime(2017, 07, 15);
            pmt.Vendor = ObjectSpace.CreateObject<ApVendor>();
            pmt.Vendor.Name = vendorName;
            pmt.GlAccount = "222010";
            pmt.PaymentAmountAud = 256000;
            pmt.PaymentAmountFx = 256000;

            var map1 = ObjectSpace.CreateObject<ApPmtDistnMapping>();
            map1.CriteriaExpression = "GlAccount = '222010'";
            map1.CounterpartyExpr = "Vendor.Name";

            var map2 = ObjectSpace.CreateObject<ApPmtDistnMapping>();
            map2.CriteriaExpression = "ELSE";
            map2.Counterparty = cp2;

            var map3 = ObjectSpace.CreateObject<ApPmtDistnMapping>();
            map3.CriteriaExpression = "VendorPaymentAmountAud > 50000";
            map3.CounterpartyExpr = "Vendor.Name";

            ObjectSpace.CommitChanges();

            #endregion

            #region Act
            
            var mapper = new ApPmtDistnMapper(ObjectSpace);
            mapper.Process(pmt);

            ObjectSpace.CommitChanges();

            #endregion

            #region Assert

            ObjectSpace.Refresh();
            pmt = ObjectSpace.FindObject<ApPmtDistn>(null);
            Assert.NotNull(pmt.Counterparty);
            Assert.AreEqual(vendorName, pmt.Counterparty.Name);

            #endregion
        }

        [Test]
        public void ImportApPmtDistnCsv()
        {
            var importer = new ApPmtDistnImporter(ObjectSpace);
            var result = importer.Execute(@"\\finserv01\VHA Import\Cash Discipline\AP Payments\AP Payment.CSV");
            Console.WriteLine(result);
        }
    }
}
