using CTMS.Module.BusinessObjects.User;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTMS.Module.Controllers.Cash
{
    public class CashFlowTestPivotGridController : ViewController
    {
        public CashFlowTestPivotGridController()
        {
            TargetViewId = "CashFlow_PivotGridViewTest";

            var layoutAction = new SingleChoiceAction(this, "CashFlowPivotLayoutAction", DevExpress.Persistent.Base.PredefinedCategory.View);
            layoutAction.Caption = "Layout";
            layoutAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;

            var loadChoice = new ChoiceActionItem();
            loadChoice.Caption = "Load";
            layoutAction.Items.Add(loadChoice);

            var saveChoice = new ChoiceActionItem();
            saveChoice.Caption = "Save";
            layoutAction.Items.Add(saveChoice);
            layoutAction.Execute += loadLayoutTestAction_Execute;

            var load2Choice = new ChoiceActionItem();
            load2Choice.Caption = "Load 2";
            layoutAction.Items.Add(load2Choice);

            var save2Choice = new ChoiceActionItem();
            save2Choice.Caption = "Save 2";
            layoutAction.Items.Add(save2Choice);
        }

        private void loadLayoutTestAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            string name = "Debug";
            var layoutObj = ObjectSpace.FindObject<UserViewLayout>(CriteriaOperator.Parse("LayoutName = ?", name));

            switch (e.SelectedChoiceActionItem.Caption)
            {
                case "Load":
                    LoadLayout(name);
                    break;
                case "Save":
                    SaveLayout(name);
                    break;
                case "Load 2":
                    break;
                case "Save 2":
                    break;
            }
        }

        protected virtual bool LoadLayout(string name)
        {
            return false;
        }

        protected virtual void SaveLayout(string name)
        {
        }
    }
}
