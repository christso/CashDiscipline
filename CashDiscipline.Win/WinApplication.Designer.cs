namespace CashDiscipline.Win {
    partial class CashDisciplineWindowsFormsApplication {
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

            this.module4 = new CashDiscipline.Module.Win.CashDisciplineWindowsFormsModule();
        
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // CashDisciplineWindowsFormsApplication
            // 
            this.ApplicationName = "CashDiscipline";
            this.CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;

            this.Modules.Add(this.module4);
            this.UseOldTemplates = false; // set to true so that Toolbar can be hidden.
            this.DatabaseVersionMismatch += new System.EventHandler<DevExpress.ExpressApp.DatabaseVersionMismatchEventArgs>(this.CashDisciplineWindowsFormsApplication_DatabaseVersionMismatch);
            this.CustomizeLanguagesList += new System.EventHandler<DevExpress.ExpressApp.CustomizeLanguagesListEventArgs>(this.CashDisciplineWindowsFormsApplication_CustomizeLanguagesList);

            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #endregion

        private CashDiscipline.Module.Win.CashDisciplineWindowsFormsModule module4;
    }
}
