namespace CashDiscipline.Module.Win {
    partial class CashDisciplineWindowsFormsModule {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {

            this.RequiredModuleTypes.Add(typeof(CashDiscipline.Module.CashDisciplineModule));
            this.RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Win.SystemModule.SystemWindowsFormsModule));
			this.RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Validation.Win.ValidationWindowsFormsModule));

            this.RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.ScriptRecorder.Win.ScriptRecorderWindowsFormsModule));
            this.RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.FileAttachments.Win.FileAttachmentsWindowsFormsModule));
            this.RequiredModuleTypes.Add(typeof(Xafology.ExpressApp.Layout.LayoutModule));
            this.RequiredModuleTypes.Add(typeof(Xafology.ExpressApp.Layout.Win.LayoutWindowsFormsModule));
            this.RequiredModuleTypes.Add(typeof(Xafology.ExpressApp.Win.SystemModule.XafologySystemWindowsFormsModule));
            this.RequiredModuleTypes.Add(typeof(Xafology.ExpressApp.BatchDelete.Win.BatchDeleteWinModule));
        }

        #endregion
    }
}