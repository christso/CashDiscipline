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

namespace CTMS.UnitTests.Base
{
    public class InMemoryDbTestBase : ITest
    {
        private const string ApplicationName = "CTMS";

        private XPObjectSpaceProvider ObjectSpaceProvider;
        protected XPObjectSpace ObjectSpace;
        protected TestApplication Application;
        private readonly ModuleBase module;

        public InMemoryDbTestBase()
        {
            module = new ModuleBase();
        }

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            InitializeImageLoader();

            ObjectSpaceProvider = CreateObjectSpaceProvider();

            Application = new TestApplication();

            // add base module
            AddExportedTypes(module);
            Application.Modules.Add(module);

            Application.Setup(ApplicationName, ObjectSpaceProvider);
            Application.CheckCompatibility();
            ObjectSpace = (XPObjectSpace)ObjectSpaceProvider.CreateObjectSpace();
        }

        [SetUp]
        public void Setup()
        {
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

        public virtual void SetupObjects()
        {
        }

        protected virtual void AddExportedTypes(ModuleBase module)
        {
            TestUtil.AddExportedTypes(module);
        }

        [TearDown]
        public void TearDown()
        {
            TestUtil.DeleteExportedObjects(module, ObjectSpace.Session);
        }

        public virtual void DeleteExportedObjects(ModuleBase module, Session session)
        {
            if (module == null)
                throw new InvalidOperationException("module cannot be null");

            foreach (var type in module.AdditionalExportedTypes)
            {
                TestUtil.DeleteObjects(ObjectSpace.Session, type);
            }
        }

    }
}
