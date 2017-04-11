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
using System.Reflection;
using CashDiscipline.Module.Clients;
using CashDiscipline.Common;
using ADOMD = Microsoft.AnalysisServices.AdomdClient;
using CashDiscipline.Module.Logic.FinAccounting;
using CashDiscipline.Module.ParamObjects.FinAccounting;
using DevExpress.ExpressApp.Editors;

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


        public void GetManifestResourceTest()
        {
            var resourcePath = CashDiscipline.Common.Constants.CashDiscSqlCreatePurgeJobPath;
            var stream = typeof(CashDiscipline.Module.AssemblyInfo).Assembly.GetManifestResourceStream(resourcePath);
            Console.WriteLine(stream.Length);
        }

        public void CriteriaToSqlTest()
        {
            string xpoCriteriaText = "[Activity] Is Not Null Or [ActionOwner.Name] Like 'UNDEFINED'";
            var criteria = CriteriaEditorHelper.GetCriteriaOperator(
               xpoCriteriaText, typeof(BankStmt), ObjectSpace);
            var sqlCriteriaText = CriteriaToWhereClauseHelper.GetOracleWhere(XpoCriteriaFixer.Fix(criteria));
            Console.WriteLine(sqlCriteriaText);
        }

        //[Test]
        public void RateRuleTest()
        {
            
            var ft = ObjectSpace.CreateObject<ForexTrade>();
            ft.PrimaryCcyAmt = 120;
            ft.CounterCcyAmt = 100;
            ft.Rate = 0.8M;
            var isValid = ft.Fit(CriteriaOperator.Parse("Iif(Rate != 0, Round(Rate,2) == Round(CounterCcyAmt / PrimaryCcyAmt,2), true)"));
            Console.WriteLine(isValid);
        }
        
        
    }
}
