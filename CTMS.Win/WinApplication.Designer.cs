namespace CTMS.Win
{
    partial class CTMSWindowsFormsApplication
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.module1 = new DevExpress.ExpressApp.SystemModule.SystemModule();
            this.module2 = new DevExpress.ExpressApp.Win.SystemModule.SystemWindowsFormsModule();
            this.module3 = new CTMS.Module.CTMSModule();
            this.module4 = new CTMS.Module.Win.CTMSWindowsFormsModule();
            this.sqlConnection1 = new System.Data.SqlClient.SqlConnection();
            this.businessClassLibraryCustomizationModule1 = new DevExpress.ExpressApp.Objects.BusinessClassLibraryCustomizationModule();
            this.generateUserFriendlyIdModule1 = new GenerateUserFriendlyId.Module.GenerateUserFriendlyIdModule();
            this.cloneObjectModule1 = new DevExpress.ExpressApp.CloneObject.CloneObjectModule();
            this.securityStrategyComplex1 = new DevExpress.ExpressApp.Security.SecurityStrategyComplex();
            this.authenticationStandard1 = new DevExpress.ExpressApp.Security.AuthenticationStandard();
            this.securityModule1 = new DevExpress.ExpressApp.Security.SecurityModule();
            this.viewVariantsModule1 = new DevExpress.ExpressApp.ViewVariantsModule.ViewVariantsModule();
            this.validationModule1 = new DevExpress.ExpressApp.Validation.ValidationModule();
            this.pivotGridModule1 = new DevExpress.ExpressApp.PivotGrid.PivotGridModule();
            this.reportsModule1 = new DevExpress.ExpressApp.Reports.ReportsModule();
            this.scriptRecorderModuleBase1 = new DevExpress.ExpressApp.ScriptRecorder.ScriptRecorderModuleBase();
            this.scriptRecorderWindowsFormsModule1 = new DevExpress.ExpressApp.ScriptRecorder.Win.ScriptRecorderWindowsFormsModule();
            this.reportsWindowsFormsModule1 = new DevExpress.ExpressApp.Reports.Win.ReportsWindowsFormsModule();
            this.pivotGridWindowsFormsModule1 = new DevExpress.ExpressApp.PivotGrid.Win.PivotGridWindowsFormsModule();
            this.pivotChartModuleBase1 = new DevExpress.ExpressApp.PivotChart.PivotChartModuleBase();
            this.pivotChartWindowsFormsModule1 = new DevExpress.ExpressApp.PivotChart.Win.PivotChartWindowsFormsModule();
            this.ctmsPivotGridWinModule1 = new D2NXAF.ExpressApp.PivotGrid.Win.D2NXAFPivotGridWinModule();
            this.D2NXAFSystemModule1 = new D2NXAF.ExpressApp.SystemModule.D2NXAFSystemModule();
            this.fileAttachmentsWindowsFormsModule1 = new DevExpress.ExpressApp.FileAttachments.Win.FileAttachmentsWindowsFormsModule();
            this.pivotGridLayoutModule1 = new D2NXAF.ExpressApp.PivotGridLayout.PivotGridLayoutModule();
            this.xpoModule1 = new D2NXAF.ExpressApp.Xpo.XpoModule();
            this.concurrencyModule1 = new D2NXAF.ExpressApp.Concurrency.ConcurrencyModule();
            this.pivotGridLayoutWindowsFormsModule1 = new D2NXAF.ExpressApp.PivotGridLayout.Win.PivotGridLayoutWindowsFormsModule();
            this.layoutModule1 = new D2NXAF.ExpressApp.Layout.LayoutModule();
            this.layoutWindowsFormsModule1 = new D2NXAF.ExpressApp.Layout.Win.LayoutWindowsFormsModule();
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
            // securityModule1
            // 
            this.securityModule1.UserType = typeof(DevExpress.ExpressApp.Security.Strategy.SecuritySystemUser);
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
            // pivotChartModuleBase1
            // 
            this.pivotChartModuleBase1.ShowAdditionalNavigation = false;
            // 
            // CTMSWindowsFormsApplication
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
            this.Modules.Add(this.reportsModule1);
            this.Modules.Add(this.securityModule1);
            this.Modules.Add(this.pivotGridLayoutModule1);
            this.Modules.Add(this.xpoModule1);
            this.Modules.Add(this.D2NXAFSystemModule1);
            this.Modules.Add(this.concurrencyModule1);
            this.Modules.Add(this.layoutModule1);
            this.Modules.Add(this.module3);
            this.Modules.Add(this.scriptRecorderModuleBase1);
            this.Modules.Add(this.scriptRecorderWindowsFormsModule1);
            this.Modules.Add(this.reportsWindowsFormsModule1);
            this.Modules.Add(this.pivotGridWindowsFormsModule1);
            this.Modules.Add(this.pivotChartModuleBase1);
            this.Modules.Add(this.pivotChartWindowsFormsModule1);
            this.Modules.Add(this.ctmsPivotGridWinModule1);
            this.Modules.Add(this.fileAttachmentsWindowsFormsModule1);
            this.Modules.Add(this.pivotGridLayoutWindowsFormsModule1);
            this.Modules.Add(this.layoutWindowsFormsModule1);
            this.Modules.Add(this.module4);
            this.Security = this.securityStrategyComplex1;
            this.DatabaseVersionMismatch += new System.EventHandler<DevExpress.ExpressApp.DatabaseVersionMismatchEventArgs>(this.CTMSWindowsFormsApplication_DatabaseVersionMismatch);
            this.CustomizeLanguagesList += new System.EventHandler<DevExpress.ExpressApp.CustomizeLanguagesListEventArgs>(this.CTMSWindowsFormsApplication_CustomizeLanguagesList);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #endregion

        private DevExpress.ExpressApp.SystemModule.SystemModule module1;
        private DevExpress.ExpressApp.Win.SystemModule.SystemWindowsFormsModule module2;
        private CTMS.Module.CTMSModule module3;
        private CTMS.Module.Win.CTMSWindowsFormsModule module4;
        private System.Data.SqlClient.SqlConnection sqlConnection1;
        private DevExpress.ExpressApp.Objects.BusinessClassLibraryCustomizationModule businessClassLibraryCustomizationModule1;
        private GenerateUserFriendlyId.Module.GenerateUserFriendlyIdModule generateUserFriendlyIdModule1;
        private DevExpress.ExpressApp.CloneObject.CloneObjectModule cloneObjectModule1;
        private DevExpress.ExpressApp.Security.SecurityStrategyComplex securityStrategyComplex1;
        private DevExpress.ExpressApp.Security.AuthenticationStandard authenticationStandard1;
        private DevExpress.ExpressApp.Security.SecurityModule securityModule1;
        private DevExpress.ExpressApp.ViewVariantsModule.ViewVariantsModule viewVariantsModule1;
        private DevExpress.ExpressApp.Validation.ValidationModule validationModule1;
        private DevExpress.ExpressApp.PivotGrid.PivotGridModule pivotGridModule1;
        private DevExpress.ExpressApp.Reports.ReportsModule reportsModule1;
        private DevExpress.ExpressApp.ScriptRecorder.ScriptRecorderModuleBase scriptRecorderModuleBase1;
        private DevExpress.ExpressApp.ScriptRecorder.Win.ScriptRecorderWindowsFormsModule scriptRecorderWindowsFormsModule1;
        private DevExpress.ExpressApp.Reports.Win.ReportsWindowsFormsModule reportsWindowsFormsModule1;
        private DevExpress.ExpressApp.PivotGrid.Win.PivotGridWindowsFormsModule pivotGridWindowsFormsModule1;
        private DevExpress.ExpressApp.PivotChart.PivotChartModuleBase pivotChartModuleBase1;
        private DevExpress.ExpressApp.PivotChart.Win.PivotChartWindowsFormsModule pivotChartWindowsFormsModule1;
        private D2NXAF.ExpressApp.PivotGrid.Win.D2NXAFPivotGridWinModule ctmsPivotGridWinModule1;
        private D2NXAF.ExpressApp.SystemModule.D2NXAFSystemModule D2NXAFSystemModule1;
        private DevExpress.ExpressApp.FileAttachments.Win.FileAttachmentsWindowsFormsModule fileAttachmentsWindowsFormsModule1;
        private D2NXAF.ExpressApp.PivotGridLayout.PivotGridLayoutModule pivotGridLayoutModule1;
        private D2NXAF.ExpressApp.Xpo.XpoModule xpoModule1;
        private D2NXAF.ExpressApp.Concurrency.ConcurrencyModule concurrencyModule1;
        private D2NXAF.ExpressApp.PivotGridLayout.Win.PivotGridLayoutWindowsFormsModule pivotGridLayoutWindowsFormsModule1;
        private D2NXAF.ExpressApp.Layout.LayoutModule layoutModule1;
        private D2NXAF.ExpressApp.Layout.Win.LayoutWindowsFormsModule layoutWindowsFormsModule1;
    }
}
