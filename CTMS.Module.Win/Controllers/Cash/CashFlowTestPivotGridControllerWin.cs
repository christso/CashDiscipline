using CTMS.Module.BusinessObjects.User;
using CTMS.Module.Controllers.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.PivotGrid.Win;
using DevExpress.Persistent.BaseImpl;
using DevExpress.XtraPivotGrid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CTMS.Module.Win.Controllers.Cash
{
    public class CashFlowTestPivotGridControllerWin : CashFlowTestPivotGridController
    {
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            var pgEditor = ((ListView)View).Editor as PivotGridListEditor;
            if (pgEditor != null)
                _PivotGridControl = pgEditor.PivotGridControl;

            var layoutObj = ObjectSpace.FindObject<UserViewLayout>(CriteriaOperator.Parse("LayoutName = ?", "Debug"));
            if (layoutObj == null)
            {
                _PivotGridControl.Fields.Clear();

                var fieldActivity = new PivotGridField("Activity.Name", PivotArea.FilterArea);
                fieldActivity.Name = "fieldActivity";
                fieldActivity.Caption = "My Activity";
                fieldActivity.Area = PivotArea.RowArea;
                _PivotGridControl.Fields.Add(fieldActivity);

                var fieldAmount = new PivotGridField("FunctionalCcyAmt", PivotArea.FilterArea);
                fieldAmount.Name = "fieldFunctionalCcyAmt";
                fieldAmount.Caption = "My FunctionalCcyAmt";
                fieldAmount.Area = PivotArea.DataArea;
                _PivotGridControl.Fields.Add(fieldAmount);
            }
            else
            {
                LoadLayout("Debug");
            }
        }

        protected override bool LoadLayout(string name)
        {
            base.LoadLayout(name);
            var layoutObj = ObjectSpace.FindObject<UserViewLayout>(CriteriaOperator.Parse("LayoutName = ?", name));
            if (layoutObj == null) return false;

            var stream = new MemoryStream(layoutObj.LayoutFile.Content);
            stream.Position = 0;
            PivotGridControl.RestoreLayoutFromStream(stream);
            return true;
        }

        protected override void SaveLayout(string name)
        {
            base.SaveLayout(name);

            var layoutObj = ObjectSpace.FindObject<UserViewLayout>(CriteriaOperator.Parse("LayoutName = ?", name));
            if (layoutObj == null)
            {
                layoutObj = ObjectSpace.CreateObject<UserViewLayout>();
                layoutObj.LayoutName = name;
            }

            // Open stream.
            var stream = new MemoryStream();
            PivotGridControl.SaveLayoutToStream(stream);
            stream.Position = 0;

            // Save stream to datastore.
            layoutObj.LayoutFile = new FileData(layoutObj.Session);
            layoutObj.LayoutFile.LoadFromStream("PivotGridLayout.xml", stream);
            layoutObj.Session.CommitTransaction();
        }

        private PivotGridControl _PivotGridControl;
        public PivotGridControl PivotGridControl
        {
            get
            {
                return _PivotGridControl;
            }
        }


    }
}
