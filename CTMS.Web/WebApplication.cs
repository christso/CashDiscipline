using System;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Web;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp.Security;
using Xafology.ExpressApp.Xpo;

namespace CTMS.Web
{
    public partial class CTMSAspNetApplication : WebApplication
    {
        private DevExpress.ExpressApp.SystemModule.SystemModule module1;
        private DevExpress.ExpressApp.Web.SystemModule.SystemAspNetModule module2;
        private CTMS.Module.CTMSModule module3;
        private CTMS.Module.Web.CTMSAspNetModule module4;
        private DevExpress.ExpressApp.Objects.BusinessClassLibraryCustomizationModule businessClassLibraryCustomizationModule1;
        private GenerateUserFriendlyId.Module.GenerateUserFriendlyIdModule generateUserFriendlyIdModule1;
        private DevExpress.ExpressApp.CloneObject.CloneObjectModule cloneObjectModule1;
        private DevExpress.ExpressApp.Security.SecurityStrategyComplex securityStrategyComplex1;
        private DevExpress.ExpressApp.Security.AuthenticationStandard authenticationStandard1;
        private DevExpress.ExpressApp.Security.SecurityModule securityModule1;
        private DevExpress.ExpressApp.ViewVariantsModule.ViewVariantsModule viewVariantsModule1;
        private DevExpress.ExpressApp.Validation.ValidationModule validationModule1;
        private DevExpress.ExpressApp.ScriptRecorder.ScriptRecorderModuleBase scriptRecorderModuleBase1;
        private DevExpress.ExpressApp.ScriptRecorder.Web.ScriptRecorderAspNetModule scriptRecorderAspNetModule1;
        private DevExpress.ExpressApp.PivotGrid.PivotGridModule pivotGridModule1;
        private DevExpress.ExpressApp.Reports.ReportsModule reportsModule1;
        private DevExpress.ExpressApp.Reports.Web.ReportsAspNetModule reportsAspNetModule1;
        private DevExpress.ExpressApp.PivotGrid.Web.PivotGridAspNetModule pivotGridAspNetModule1;
        private Xafology.ExpressApp.SystemModule.XafologySystemModule XafologySystemModule1;
        private Xafology.ExpressApp.PivotGrid.Web.XafologyPivotGridWebModule XafologyPivotGridWebModule1;
        private DevExpress.ExpressApp.FileAttachments.Web.FileAttachmentsAspNetModule fileAttachmentsAspNetModule1;
        private Xafology.ExpressApp.PivotGridLayout.PivotGridLayoutModule pivotGridLayoutModule1;
        private XpoModule xpoModule1;
        private Xafology.ExpressApp.PivotGridLayout.Web.PivotGridLayoutAspNetModule pivotGridLayoutAspNetModule1;
        private Xafology.ExpressApp.Concurrency.ConcurrencyModule concurrencyModule1;
        private Xafology.ExpressApp.Layout.LayoutModule layoutModule1;
        private Xafology.ExpressApp.Layout.Web.LayoutAspNetModule layoutAspNetModule1;
        private Xafology.ExpressApp.Web.SystemModule.XafologySystemAspNetModule XafologySystemAspNetModule1;
        private Xafology.ExpressApp.MsoExcel.MsoExcelModule msoExcelModule1;
        private System.Data.SqlClient.SqlConnection sqlConnection1;

        public CTMSAspNetApplication()
        {
            InitializeComponent();
        }

        // Allow edit both collection and non-collection properties at the same time
        // http://documentation.devexpress.com/#xaf/CustomDocument3230
        protected override void OnLoggedOn(LogonEventArgs args)
        {
            base.OnLoggedOn(args);
            //((ShowViewStrategy)base.ShowViewStrategy).CollectionsEditMode =
            //    DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
        }

        protected override void CreateDefaultObjectSpaceProvider(CreateCustomObjectSpaceProviderEventArgs args)
        {
            args.ObjectSpaceProvider = new D2NObjectSpaceProvider(args.ConnectionString, args.Connection, true);
        }

