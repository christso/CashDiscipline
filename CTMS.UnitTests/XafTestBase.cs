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
using CTMS.Module.BusinessObjects.Market;
using gufi = GenerateUserFriendlyId.Module;

namespace CTMS.UnitTests
{
    public enum DbType
    {
        SQLServer,
        PostgreSQL,
        Oracle
    }

    public class XafTestBase
    {
        protected XPObjectSpace ObjectSpace;
        protected TestApplication Application;
        protected DbType TargetDbType;

        /// <summary>
        /// This only supports MSSQL
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        protected string GetConnectionString(string database = "TreasuryTest")
        {
            string server;
            string user;
            switch (TargetDbType)
            {
                case DbType.SQLServer:
                    //server = @"(localdb)\Projects";
                    server = @".\SQLEXPRESS";
                    //MSSqlClientHelper.DropAndCreateDatabase(server, database, @"D:\Data\SQL Server");
                    D2NXAF.Utils.Data.MSSqlClientHelper.DropAndCreateDatabase(server, database, @"D:\Program Files\Microsoft SQL Server\MSSQL10.SQLEXPRESS\MSSQL\DATA");
                    return MSSqlConnectionProvider.GetConnectionString(server, database);
                case DbType.PostgreSQL:
                    server = "localhost";
                    user = "postgres";
                    return PostgreSqlConnectionProvider.GetConnectionString(server, user, "", database);
                case DbType.Oracle:
                    return "";
                default:
                    throw new ArgumentException("Invalid DbType.");
            }
        }
        [SetUp]
        public void Setup()
        {
            var classType = GetType();
            if (!ImageLoader.IsInitialized)
            {
                ImageLoader.Init(new AssemblyResourceImageSource(classType.Assembly.FullName, "Images"));
            }

            string connectionString = GetConnectionString();

            XPObjectSpaceProvider objectSpaceProvider =
                new XPObjectSpaceProvider(connectionString, null);
            Application = new TestApplication();
            //Application.ConnectionString = connectionString;
            Application.ApplicationName = "CTMS";


            ModuleBase testModule = new ModuleBase();
            gufi.GenerateUserFriendlyIdModule generateUserFriendlyIdModule1 = new gufi.GenerateUserFriendlyIdModule();

            testModule.AdditionalExportedTypes.Add(typeof(SetOfBooks));
            testModule.AdditionalExportedTypes.Add(typeof(BankStmt));
            testModule.AdditionalExportedTypes.Add(typeof(CashFlow));
            testModule.AdditionalExportedTypes.Add(typeof(CashFlowDefaults));
            testModule.AdditionalExportedTypes.Add(typeof(Account));
            testModule.AdditionalExportedTypes.Add(typeof(FinActivity));
            testModule.AdditionalExportedTypes.Add(typeof(FinAccount));
            testModule.AdditionalExportedTypes.Add(typeof(FinJournalGroup));
            testModule.AdditionalExportedTypes.Add(typeof(FinGenJournalParam));
            testModule.AdditionalExportedTypes.Add(typeof(GenLedger));
            testModule.AdditionalExportedTypes.Add(typeof(GlAccount));
            testModule.AdditionalExportedTypes.Add(typeof(GlCompany));
            testModule.AdditionalExportedTypes.Add(typeof(GlCountry));
            testModule.AdditionalExportedTypes.Add(typeof(GlIntercompany));
            testModule.AdditionalExportedTypes.Add(typeof(GlLocation));
            testModule.AdditionalExportedTypes.Add(typeof(GlProduct));
            testModule.AdditionalExportedTypes.Add(typeof(GlProject));
            testModule.AdditionalExportedTypes.Add(typeof(GlSalesChannel));
            testModule.AdditionalExportedTypes.Add(typeof(FinAccountingDefaults));
            testModule.AdditionalExportedTypes.Add(typeof(ForexTrade));
            testModule.AdditionalExportedTypes.Add(typeof(ForexCounterparty));
            testModule.AdditionalExportedTypes.Add(typeof(CashFlowFixParam));
            testModule.AdditionalExportedTypes.Add(typeof(ForexRate));
            testModule.AdditionalExportedTypes.Add(typeof(BankStmtCashFlowForecast));
            testModule.AdditionalExportedTypes.Add(typeof(ForexStdSettleAccount));
            testModule.AdditionalExportedTypes.Add(typeof(ForexTradePredelivery));
            testModule.AdditionalExportedTypes.Add(typeof(AustPostSettle));
            testModule.AdditionalExportedTypes.Add(typeof(CashReportParam));
            testModule.AdditionalExportedTypes.Add(typeof(AccountSummary));

            Application.Modules.Add(testModule);
            Application.Modules.Add(generateUserFriendlyIdModule1);

            Application.Setup("CTMS", objectSpaceProvider);
            Application.CheckCompatibility();
            ObjectSpace = (XPObjectSpace)objectSpaceProvider.CreateObjectSpace();

            SetupObjects();
        }

        protected virtual void SetupObjects()
        {
        }
    }
}
