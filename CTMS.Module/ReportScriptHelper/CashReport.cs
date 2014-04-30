using System;
using DevExpress.XtraPivotGrid;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.UI.PivotGrid;
using DevExpress.Data.ChartDataSources;

namespace CTMS.Module.ReportScriptHelper
{
    public class CashReport
    {
        public void PivotGrid_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            XRPivotGridField fieldTranDate;
            XRPivotGridField fieldFunctionalCcyAmt;
            XRPivotGridField fieldActivity;
            XRPivotGridField fieldDim_1_1;
            XRPivotGridField fieldDim_1_2;
            XRPivotGridField fieldDim_1_3;

            IPivotGrid pg = (XRPivotGrid)sender;
            
            XRPivotGrid pgControl = (XRPivotGrid)sender;
            pgControl.OptionsData.DataFieldUnboundExpressionMode = DataFieldUnboundExpressionMode.UseSummaryValues;
            pgControl.Fields.Clear();

            fieldTranDate = new XRPivotGridField("TranDate", PivotArea.ColumnArea);
            fieldTranDate.Caption = "Tran Date";
            fieldTranDate.ValueFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            fieldTranDate.ValueFormat.FormatString = "{0:dd-MMM-yy}";
            pgControl.Fields.Add(fieldTranDate);

            fieldActivity = new XRPivotGridField("Activity.Name", PivotArea.FilterArea);
            fieldActivity.Caption = "Activity";
            pgControl.Fields.Add(fieldActivity);

            fieldDim_1_1 = new XRPivotGridField("Activity.Dim_1_1", PivotArea.RowArea);
            fieldDim_1_1.Caption = "Dim_1_1";
            pgControl.Fields.Add(fieldDim_1_1);

            fieldDim_1_2 = new XRPivotGridField("Activity.Dim_1_2", PivotArea.RowArea);
            fieldDim_1_2.Caption = "Dim_1_2";
            pgControl.Fields.Add(fieldDim_1_2);

            fieldDim_1_3 = new XRPivotGridField("Activity.Dim_1_3", PivotArea.RowArea);
            fieldDim_1_3.Caption = "Dim_1_3";
            pgControl.Fields.Add(fieldDim_1_3);

            fieldFunctionalCcyAmt = new XRPivotGridField("FunctionalCcyAmt", PivotArea.DataArea);
            fieldFunctionalCcyAmt.Caption = "Functional Ccy Amt";
            fieldFunctionalCcyAmt.CellFormat.FormatString = "n2";
            fieldFunctionalCcyAmt.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            pgControl.Fields.Add(fieldFunctionalCcyAmt);
        }
    }
}
