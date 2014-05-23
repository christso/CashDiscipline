namespace CTMS.Module.Web
{
    partial class CTMSAspNetModule
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
            // 
            // CTMSAspNetModule
            // 
            this.RequiredModuleTypes.Add(typeof(CTMS.Module.CTMSModule));
            this.RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Web.SystemModule.SystemAspNetModule));
            this.RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.ScriptRecorder.Web.ScriptRecorderAspNetModule));
            this.RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Reports.Web.ReportsAspNetModule));
            this.RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.PivotGrid.Web.PivotGridAspNetModule));
            this.RequiredModuleTypes.Add(typeof(D2NXAF.ExpressApp.PivotGrid.Web.D2NXAFPivotGridWebModule));
            this.RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.FileAttachments.Web.FileAttachmentsAspNetModule));
            this.RequiredModuleTypes.Add(typeof(D2NXAF.ExpressApp.PivotGridLayout.Web.PivotGridLayoutAspNetModule));
        }

        #endregion
    }
}
