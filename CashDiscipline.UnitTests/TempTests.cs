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
using CashDiscipline.Module.Attributes;

namespace CashDiscipline.UnitTests
{
    public class TempTests : TestBase
    {
        public TempTests()
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
        }

        public override void OnAddExportedTypes(ModuleBase module)
        {
            CashDisciplineTestHelper.AddExportedTypes(module);
        }

        //[Test]
        public void SaveTest()
        {

            var reversalFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            reversalFixTag.Name = CashDiscipline.Module.Constants.ReversalFixTag;
            reversalFixTag.FixTagType = CashForecastFixTagType.Ignore;

            var revRecFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            revRecFixTag.Name = CashDiscipline.Module.Constants.RevRecFixTag;
            revRecFixTag.FixTagType = CashForecastFixTagType.Ignore;

            var resRevRecFixTag = ObjectSpace.CreateObject<CashForecastFixTag>();
            resRevRecFixTag.Name = CashDiscipline.Module.Constants.ResRevRecFixTag;
            resRevRecFixTag.FixTagType = CashForecastFixTagType.Ignore;


            var cf1 = ObjectSpace.CreateObject<CashFlow>();
            cf1.AccountCcyAmt = 100;
            cf1.Fix = reversalFixTag;

            var cf2 = ObjectSpace.CreateObject<CashFlow>();
            cf2.AccountCcyAmt = 300;
            cf2.ParentCashFlow = cf1;
            cf2.Fixer = cf1;
            cf2.Fix = revRecFixTag;

            ObjectSpace.CommitChanges();

            var os = (XPObjectSpace)Application.CreateObjectSpace();
            var cfs = os.GetObjects<CashFlow>();

            Assert.AreEqual(2, cfs.Count);
        }

        //[Test]
        public void EvaluateEndOfMonth()
        {
            var cf = ObjectSpace.CreateObject<CashFlow>();
            cf.TranDate = new DateTime(2016,  3, 28);
            cf.FixToDate = new DateTime(2016, 3, 31);
            var result = cf.Evaluate(CriteriaOperator.Parse("LocalDateTimeNextMonth(TranDate)"));
            var objType = result.GetType();
        }


        [Test]
        public void TempTest()
        {
            var attrs = typeof(BankStmt).CustomAttributes;
            foreach (var attr in attrs)
            {
                if (attr.AttributeType == typeof(AutoColumnWidthAttribute))
                    Console.WriteLine("Attribute {0} matched", attr.AttributeType.Name);
                else
                    Console.WriteLine("Attribute {0} not matched", attr.AttributeType.Name);
            }
        }
    }
}
