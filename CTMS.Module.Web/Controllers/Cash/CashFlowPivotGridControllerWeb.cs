using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.PivotGrid.Web;
using DevExpress.Web.ASPxPivotGrid;
using DevExpress.ExpressApp.Web.Editors.ASPx;
using DevExpress.Web;
using DevExpress.XtraPivotGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTMS.Module.Controllers.Cash;
using Xafology.PivotGrid;
using System.IO;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Utils;
using DevExpress.ExpressApp.Actions;
using CTMS.Module.ControllerHelpers.Cash;
using Xafology.ExpressApp.PivotGridLayout.Web.Controllers;
using System.Web.UI.WebControls;

namespace CTMS.Module.Web.Controllers.Cash
{
    public class CashFlowPivotGridControllerWeb : CashFlowPivotGridController
    {
        private ASPxPivotGrid _PivotGridControl;

        public CashFlowPivotGridControllerWeb()
        {
            TargetViewId = "CashFlow_PivotGridView";
        }

        public ASPxPivotGrid PivotGridControl
        {
            get
            {
                return _PivotGridControl;
            }
        }

        public CashFlowPivotGridSetup PivotSetup
        {
            get
            {
                var controller = Frame.GetController<CashFlowPivotGridLayoutControllerWeb>();
                return (CashFlowPivotGridSetup)controller.PivotGridSetupObject;
            }
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            var pgEditor = (ASPxPivotGridListEditor)((ListView)View).Editor;
            if (pgEditor != null)
                _PivotGridControl = (ASPxPivotGrid)pgEditor.PivotGridControl;

            PivotGridControl.OptionsData.DataFieldUnboundExpressionMode = DataFieldUnboundExpressionMode.UseSummaryValues;

            //PivotGridControl.CustomCellDisplayText += pgControl_CustomCellDisplayText;
            //PivotGridControl.CustomCellValue += pgControl_CustomCellValue;
            //PivotGridControl.CustomFieldSort += pgControl_CustomFieldSort;
            PivotGridControl.CustomSummary += pgControl_CustomSummary;

            //PivotGridControl.CellDoubleClick += pgControl_CellDoubleClick;
        }

        //TODO: create platform-agnostic version
        private void pgControl_CustomSummary(object sender, PivotGridCustomSummaryEventArgs e)
        {
            PivotGridFieldBase pgfieldSnapshot = PivotGridControl.Fields.GetFieldByName(PivotSetup.fieldSnapshot.Name);

            if (PivotSetup.IsVarianceField(e.DataField)
                && pgfieldSnapshot != null
                && ReportParam.IsVarianceMode)
            {
                PivotDrillDownDataSource ds = e.CreateDrillDownDataSource();
                decimal variance = 0;
                for (int i = 0; i < ds.RowCount; i++)
                {
                    PivotDrillDownDataRow row = ds[i];
                    decimal amount = (decimal)row[e.DataField.Name];
                    if (Convert.ToString(row[pgfieldSnapshot]) == ReportParam.Snapshot1.Name)
                        variance += amount;
                    else
                        variance -= amount;
                }
                e.CustomValue = variance;
            }
        }

        //private void pgControl_CustomCellDisplayText(object sender, PivotCellDisplayTextEventArgs e)
        //{

        //}

        //private void pgControl_CustomFieldSort(object sender, PivotGridCustomFieldSortEventArgs e)
        //{
            
        //}

        //// TODO: create platform-agnostic version
        //private void pgControl_CustomCellValue(object sender, PivotCellValueEventArgs e)
        //{
        //    var pgfieldSnapshot = PivotGridControlFields.GetFieldByName(fieldSnapshot.Name);
        //    if (pgfieldSnapshot == null) return;
        //    var snapshot = Convert.ToString(e.GetFieldValue((PivotGridField)pgfieldSnapshot));

        //    if (IsVarianceField(e.DataField)
        //        && ReportParam.IsVarianceMode
        //        && snapshot == ReportParam.Snapshot2.Name)
        //    {
        //        if (snapshot == ReportParam.Snapshot2.Name)
        //        {
        //            if (e.Value != null)
        //                e.Value = Convert.ToDecimal(e.Value) * -1;
        //        }
        //    }
        //}

        //protected override void RefreshNoteField()
        //{
        //    foreach (PivotGridField field in PivotGridControl.Fields)
        //    {
        //        if (field.Caption == "Note")
        //        {
        //            field.Area = PivotArea.FilterArea;
        //            field.Area = PivotArea.DataArea;
        //        }
        //    }
        //}
    }
}
