using CTMS.Module.ControllerHelpers.Cash;
using CTMS.Module.Controllers.Cash;
using CTMS.Module.HelperClasses.UI;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.PivotGrid.Win;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.XtraPivotGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTMS.Module.Win.Controllers.Cash
{
    public class CashFlowPivotGridControllerWin : CashFlowPivotGridController
    {
        private PivotGridControl _PivotGridControl;

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            var pgEditor = ((ListView)View).Editor as PivotGridListEditor;
            if (pgEditor != null)
                _PivotGridControl = pgEditor.PivotGridControl;

            PivotGridControl.OptionsData.DataFieldUnboundExpressionMode = DataFieldUnboundExpressionMode.UseSummaryValues;

            PivotGridControl.CustomCellDisplayText += pgControl_CustomCellDisplayText;
            PivotGridControl.CustomCellValue += pgControl_CustomCellValue;
            PivotGridControl.CellDoubleClick += pgControl_CellDoubleClick;
            PivotGridControl.CustomFieldSort += pgControl_CustomFieldSort;
            PivotGridControl.CustomSummary += pgControl_CustomSummary;
        }

        public PivotGridControl PivotGridControl
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
                var controller = Frame.GetController<CashFlowPivotGridLayoutControllerWin>();
                return (CashFlowPivotGridSetup)controller.PivotGridSetupObject;
            }
        }

        protected override void SetupView()
        {
            base.SetupView();
            var pgControl = PivotGridControl;
            if (pgControl != null && !pgControl.IsDisposed)
            {
                pgControl.RefreshData();
            }
        }

        #region Pivot Grid Control Events

        protected void pgControl_CellDoubleClick(object sender, PivotCellEventArgs e)
        {
            if (e.DataField.Caption == fieldNoteCaption)
            {
                var objSpace = Application.CreateObjectSpace();
                var noteObj = GetNoteObject(e, objSpace, true);
                if (noteObj != null)
                {
                    // remember location of cell that was double-clicked
                    DoubleClickPivotCellEventArgs = e;
                    var dialog = new PopupDialogDetailViewManager(Application);
                    dialog.Accepting += noteDialogAccepting;
                    dialog.ShowView(objSpace, noteObj);
                }
            }
        }

        /// <summary>
        /// Places Snapshot1 in front of Snapshot2
        /// </summary>
        protected void pgControl_CustomFieldSort(object sender, PivotGridCustomFieldSortEventArgs e)
        {
            if (e.Field.Caption == "Snapshot" && ReportParam.IsVarianceMode)
            {

                if (e.Value1 == null || e.Value2 == null) return;
                e.Handled = true;
                string s1 = e.Value1.ToString();
                string s2 = e.Value2.ToString();
                if (s1 == ReportParam.Snapshot1.Name && s2 == ReportParam.Snapshot2.Name)
                    e.Result = -1;
                else if (s2 == ReportParam.Snapshot1.Name && s1 == ReportParam.Snapshot2.Name)
                    e.Result = 1;
                else
                    e.Result = 0;
            }
        }

        protected void pgControl_CustomCellValue(object sender, PivotCellValueEventArgs e)
        {
            var pgfieldSnapshot = PivotGridControl.Fields.GetFieldByName(PivotSetup.fieldSnapshot.Name);
            if (pgfieldSnapshot == null) return;
            var snapshot = Convert.ToString(e.GetFieldValue((PivotGridField)pgfieldSnapshot));

            if (PivotSetup.IsVarianceField(e.DataField)
                && ReportParam.IsVarianceMode
                && snapshot == ReportParam.Snapshot2.Name)
            {
                if (snapshot == ReportParam.Snapshot2.Name)
                {
                    if (e.Value != null)
                        e.Value = Convert.ToDecimal(e.Value) * -1;
                }
            }
        }

        protected void pgControl_CustomSummary(object sender, PivotGridCustomSummaryEventArgs e)
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

        protected void pgControl_CustomCellDisplayText(object sender, PivotCellDisplayTextEventArgs e)
        {
            if (e.DataField.Caption == CashFlowPivotGridSetup.NoteFieldCaption)
            {
                var noteObj = GetNoteObject(e, ObjectSpace, false);
                if (noteObj != null && !string.IsNullOrEmpty(noteObj.Text))
                {
                    e.DisplayText = noteObj.Text;
                }
            }
        }

        void noteDialogAccepting(object sender, DialogControllerAcceptingEventArgs e)
        {
            RefreshNoteField();
        }

        #endregion

        protected void RefreshNoteField()
        {
            foreach (PivotGridField field in PivotGridControl.Fields)
            {
                if (field.Caption == "Note")
                {
                    field.Area = PivotArea.FilterArea;
                    field.Area = PivotArea.DataArea;
                }
            }
        }
    }
}
