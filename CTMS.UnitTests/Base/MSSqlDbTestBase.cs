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
using CTMS.Module.Controllers;
using CTMS.Module.DatabaseUpdate;
using CTMS.Module.ParamObjects.Cash;

using Xafology.Utils.Data;

namespace CTMS.UnitTests.Base
{
    [TestFixture]
    public class MSSqlDbTestBase : ITest
    {
        private const string DataPath = @"D:\CTSO\Data\MSSQL12\Data";
        private const string ServerName = @"(localdb)\ProjectsV12";
        private const string DatabaseName = "CTMS_Test";
        private const string ApplicationName = "CTMS";

        private XPObjectSpaceProvider ObjectSpaceProvider;
        public XPObjectSpace ObjectSpace { get; set; }
        protected TestApplication Application;

        private readonly ModuleBase module;

        public event EventHandler<EventArgs> OnSetupObjects;

        public MSSqlDbTestBase()
        {
            module = new ModuleBase();
        }

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            InitializeImageLoader();

            ObjectSpaceProvider = CreateObjectSpaceProvider();

            Application = new TestApplication();

            AddExportedTypes(module);
            Application.Modules.Add(module);

            Application.Setup(ApplicationName, ObjectSpaceProvider);
            Application.CheckCompatibility();
            ObjectSpace = (XPObjectSpace)ObjectSpaceProvider.CreateObjectSpace();
        }

        [SetUp]
        public void Setup()
        {
            if (OnSetupObjects != null)
                OnSetupObjects(this, EventArgs.Empty);
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
            MSSqlClientHelper.DropDatabase(ServerName, DatabaseName);
            MSSqlClientHelper.CreateDatabase(ServerName, DatabaseName, DataPath);
            string connectionString = MSSqlConnectionProvider.GetConnectionString(ServerName, DatabaseName);
            return new XPObjectSpaceProvider(connectionString, null);
        }

        [TearDown]
        public void TearDown()
        {
            TestUtil.DeleteExportedObjects(module, ObjectSpace.Session);
        }

        [TestFixtureTearDown]
        public void TearDownFixture()
        {
            MSSqlClientHelper.DropDatabase(ServerName, DatabaseName);
        }

        public virtual void AddExportedTypes(ModuleBase module)
        {
            TestUtil.AddExportedTypes(module);
        }
    }
}
