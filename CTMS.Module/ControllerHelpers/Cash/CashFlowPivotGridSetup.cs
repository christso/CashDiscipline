using D2NXAF.ExpressApp.PivotGridLayout;
using DevExpress.XtraPivotGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTMS.Module.ControllerHelpers.Cash
{
    public class CashFlowPivotGridSetup : PivotGridSetup
    {
        public const string NoteFieldCaption = "Note";

        public PivotGridFieldBase fieldSnapshot;
        public PivotGridFieldBase fieldTranYear;
        public PivotGridFieldBase fieldTranQuarter;
        public PivotGridFieldBase fieldTranMonth;
        public PivotGridFieldBase fieldTranDate;
        public PivotGridFieldBase fieldAccountCcyAmt;
        public PivotGridFieldBase fieldFunctionalCcyAmt;
        public PivotGridFieldBase fieldAccountCcyAmtVar;
        public PivotGridFieldBase fieldFunctionalCcyAmtVar;
        public PivotGridFieldBase fieldCounterCcyAmtVar;
        public PivotGridFieldBase fieldSource;
        public PivotGridFieldBase fieldActivity;
        public PivotGridFieldBase fieldDim_1_1;
        public PivotGridFieldBase fieldDim_1_2;
        public PivotGridFieldBase fieldDim_1_3;
        public PivotGridFieldBase fieldFix;
        public PivotGridFieldBase fieldFixType;
        public PivotGridFieldBase fieldFixRank;
        public PivotGridFieldBase fieldFixerYear;
        public PivotGridFieldBase fieldFixerQuarter;
        public PivotGridFieldBase fieldFixerMonth;
        public PivotGridFieldBase fieldFixerDate;
        public PivotGridFieldBase fieldFixActivity;
        public PivotGridFieldBase fieldParentYear;
        public PivotGridFieldBase fieldParentQuarter;
        public PivotGridFieldBase fieldParentMonth;
        public PivotGridFieldBase fieldParentDate;
        public PivotGridFieldBase fieldParentSource;
        public PivotGridFieldBase fieldParentActivity;
        public PivotGridFieldBase fieldParentDim_1_1;
        public PivotGridFieldBase fieldParentDim_1_2;
        public PivotGridFieldBase fieldParentDim_1_3;
        public PivotGridFieldBase fieldCounterCcy;
        public PivotGridFieldBase fieldNote;

        public CashFlowPivotGridSetup()
        {
            Layouts.Add(new PivotGridLayout(Constants.CashFlowPivotLayoutMonthlyVariance, LayoutFields));
            Layouts.Add(new PivotGridLayout(Constants.CashFlowPivotLayoutDaily, LayoutFields));
            Layouts.Add(new PivotGridLayout(Constants.CashFlowPivotLayoutFixForecast, LayoutFields));
            Layouts.Add(new PivotGridLayout(Constants.CashFlowPivotLayoutWeekly, LayoutFields));
            Layouts.Add(new PivotGridLayout(Constants.CashFlowPivotLayoutMonthly, LayoutFields));

            #region Cash Flow
            fieldSnapshot = new PivotGridFieldBase("Snapshot", PivotArea.FilterArea);
            fieldSnapshot.Name = "fieldSnapshot";
            fieldSnapshot.Caption = "Snapshot";
            fieldSnapshot.SortMode = PivotSortMode.Custom;
            Fields.Add(fieldSnapshot);

            fieldTranYear = new PivotGridFieldBase("TranDate", PivotArea.FilterArea);
            fieldTranYear.Name = "fieldTranYear";
            fieldTranYear.Caption = "Tran Year";
            fieldTranYear.GroupInterval = PivotGroupInterval.DateYear;
            Fields.Add(fieldTranYear);

            fieldTranQuarter = new PivotGridFieldBase("TranDate", PivotArea.FilterArea);
            fieldTranQuarter.GroupInterval = PivotGroupInterval.DateQuarter;
            fieldTranQuarter.Name = "fieldTranQuarter";
            fieldTranQuarter.Caption = "Tran Quarter";
            fieldTranQuarter.ValueFormat.FormatString = "Qtr {0}";
            fieldTranQuarter.ValueFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            Fields.Add(fieldTranQuarter);

            fieldTranMonth = new PivotGridFieldBase("TranDate", PivotArea.FilterArea);
            fieldTranMonth.Name = "fieldTranMonth";
            fieldTranMonth.Caption = "Tran Month";
            fieldTranMonth.GroupInterval = PivotGroupInterval.DateMonth;
            fieldTranMonth.ValueFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            fieldTranMonth.ValueFormat.FormatString = "{0:MMM}";
            Fields.Add(fieldTranMonth);

            fieldTranDate = new PivotGridFieldBase("TranDate", PivotArea.FilterArea);
            fieldTranDate.Name = "fieldTranDate";
            fieldTranDate.Caption = "Tran Date";
            fieldTranDate.ValueFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            fieldTranDate.ValueFormat.FormatString = "{0:dd-MMM-yy}";
            Fields.Add(fieldTranDate);

            fieldAccountCcyAmt = new PivotGridFieldBase("AccountCcyAmt", PivotArea.FilterArea);
            fieldAccountCcyAmt.Name = "fieldAccountCcyAmt";
            fieldAccountCcyAmt.Caption = "Account Ccy Amt";
            fieldAccountCcyAmt.CellFormat.FormatString = "n2";
            fieldAccountCcyAmt.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            Fields.Add(fieldAccountCcyAmt);

            fieldFunctionalCcyAmt = new PivotGridFieldBase("FunctionalCcyAmt", PivotArea.FilterArea);
            fieldFunctionalCcyAmt.Name = "fieldFunctionalCcyAmt";
            fieldFunctionalCcyAmt.Caption = "Functional Ccy Amt";
            fieldFunctionalCcyAmt.CellFormat.FormatString = "n2";
            fieldFunctionalCcyAmt.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            fieldFunctionalCcyAmt.ColumnValueLineCount = 2;
            Fields.Add(fieldFunctionalCcyAmt);

            fieldAccountCcyAmtVar = new PivotGridFieldBase("AccountCcyAmt", PivotArea.FilterArea);
            fieldAccountCcyAmtVar.Name = "fieldAccountCcyAmtVarVar";
            fieldAccountCcyAmtVar.Caption = "Account Ccy Amt Var";
            fieldAccountCcyAmtVar.CellFormat.FormatString = "n2";
            fieldAccountCcyAmtVar.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            fieldAccountCcyAmtVar.SummaryType = DevExpress.Data.PivotGrid.PivotSummaryType.Custom;
            fieldAccountCcyAmtVar.ColumnValueLineCount = 2;
            Fields.Add(fieldAccountCcyAmtVar);

            fieldFunctionalCcyAmtVar = new PivotGridFieldBase("FunctionalCcyAmt", PivotArea.FilterArea);
            fieldFunctionalCcyAmtVar.Name = "fieldFunctionalCcyAmtVar";
            fieldFunctionalCcyAmtVar.Caption = "Functional Ccy Amt Var";
            fieldFunctionalCcyAmtVar.CellFormat.FormatString = "n2";
            fieldFunctionalCcyAmtVar.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            fieldFunctionalCcyAmtVar.SummaryType = DevExpress.Data.PivotGrid.PivotSummaryType.Custom;
            fieldFunctionalCcyAmtVar.ColumnValueLineCount = 2;
            Fields.Add(fieldFunctionalCcyAmtVar);

            fieldCounterCcyAmtVar = new PivotGridFieldBase("CounterCcyAmt", PivotArea.FilterArea);
            fieldCounterCcyAmtVar.Name = "fieldCounterCcyAmtVar";
            fieldCounterCcyAmtVar.Caption = "Counter Ccy Amt Var";
            fieldCounterCcyAmtVar.CellFormat.FormatString = "n2";
            fieldCounterCcyAmtVar.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            fieldCounterCcyAmtVar.SummaryType = DevExpress.Data.PivotGrid.PivotSummaryType.Custom;
            fieldCounterCcyAmtVar.ColumnValueLineCount = 2;
            Fields.Add(fieldCounterCcyAmtVar);

            fieldActivity = new PivotGridFieldBase("Activity.Name", PivotArea.FilterArea);
            fieldActivity.Name = "fieldActivity";
            fieldActivity.Caption = "Activity";
            Fields.Add(fieldActivity);

            fieldDim_1_1 = new PivotGridFieldBase("Activity.Dim_1_1", PivotArea.FilterArea);
            fieldDim_1_1.Name = "fieldDim_1_1";
            fieldDim_1_1.Caption = "Dim_1_1";
            Fields.Add(fieldDim_1_1);

            fieldDim_1_2 = new PivotGridFieldBase("Activity.Dim_1_2", PivotArea.FilterArea);
            fieldDim_1_2.Name = "fieldDim_1_2";
            fieldDim_1_2.Caption = "Dim_1_2";
            Fields.Add(fieldDim_1_2);

            fieldDim_1_3 = new PivotGridFieldBase("Activity.Dim_1_3", PivotArea.FilterArea);
            fieldDim_1_3.Name = "fieldDim_1_3";
            fieldDim_1_3.Caption = "Dim_1_3";
            Fields.Add(fieldDim_1_3);

            fieldSource = new PivotGridFieldBase("Source.Name", PivotArea.FilterArea);
            fieldSource.Name = "fieldSource";
            fieldSource.Caption = "Source";
            Fields.Add(fieldSource);

            fieldCounterCcy = new PivotGridFieldBase("CounterCcy.Name", PivotArea.FilterArea);
            fieldCounterCcy.Name = "fieldCounterCcy";
            fieldCounterCcy.Caption = "Counter Ccy";
            Fields.Add(fieldCounterCcy);

            fieldFixActivity = new PivotGridFieldBase("FixActivity.Name", PivotArea.FilterArea);
            fieldFixActivity.Name = "fieldFixActivity";
            fieldFixActivity.Caption = "Fix Activity";
            Fields.Add(fieldFixActivity);

            fieldFix = new PivotGridFieldBase("Fix.Name", PivotArea.FilterArea);
            fieldFix.Name = "fieldFix";
            fieldFix.Caption = "Fix";
            Fields.Add(fieldFix);

            fieldFixType = new PivotGridFieldBase("Fix.FixTagType", PivotArea.FilterArea);
            fieldFixType.Name = "fieldFixType";
            fieldFixType.Caption = "FixType";
            Fields.Add(fieldFixType);

            fieldNote = new PivotGridFieldBase("Note", PivotArea.FilterArea);
            fieldNote.Name = "fieldNote";
            fieldNote.UnboundType = DevExpress.Data.UnboundColumnType.String;
            fieldNote.SummaryType = DevExpress.Data.PivotGrid.PivotSummaryType.Custom;
            fieldNote.Caption = NoteFieldCaption;
            Fields.Add(fieldNote);
            #endregion

            #region Parent
            fieldParentYear = new PivotGridFieldBase("ParentTranDate", PivotArea.FilterArea);
            fieldParentYear.Name = "fieldParentYear";
            fieldParentYear.Caption = "Parent Year";
            fieldParentYear.GroupInterval = PivotGroupInterval.DateYear;
            Fields.Add(fieldParentYear);

            fieldParentQuarter = new PivotGridFieldBase("ParentTranDate", PivotArea.FilterArea);
            fieldParentQuarter.GroupInterval = PivotGroupInterval.DateQuarter;
            fieldParentQuarter.Name = "fieldParentQuarter";
            fieldParentQuarter.Caption = "Parent Quarter";
            fieldParentQuarter.ValueFormat.FormatString = "Qtr {0}";
            fieldParentQuarter.ValueFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            Fields.Add(fieldParentQuarter);

            fieldParentMonth = new PivotGridFieldBase("ParentTranDate", PivotArea.FilterArea);
            fieldParentMonth.Name = "fieldParentMonth";
            fieldParentMonth.Caption = "Parent Month";
            fieldParentMonth.GroupInterval = PivotGroupInterval.DateMonth;
            fieldParentMonth.ValueFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            fieldParentMonth.ValueFormat.FormatString = "{0:MMM}";
            Fields.Add(fieldParentMonth);

            fieldParentDate = new PivotGridFieldBase("ParentTranDate", PivotArea.FilterArea);
            fieldParentDate.Name = "fieldParentTranDate";
            fieldParentDate.Caption = "Parent Date";
            fieldParentDate.ValueFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            fieldParentDate.ValueFormat.FormatString = "{0:dd-MMM-yy}";
            Fields.Add(fieldParentDate);

            fieldParentSource = new PivotGridFieldBase("ParentSource.Name", PivotArea.FilterArea);
            fieldParentSource.Name = "fieldParentSource";
            fieldParentSource.Caption = "ParentSource";
            Fields.Add(fieldParentSource);

            fieldParentActivity = new PivotGridFieldBase("ParentActivity.Name", PivotArea.FilterArea);
            fieldParentActivity.Name = "fieldParentActivity";
            fieldParentActivity.Caption = "ParentActivity";
            Fields.Add(fieldParentActivity);

            fieldParentDim_1_1 = new PivotGridFieldBase("ParentActivity.Dim_1_1", PivotArea.FilterArea);
            fieldParentDim_1_1.Name = "fieldParentName";
            fieldParentDim_1_1.Caption = "ParentDim_1_1";
            Fields.Add(fieldParentDim_1_1);

            fieldParentDim_1_2 = new PivotGridFieldBase("ParentActivity.Dim_1_2", PivotArea.FilterArea);
            fieldParentDim_1_2.Name = "fieldParentDim";
            fieldParentDim_1_2.Caption = "ParentDim_1_2";
            Fields.Add(fieldParentDim_1_2);

            fieldParentDim_1_3 = new PivotGridFieldBase("ParentActivity.Dim_1_3", PivotArea.FilterArea);
            fieldParentDim_1_3.Name = "ParentDim_1_3";
            fieldParentDim_1_3.Caption = "ParentDim_1_3";
            Fields.Add(fieldParentDim_1_3);
            #endregion
        }

        public void LayoutFields(string layoutName)
        {
            foreach (PivotGridFieldBase field in Fields)
            {
                field.Area = PivotArea.FilterArea;
                field.Visible = false;
            }

            switch (layoutName)
            {
                case Constants.CashFlowPivotLayoutFixForecast:
                    fieldParentMonth.Visible = true;
                    fieldParentSource.Visible = true;
                    fieldParentDate.Visible = true;
                    fieldFix.Visible = true;
                    fieldFunctionalCcyAmt.Visible = true;

                    fieldParentMonth.Area = PivotArea.RowArea;
                    fieldParentSource.Area = PivotArea.RowArea;
                    fieldParentDate.Area = PivotArea.RowArea;
                    fieldFix.Area = PivotArea.ColumnArea;
                    fieldFunctionalCcyAmt.Area = PivotArea.DataArea;
                    break;
                case Constants.CashFlowPivotLayoutMonthly:
                    fieldDim_1_1.Visible = true;
                    fieldDim_1_2.Visible = true;
                    fieldDim_1_3.Visible = true;
                    fieldTranYear.Visible = true;
                    fieldTranMonth.Visible = true;
                    fieldFunctionalCcyAmt.Visible = true;

                    fieldDim_1_1.Area = PivotArea.RowArea;
                    fieldDim_1_2.Area = PivotArea.RowArea;
                    fieldDim_1_3.Area = PivotArea.RowArea;
                    fieldTranYear.Area = PivotArea.ColumnArea;
                    fieldTranMonth.Area = PivotArea.ColumnArea;
                    fieldFunctionalCcyAmt.Area = PivotArea.DataArea;
                    break;
                case Constants.CashFlowPivotLayoutMonthlyVariance:
                    fieldActivity.Visible = true;
                    fieldSnapshot.Visible = true;
                    fieldFunctionalCcyAmtVar.Visible = true;

                    fieldActivity.Area = PivotArea.RowArea;
                    fieldSnapshot.Area = PivotArea.ColumnArea;
                    fieldFunctionalCcyAmtVar.Area = PivotArea.DataArea;
                    break;
            }
        }

        public bool IsVarianceField(PivotGridFieldBase field)
        {
            return field.Name == fieldAccountCcyAmtVar.Name
                || field.Name == fieldCounterCcyAmtVar.Name
                || field.Name == fieldFunctionalCcyAmtVar.Name;
        }
    }
}
