using CTMS.Module.Controllers.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.PivotGrid.Win;
using DevExpress.XtraPivotGrid;
using D2NXAF.PivotGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CTMS.Module.BusinessObjects.Cash;
using System.IO;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp.Actions;
using CTMS.Module.ParamObjects.Cash;
using CTMS.Module.HelperClasses.UI;
using DevExpress.ExpressApp.SystemModule;
using CTMS.Module.BusinessObjects.User;
using DevExpress.Data.Filtering;

namespace CTMS.Module.Win.Controllers.Cash
{
    public class CashFlowPivotGridControllerWin : CashFlowPivotGridController
    {
        public CashFlowPivotGridControllerWin()
        {
            var layoutTestAction = new SingleChoiceAction(this, "LayoutTestAction", DevExpress.Persistent.Base.PredefinedCategory.View);
            layoutTestAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;

            var loadChoice = new ChoiceActionItem();
            loadChoice.Caption = "Load";
            layoutTestAction.Items.Add(loadChoice);

            var saveChoice = new ChoiceActionItem();
            saveChoice.Caption = "Save";
            layoutTestAction.Items.Add(saveChoice);
            layoutTestAction.Execute += loadLayoutTestAction_Execute;
        }

        void loadLayoutTestAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var layoutObj = ObjectSpace.FindObject<UserViewLayout>(CriteriaOperator.Parse("LayoutName = ?", "Debug"));
            if (layoutObj == null)
            {
                layoutObj = ObjectSpace.CreateObject<UserViewLayout>();
                layoutObj.LayoutName = "Debug";
            }

            switch (e.SelectedChoiceActionItem.Caption)
            {
                case "Load":
                    LoadLayoutTest(layoutObj);
                    break;
                case "Save":
                    SaveLayoutTest(layoutObj);
                    break;
            }
        }

        private void LoadLayoutTest(UserViewLayout layoutObj)
        {
            var stream = new MemoryStream(layoutObj.LayoutFile.Content);
            stream.Position = 0;
            PivotGridControl.RestoreLayoutFromStream(stream);
        }

        private void SaveLayoutTest(UserViewLayout layoutObj)
        {
            // Open stream.
            var stream = new MemoryStream();
            PivotGridControl.SaveLayoutToStream(stream);
            stream.Position = 0;

            // Save stream to datastore.
            layoutObj.LayoutFile = new FileData(layoutObj.Session);
            layoutObj.LayoutFile.LoadFromStream("PivotGridLayout.xml", stream);
            layoutObj.Session.CommitTransaction();
        }

        protected override void OnActivated()
        {
            base.OnActivated();
        }

        public PivotGridControl PivotGridControl
        {
            get
            {
                var pgEditor = ((ListView)View).Editor as PivotGridListEditor;
                if (pgEditor == null) return null;
                return pgEditor.PivotGridControl;
            }
        }

        protected override PivotGridFieldCollectionBase PivotGridControlFields
        {
            get
            {
                return PivotGridControl.Fields;
            }
        }

        protected override void SyncPivotGridFieldsToControl()
        {
            base.SyncPivotGridFieldsToControl();
            var pgControl = PivotGridControl;
            pgControl.Fields.Clear();
            // add fields to Pivot Grid
            foreach (PivotGridFieldBase baseField in PivotGridFields)
            {
                if (baseField == null) continue;
                var winField = Utils.MapPivotGridFieldToWin(baseField);
                winField.Appearance.Header.Options.UseTextOptions = true;
                winField.Appearance.Header.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
                pgControl.Fields.Add(winField);
            }
        }

        protected override void SavePivotGridLayoutToStream(MemoryStream stream)
        {
            base.SavePivotGridLayoutToStream(stream);
            PivotGridControl.SaveLayoutToStream(stream);
        }

        protected override void SavePivotGridLayout(BusinessObjects.User.UserViewLayout layoutObj)
        {
            var stream = new MemoryStream();
            PivotGridControl.SaveLayoutToStream(stream);
            stream.Position = 0;

            base.SavePivotGridLayout(layoutObj, stream);
        }

        protected override void LoadPivotGridLayout(BusinessObjects.User.UserViewLayout layoutObj)
        {
            base.LoadPivotGridLayout(layoutObj);
            PivotGridControl.Fields.Clear();
            var stream = new MemoryStream(layoutObj.LayoutFile.Content);
            stream.Position = 0;
            PivotGridControl.RestoreLayoutFromStream(stream);
        }

        protected override void OnViewControlsCreated()
        {
            var pgControl = PivotGridControl;

            pgControl.OptionsData.DataFieldUnboundExpressionMode = DataFieldUnboundExpressionMode.UseSummaryValues;

            pgControl.OptionsDataField.ColumnValueLineCount = 2;
            pgControl.Appearance.Cell.Options.UseTextOptions = true;
            pgControl.Appearance.Cell.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            pgControl.Appearance.FieldValue.Options.UseTextOptions = true;
            pgControl.Appearance.FieldValue.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            pgControl.Appearance.FieldHeader.Options.UseTextOptions = true;
            pgControl.Appearance.FieldHeader.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            pgControl.Appearance.DataHeaderArea.Options.UseTextOptions = true;
            pgControl.Appearance.DataHeaderArea.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            pgControl.Appearance.RowHeaderArea.Options.UseTextOptions = true;
            pgControl.Appearance.RowHeaderArea.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;

            pgControl.CustomCellDisplayText += pgControl_CustomCellDisplayText;
            pgControl.CustomCellValue += pgControl_CustomCellValue;
            pgControl.CellDoubleClick += pgControl_CellDoubleClick;
            pgControl.CustomFieldSort += pgControl_CustomFieldSort;
            pgControl.CustomSummary += pgControl_CustomSummary;

            base.OnViewControlsCreated();

            pgControl.RefreshData();
        }

        protected override void CreatePivotGridFields()
        {
            base.CreatePivotGridFields();
            // TODO: can you cast a base type to a derived type?
            // if not, then copy the base object to a new derived object

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

        // Places Snapshot1 in front of Snapshot2
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
            var pgfieldSnapshot = PivotGridControlFields.GetFieldByName(fieldSnapshot.Name);
            if (pgfieldSnapshot == null) return;
            var snapshot = Convert.ToString(e.GetFieldValue((PivotGridField)pgfieldSnapshot));

            if (IsVarianceField(e.DataField)
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
            PivotGridFieldBase pgfieldSnapshot = PivotGridControlFields.GetFieldByName(fieldSnapshot.Name);
            if (IsVarianceField(e.DataField)
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
            if (e.DataField.Caption == fieldNoteCaption)
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

        protected override void SetupView()
        {
            base.SetupView();
            var pgControl = PivotGridControl;
            if (pgControl != null)
            {
                pgControl.RefreshData();
            }
        }

        protected override void RefreshNoteField()
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
