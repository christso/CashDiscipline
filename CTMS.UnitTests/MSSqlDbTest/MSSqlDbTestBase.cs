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

using gufi = GenerateUserFriendlyId.Module;
using Xafology.Utils.Data;

namespace CTMS.UnitTests.MSSqlDbTest
{
    public class MSSqlDbTestBase
    {
        private const string DataPath = @"D:\CTSO\Data\MSSQL12\Data";
        private const string ServerName = @"(localdb)\ProjectsV12";
        private const string DatabaseName = "CTMS_Test";
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

            // add other modules
            gufi.GenerateUserFriendlyIdModule generateUserFriendlyIdModule1 = new gufi.GenerateUserFriendlyIdModule();
            Application.Modules.Add(generateUserFriendlyIdModule1);

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
            MSSqlClientHelper.DropDatabase(ServerName, DatabaseName);
            MSSqlClientHelper.CreateDatabase(ServerName, DatabaseName, DataPath);
            string connectionString = MSSqlConnectionProvider.GetConnectionString(ServerName, DatabaseName);
            return new XPObjectSpaceProvider(connectionString, null);
        }

        protected virtual void SetupObjects()
        {
        }

        [TearDown]
        public void TearDown()
        {
            Application = null;
            MSSqlClientHelper.DropDatabase(ServerName, DatabaseName);
        }
    }
}