        private void CTMSAspNetApplication_DatabaseVersionMismatch(object sender, DevExpress.ExpressApp.DatabaseVersionMismatchEventArgs e)
        {
#if EASYTEST
			e.Updater.Update();
			e.Handled = true;
#else
            e.Updater.Update();
            e.Handled = true;
            //if (System.Diagnostics.Debugger.IsAttached)
            //{
            //    e.Updater.Update();
            //    e.Handled = true;
            //}
            //else
            //{
            //    string message = "The application cannot connect to the specified database, because the latter doesn't exist or its version is older than that of the application.\r\n" +
            //        "This error occurred  because the automatic database update was disabled when the application was started without debugging.\r\n" +
            //        "To avoid this error, you should either start the application under Visual Studio in debug mode, or modify the " +
            //        "source code of the 'DatabaseVersionMismatch' event handler to enable automatic database update, " +
            //        "or manually create a database using the 'DBUpdater' tool.\r\n" +
            //        "Anyway, refer to the following help topics for more detailed information:\r\n" +
            //        "'Update Application and Database Versions' at http://www.devexpress.com/Help/?document=ExpressApp/CustomDocument2795.htm\r\n" +
            //        "'Database Security References' at http://www.devexpress.com/Help/?document=ExpressApp/CustomDocument3237.htm\r\n" +
            //        "If this doesn't help, please contact our Support Team at http://www.devexpress.com/Support/Center/";

            //    if (e.CompatibilityError != null && e.CompatibilityError.Exception != null)
            //    {
            //        message += "\r\n\r\nInner exception: " + e.CompatibilityError.Exception.Message;
            //    }
            //    throw new InvalidOperationException(message);
            //}
#endif
        }

