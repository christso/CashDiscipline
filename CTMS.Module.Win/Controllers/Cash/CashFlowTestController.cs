using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Win.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

/* UNCOMMENT THIS TO ADD TEST CODE

namespace CTMS.Module.Win.Controllers.Cash
{
    public class CashFlowTestController : ViewController
    {
        public CashFlowTestController()
        {
            TargetObjectType = typeof(CashFlow);
            TargetViewType = ViewType.ListView;

            var testAction = new SingleChoiceAction(this, "CashFlowTestAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            testAction.Caption = "Tests";
            testAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            testAction.Execute += TestAction_Execute; ;

            var testActionItem = new ChoiceActionItem();
            testActionItem.Caption = "Test 1";
            testAction.Items.Add(testActionItem);
        }

        private void TestAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {

        }
    }
}

*/