﻿using NUnit.Framework;
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
            map2.CounterpartyExpr = "AGGREGATE";

            #endregion

            #region Act

            var mapper = new ApPmtDistnMapper(ObjectSpace);
            mapper.Process(pmt);

            ObjectSpace.CommitChanges();

            #endregion

            #region Assert

            pmt = ObjectSpace.FindObject<ApPmtDistn>(null);
            Assert.NotNull(pmt.Counterparty);
            Assert.AreEqual(vendorName, pmt.Counterparty.Name);

            #endregion
        }
    }
}