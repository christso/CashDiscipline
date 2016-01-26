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
using CTMS.Module.BusinessObjects.Forex;
using CTMS.Module.ParamObjects.Cash;

namespace CTMS.UnitTests.InMemoryDbTest
{
    public class InMemoryDbTestBase
    {
        private const string ApplicationName = "CTMS";

        private XPObjectSpaceProvider ObjectSpaceProvider;
        protected XPObjectSpace ObjectSpace;
        protected TestApplication Application;

        [SetUp]
        public void Setup()
        {
            InitializeImageLoader();

            ObjectSpaceProvider = CreateObjectSpaceProvider();

            Application = new TestApplication();

            // add base module
            ModuleBase module = new ModuleBase();
            TestUtil.AddExportedTypes(module);
            Application.Modules.Add(module);
            AddExportedTypes(module);

            Application.Setup(ApplicationName, ObjectSpaceProvider);
            Application.CheckCompatibility();
            ObjectSpace = (XPObjectSpace)ObjectSpaceProvider.CreateObjectSpace();

            SetupObjects();
        }

        private void InitializeImageLoader()
        {
            var classType = GetType();
            if (!ImageLoader.IsInitialized)
            {
                ImageLoader.Init(new AssemblyResourceImageSource(classType.Assembly.FullName, "Images"));
            }
        }

        private XPObjectSpaceProvider CreateObjectSpaceProvider()
        {
            return new XPObjectSpaceProvider(new MemoryDataStoreProvider());
        }

        protected virtual void SetupObjects()
        {
        }

        protected virtual void AddExportedTypes(ModuleBase module)
        {
            // module.AdditionalExportedTypes.Add(typeof(SetOfBooks));
        }

        [TearDown]
        public void TearDown()
        {
            Application = null;
        }
    }
}