        private void InitializeComponent()
        {
            this.module1 = new DevExpress.ExpressApp.SystemModule.SystemModule();
            this.module2 = new DevExpress.ExpressApp.Web.SystemModule.SystemAspNetModule();
            this.module3 = new CTMS.Module.CTMSModule();
            this.module4 = new CTMS.Module.Web.CTMSAspNetModule();
            this.sqlConnection1 = new System.Data.SqlClient.SqlConnection();
            this.businessClassLibraryCustomizationModule1 = new DevExpress.ExpressApp.Objects.BusinessClassLibraryCustomizationModule();
            this.generateUserFriendlyIdModule1 = new GenerateUserFriendlyId.Module.GenerateUserFriendlyIdModule();
            this.cloneObjectModule1 = new DevExpress.ExpressApp.CloneObject.CloneObjectModule();
            this.securityStrategyComplex1 = new DevExpress.ExpressApp.Security.SecurityStrategyComplex();
            this.authenticationStandard1 = new DevExpress.ExpressApp.Security.AuthenticationStandard();
            this.securityModule1 = new DevExpress.ExpressApp.Security.SecurityModule();
            this.viewVariantsModule1 = new DevExpress.ExpressApp.ViewVariantsModule.ViewVariantsModule();
            this.validationModule1 = new DevExpress.ExpressApp.Validation.ValidationModule();
            this.scriptRecorderModuleBase1 = new DevExpress.ExpressApp.ScriptRecorder.ScriptRecorderModuleBase();
            this.scriptRecorderAspNetModule1 = new DevExpress.ExpressApp.ScriptRecorder.Web.ScriptRecorderAspNetModule();
            this.pivotGridModule1 = new DevExpress.ExpressApp.PivotGrid.PivotGridModule();
            this.reportsModule1 = new DevExpress.ExpressApp.Reports.ReportsModule();
            this.reportsAspNetModule1 = new DevExpress.ExpressApp.Reports.Web.ReportsAspNetModule();
            this.pivotGridAspNetModule1 = new DevExpress.ExpressApp.PivotGrid.Web.PivotGridAspNetModule();
            this.XafologySystemModule1 = new Xafology.ExpressApp.SystemModule.XafologySystemModule();
            this.XafologyPivotGridWebModule1 = new Xafology.ExpressApp.PivotGrid.Web.XafologyPivotGridWebModule();
            this.fileAttachmentsAspNetModule1 = new DevExpress.ExpressApp.FileAttachments.Web.FileAttachmentsAspNetModule();
            this.pivotGridLayoutModule1 = new Xafology.ExpressApp.PivotGridLayout.PivotGridLayoutModule();
            this.xpoModule1 = new Xafology.ExpressApp.Xpo.XpoModule();
            this.pivotGridLayoutAspNetModule1 = new Xafology.ExpressApp.PivotGridLayout.Web.PivotGridLayoutAspNetModule();
            this.concurrencyModule1 = new Xafology.ExpressApp.Concurrency.ConcurrencyModule();
            this.layoutModule1 = new Xafology.ExpressApp.Layout.LayoutModule();
            this.layoutAspNetModule1 = new Xafology.ExpressApp.Layout.Web.LayoutAspNetModule();
            this.XafologySystemAspNetModule1 = new Xafology.ExpressApp.Web.SystemModule.XafologySystemAspNetModule();
            this.msoExcelModule1 = new Xafology.ExpressApp.MsoExcel.MsoExcelModule();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // sqlConnection1
            // 
            this.sqlConnection1.ConnectionString = "Integrated Security=SSPI;Pooling=false;Data Source=.\\SQLEXPRESS;Initial Catalog=C" +
    "TMS";
            this.sqlConnection1.FireInfoMessageEventOnUserErrors = false;
            // 
            // securityStrategyComplex1
            // 
            this.securityStrategyComplex1.Authentication = this.authenticationStandard1;
            this.securityStrategyComplex1.RoleType = typeof(DevExpress.ExpressApp.Security.Strategy.SecuritySystemRole);
            this.securityStrategyComplex1.UserType = typeof(DevExpress.ExpressApp.Security.Strategy.SecuritySystemUser);
            // 
            // authenticationStandard1
            // 
            this.authenticationStandard1.LogonParametersType = typeof(DevExpress.ExpressApp.Security.AuthenticationStandardLogonParameters);
            // 
            // validationModule1
            // 
            this.validationModule1.AllowValidationDetailsAccess = true;
            this.validationModule1.IgnoreWarningAndInformationRules = false;
            // 
            // reportsModule1
            // 
            this.reportsModule1.EnableInplaceReports = true;
            this.reportsModule1.ReportDataType = typeof(DevExpress.Persistent.BaseImpl.ReportData);
            // 
            // CTMSAspNetApplication
            // 
            this.ApplicationName = "CTMS";
            this.Connection = this.sqlConnection1;
            this.Modules.Add(this.module1);
            this.Modules.Add(this.module2);
            this.Modules.Add(this.businessClassLibraryCustomizationModule1);
            this.Modules.Add(this.generateUserFriendlyIdModule1);
            this.Modules.Add(this.cloneObjectModule1);
            this.Modules.Add(this.viewVariantsModule1);
            this.Modules.Add(this.validationModule1);
            this.Modules.Add(this.pivotGridModule1);
            this.Modules.Add(this.securityModule1);
            this.Modules.Add(this.pivotGridLayoutModule1);
            this.Modules.Add(this.XafologySystemModule1);
            this.Modules.Add(this.concurrencyModule1);
            this.Modules.Add(this.xpoModule1);
            this.Modules.Add(this.layoutModule1);
            this.Modules.Add(this.msoExcelModule1);
            this.Modules.Add(this.module3);
            this.Modules.Add(this.scriptRecorderModuleBase1);
            this.Modules.Add(this.scriptRecorderAspNetModule1);
            this.Modules.Add(this.reportsModule1);
            this.Modules.Add(this.reportsAspNetModule1);
            this.Modules.Add(this.pivotGridAspNetModule1);
            this.Modules.Add(this.XafologyPivotGridWebModule1);
            this.Modules.Add(this.fileAttachmentsAspNetModule1);
            this.Modules.Add(this.pivotGridLayoutAspNetModule1);
            this.Modules.Add(this.layoutAspNetModule1);
            this.Modules.Add(this.XafologySystemAspNetModule1);
            this.Modules.Add(this.module4);
            this.Security = this.securityStrategyComplex1;
            this.DatabaseVersionMismatch += new System.EventHandler<DevExpress.ExpressApp.DatabaseVersionMismatchEventArgs>(this.CTMSAspNetApplication_DatabaseVersionMismatch);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #region Skip Logon

        protected override void OnCustomProcessShortcut(CustomProcessShortcutEventArgs args)
        {
            base.OnCustomProcessShortcut(args);
            if (args.Shortcut["UserName"] == "Anonymous")
            {

            }
        }
        public void Logon(string userName, string password)
        {
            ((AuthenticationStandardLogonParameters)SecuritySystem.LogonParameters).UserName = userName;
            ((AuthenticationStandardLogonParameters)SecuritySystem.LogonParameters).Password = password;
            Logon(userName, "");
        }
        #endregion
    }
}
