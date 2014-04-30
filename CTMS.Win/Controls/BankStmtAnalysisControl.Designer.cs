namespace CTMS.Win.Controls
{
    partial class BankStmtAnalysisControl
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
            DevExpress.XtraPivotGrid.PivotGridGroup pivotGridGroup1 = new DevExpress.XtraPivotGrid.PivotGridGroup();
            this.fieldYears1 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldHalfYears1 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldQuarters1 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldMonths1 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldDate1 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.pivotGridControl1 = new DevExpress.XtraPivotGrid.PivotGridControl();
            this.fieldAccountName1 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldActivityName1 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldDim111 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldDim121 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldDim131 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldCurrencyName1 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldDate2 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldHalfYears2 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldMonths2 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldQuarters2 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldYears2 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldTranAmount1 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldFunctionalCcyAmt1 = new DevExpress.XtraPivotGrid.PivotGridField();
            this.fieldCounterCcyAmt1 = new DevExpress.XtraPivotGrid.PivotGridField();
            ((System.ComponentModel.ISupportInitialize)(this.pivotGridControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // fieldYears1
            // 
            this.fieldYears1.Caption = "Years";
            this.fieldYears1.FieldName = "[Date].[Calendar Date].[Years]";
            this.fieldYears1.Name = "fieldYears1";
            this.fieldYears1.Visible = false;
            // 
            // fieldHalfYears1
            // 
            this.fieldHalfYears1.Caption = "HalfYears";
            this.fieldHalfYears1.FieldName = "[Date].[Calendar Date].[HalfYears]";
            this.fieldHalfYears1.Name = "fieldHalfYears1";
            this.fieldHalfYears1.Visible = false;
            // 
            // fieldQuarters1
            // 
            this.fieldQuarters1.Caption = "Quarters";
            this.fieldQuarters1.FieldName = "[Date].[Calendar Date].[Quarters]";
            this.fieldQuarters1.Name = "fieldQuarters1";
            this.fieldQuarters1.Visible = false;
            // 
            // fieldMonths1
            // 
            this.fieldMonths1.Caption = "Months";
            this.fieldMonths1.FieldName = "[Date].[Calendar Date].[Months]";
            this.fieldMonths1.Name = "fieldMonths1";
            this.fieldMonths1.Visible = false;
            // 
            // fieldDate1
            // 
            this.fieldDate1.Caption = "Date";
            this.fieldDate1.FieldName = "[Date].[Calendar Date].[Date]";
            this.fieldDate1.Name = "fieldDate1";
            this.fieldDate1.SortMode = DevExpress.XtraPivotGrid.PivotSortMode.Key;
            this.fieldDate1.Visible = false;
            // 
            // pivotGridControl1
            // 
            this.pivotGridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pivotGridControl1.Fields.AddRange(new DevExpress.XtraPivotGrid.PivotGridField[] {
            this.fieldAccountName1,
            this.fieldActivityName1,
            this.fieldDim111,
            this.fieldDim121,
            this.fieldDim131,
            this.fieldCurrencyName1,
            this.fieldYears1,
            this.fieldHalfYears1,
            this.fieldQuarters1,
            this.fieldMonths1,
            this.fieldDate1,
            this.fieldDate2,
            this.fieldHalfYears2,
            this.fieldMonths2,
            this.fieldQuarters2,
            this.fieldYears2,
            this.fieldTranAmount1,
            this.fieldFunctionalCcyAmt1,
            this.fieldCounterCcyAmt1});
            pivotGridGroup1.Caption = "Calendar Date";
            pivotGridGroup1.Fields.Add(this.fieldYears1);
            pivotGridGroup1.Fields.Add(this.fieldHalfYears1);
            pivotGridGroup1.Fields.Add(this.fieldQuarters1);
            pivotGridGroup1.Fields.Add(this.fieldMonths1);
            pivotGridGroup1.Fields.Add(this.fieldDate1);
            pivotGridGroup1.Hierarchy = "[Date].[Calendar Date]";
            pivotGridGroup1.ShowNewValues = true;
            this.pivotGridControl1.Groups.AddRange(new DevExpress.XtraPivotGrid.PivotGridGroup[] {
            pivotGridGroup1});
            this.pivotGridControl1.Location = new System.Drawing.Point(0, 0);
            this.pivotGridControl1.Name = "pivotGridControl1";
            this.pivotGridControl1.OLAPConnectionString = "provider=msolap;data source=169.254.183.212;initial catalog=TreasuryAnalysis;cube" +
    " name=Treasury";
            this.pivotGridControl1.OLAPDataProvider = DevExpress.XtraPivotGrid.OLAPDataProvider.Adomd;
            this.pivotGridControl1.Size = new System.Drawing.Size(620, 324);
            this.pivotGridControl1.TabIndex = 0;
            // 
            // fieldAccountName1
            // 
            this.fieldAccountName1.Caption = "Account Name";
            this.fieldAccountName1.FieldName = "[Account].[Account Name].[Account Name]";
            this.fieldAccountName1.Name = "fieldAccountName1";
            this.fieldAccountName1.Visible = false;
            // 
            // fieldActivityName1
            // 
            this.fieldActivityName1.Caption = "Activity Name";
            this.fieldActivityName1.FieldName = "[Activity].[Activity Name].[Activity Name]";
            this.fieldActivityName1.Name = "fieldActivityName1";
            this.fieldActivityName1.Visible = false;
            // 
            // fieldDim111
            // 
            this.fieldDim111.Caption = "Dim 1 1";
            this.fieldDim111.FieldName = "[Activity].[Dim 1 1].[Dim 1 1]";
            this.fieldDim111.Name = "fieldDim111";
            this.fieldDim111.Visible = false;
            // 
            // fieldDim121
            // 
            this.fieldDim121.Caption = "Dim 1 2";
            this.fieldDim121.FieldName = "[Activity].[Dim 1 2].[Dim 1 2]";
            this.fieldDim121.Name = "fieldDim121";
            this.fieldDim121.Visible = false;
            // 
            // fieldDim131
            // 
            this.fieldDim131.Caption = "Dim 1 3";
            this.fieldDim131.FieldName = "[Activity].[Dim 1 3].[Dim 1 3]";
            this.fieldDim131.Name = "fieldDim131";
            this.fieldDim131.Visible = false;
            // 
            // fieldCurrencyName1
            // 
            this.fieldCurrencyName1.Caption = "Currency Name";
            this.fieldCurrencyName1.FieldName = "[Currency].[Currency Name].[Currency Name]";
            this.fieldCurrencyName1.Name = "fieldCurrencyName1";
            this.fieldCurrencyName1.Visible = false;
            // 
            // fieldDate2
            // 
            this.fieldDate2.Caption = "Date";
            this.fieldDate2.FieldName = "[Date].[Date].[Date]";
            this.fieldDate2.Name = "fieldDate2";
            this.fieldDate2.SortMode = DevExpress.XtraPivotGrid.PivotSortMode.Key;
            this.fieldDate2.Visible = false;
            // 
            // fieldHalfYears2
            // 
            this.fieldHalfYears2.Caption = "HalfYears";
            this.fieldHalfYears2.FieldName = "[Date].[HalfYears].[HalfYears]";
            this.fieldHalfYears2.Name = "fieldHalfYears2";
            this.fieldHalfYears2.Visible = false;
            // 
            // fieldMonths2
            // 
            this.fieldMonths2.Caption = "Months";
            this.fieldMonths2.FieldName = "[Date].[Months].[Months]";
            this.fieldMonths2.Name = "fieldMonths2";
            this.fieldMonths2.Visible = false;
            // 
            // fieldQuarters2
            // 
            this.fieldQuarters2.Caption = "Quarters";
            this.fieldQuarters2.FieldName = "[Date].[Quarters].[Quarters]";
            this.fieldQuarters2.Name = "fieldQuarters2";
            this.fieldQuarters2.Visible = false;
            // 
            // fieldYears2
            // 
            this.fieldYears2.Caption = "Years";
            this.fieldYears2.FieldName = "[Date].[Years].[Years]";
            this.fieldYears2.Name = "fieldYears2";
            this.fieldYears2.Visible = false;
            // 
            // fieldTranAmount1
            // 
            this.fieldTranAmount1.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.fieldTranAmount1.Caption = "Tran Amount";
            this.fieldTranAmount1.DisplayFolder = "Bank Stmt";
            this.fieldTranAmount1.FieldName = "[Measures].[Tran Amount]";
            this.fieldTranAmount1.Name = "fieldTranAmount1";
            this.fieldTranAmount1.Visible = false;
            // 
            // fieldFunctionalCcyAmt1
            // 
            this.fieldFunctionalCcyAmt1.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.fieldFunctionalCcyAmt1.Caption = "Local Ccy Amt";
            this.fieldFunctionalCcyAmt1.DisplayFolder = "Bank Stmt";
            this.fieldFunctionalCcyAmt1.FieldName = "[Measures].[Local Ccy Amt]";
            this.fieldFunctionalCcyAmt1.Name = "fieldFunctionalCcyAmt1";
            this.fieldFunctionalCcyAmt1.Visible = false;
            // 
            // fieldCounterCcyAmt1
            // 
            this.fieldCounterCcyAmt1.Area = DevExpress.XtraPivotGrid.PivotArea.DataArea;
            this.fieldCounterCcyAmt1.Caption = "Counter Ccy Amt";
            this.fieldCounterCcyAmt1.DisplayFolder = "Bank Stmt";
            this.fieldCounterCcyAmt1.FieldName = "[Measures].[Counter Ccy Amt]";
            this.fieldCounterCcyAmt1.Name = "fieldCounterCcyAmt1";
            this.fieldCounterCcyAmt1.Visible = false;
            // 
            // BankStmtAnalysisControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pivotGridControl1);
            this.Name = "BankStmtAnalysisControl";
            this.Size = new System.Drawing.Size(620, 324);
            ((System.ComponentModel.ISupportInitialize)(this.pivotGridControl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraPivotGrid.PivotGridControl pivotGridControl1;
        private DevExpress.XtraPivotGrid.PivotGridField fieldAccountName1;
        private DevExpress.XtraPivotGrid.PivotGridField fieldActivityName1;
        private DevExpress.XtraPivotGrid.PivotGridField fieldDim111;
        private DevExpress.XtraPivotGrid.PivotGridField fieldDim121;
        private DevExpress.XtraPivotGrid.PivotGridField fieldDim131;
        private DevExpress.XtraPivotGrid.PivotGridField fieldCurrencyName1;
        private DevExpress.XtraPivotGrid.PivotGridField fieldYears1;
        private DevExpress.XtraPivotGrid.PivotGridField fieldHalfYears1;
        private DevExpress.XtraPivotGrid.PivotGridField fieldQuarters1;
        private DevExpress.XtraPivotGrid.PivotGridField fieldMonths1;
        private DevExpress.XtraPivotGrid.PivotGridField fieldDate1;
        private DevExpress.XtraPivotGrid.PivotGridField fieldDate2;
        private DevExpress.XtraPivotGrid.PivotGridField fieldHalfYears2;
        private DevExpress.XtraPivotGrid.PivotGridField fieldMonths2;
        private DevExpress.XtraPivotGrid.PivotGridField fieldQuarters2;
        private DevExpress.XtraPivotGrid.PivotGridField fieldYears2;
        private DevExpress.XtraPivotGrid.PivotGridField fieldTranAmount1;
        private DevExpress.XtraPivotGrid.PivotGridField fieldFunctionalCcyAmt1;
        private DevExpress.XtraPivotGrid.PivotGridField fieldCounterCcyAmt1;

    }
}
