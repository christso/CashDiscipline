using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.PivotGrid.Web;
using DevExpress.Web.ASPxPivotGrid;
using DevExpress.ExpressApp.Web.Editors.ASPx;
using DevExpress.Web.ASPxGridView;
using DevExpress.XtraPivotGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTMS.Module.Controllers.Cash;
using D2NXAF.PivotGrid;
using System.IO;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Utils;
using DevExpress.ExpressApp.Actions;

namespace CTMS.Module.Web.Controllers.Cash
{
    public class CashFlowPivotGridControllerWeb : CashFlowPivotGridController
    {
        public CashFlowPivotGridControllerWeb()
        {
        }


        public ASPxPivotGrid PivotGridControl
        {
            get
            {
                var pgEditor = (ASPxPivotGridListEditor)((ListView)View).Editor;
                if (pgEditor == null) return null;
                var pgControl = (ASPxPivotGrid)pgEditor.PivotGridControl;
                return (ASPxPivotGrid)pgControl;
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
                var webField = Utils.MapPivotGridFieldToWeb(baseField);
                webField.HeaderStyle.Wrap = DefaultBoolean.True;
                pgControl.Fields.Add(webField);
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
            PivotGridControl.LoadLayoutFromStream(stream);

        }

        protected override void ResetPivotGridLayout()
        {
            base.ResetPivotGridLayout();
            PivotGridControl.Fields.Clear();
            OnViewControlsCreated();
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            var pgControl = PivotGridControl;

            pgControl.OptionsData.DataFieldUnboundExpressionMode = DataFieldUnboundExpressionMode.UseSummaryValues;

            pgControl.OptionsDataField.ColumnValueLineCount = 2;

            pgControl.Styles.CellStyle.Wrap = DefaultBoolean.True;
            pgControl.Styles.FieldValueStyle.Wrap = DefaultBoolean.True;
            pgControl.Styles.HeaderStyle.Wrap = DefaultBoolean.True;
            pgControl.Styles.RowAreaStyle.Wrap = DefaultBoolean.True;
            pgControl.Styles.ColumnAreaStyle.Wrap = DefaultBoolean.True;

            //pgControl.CustomCellDisplayText += pgControl_CustomCellDisplayText;
            //pgControl.CustomCellValue += pgControl_CustomCellValue;
            //pgControl.CustomFieldSort += pgControl_CustomFieldSort;
            //pgControl.CustomSummary += pgControl_CustomSummary;

            //pgControl.CellDoubleClick += pgControl_CellDoubleClick;
        }

        // TODO: create platform-agnostic version
        private void pgControl_CustomSummary(object sender, PivotGridCustomSummaryEventArgs e)
        {
            PivotGridFieldBase pgfieldSnapshot = PivotGridControlFields.GetFieldByName(((PivotGridFieldBase)fieldSnapshot).Name);
            
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

        private void pgControl_CustomCellDisplayText(object sender, PivotCellDisplayTextEventArgs e)
        {

        }

        private void pgControl_CustomFieldSort(object sender, PivotGridCustomFieldSortEventArgs e)
        {
            
        }

        // TODO: create platform-agnostic version
        private void pgControl_CustomCellValue(object sender, PivotCellValueEventArgs e)
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

        protected override void SetupView()
        {
            base.SetupView();
            var pgControl = PivotGridControl;
            if (pgControl != null)
            {
                //pgControl.ReloadData();
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
