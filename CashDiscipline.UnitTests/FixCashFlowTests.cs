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


namespace CashDiscipline.UnitTests
{
    [TestFixture]
    public class FixCashFlowTests : TestBase
    {
        #region Set up
        public FixCashFlowTests()
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
            CashDiscipline.Module.DatabaseUpdate.Updater.CreateCashFlowDefaults(ObjectSpace);
        }

        public override void OnAddExportedTypes(ModuleBase module)
        {
            CashDisciplineTestHelper.AddExportedTypes(module);
        }
        #endregion
        
        
    }
}
