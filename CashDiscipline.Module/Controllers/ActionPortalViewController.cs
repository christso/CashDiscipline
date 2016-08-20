using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.Setup;
using CashDiscipline.Module.Controllers.Cash;
using CashDiscipline.Module.Logic.Setup;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Xpo.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* This controller is supposed to show all actions and allow them to be executed from a central view.
 * It currently navigates to a hard coded view, and we still need to work out how to execute an action in another view.
*/
namespace CashDiscipline.Module.Controllers
{
    public class ActionPortalViewController : ViewController
    {

        public ActionPortalViewController()
        {
            TargetObjectType = typeof(ActionPortal);

            var portalAction = new SingleChoiceAction(this, "ProgramPortalAction", PredefinedCategory.ObjectsCreation);
            portalAction.Caption = "Actions";
            portalAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            portalAction.Execute += portalAction_Execute;
            var resetChoice = new ChoiceActionItem();
            resetChoice.Caption = "Reset";
            portalAction.Items.Add(resetChoice);

            var runChoice = new ChoiceActionItem();
            runChoice.Caption = "Run";
            portalAction.Items.Add(runChoice);

        }

        private void portalAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            switch(e.SelectedChoiceActionItem.Caption)
            {
                case "Reset":
                    ResetPortal();
                    break;
                case "Run":
                    RunPortalAction(e.ShowViewParameters);
                    break;
            }
        }

        private void ResetPortal()
        {

            var objSpace = (XPObjectSpace)ObjectSpace;
            var classInfo = objSpace.Session.GetClassInfo<ActionPortal>();

            #region delete all actions
            var deleter = new Xafology.ExpressApp.BatchDelete.BatchDeleter(ObjectSpace);
            deleter.Delete(classInfo, null);
            #endregion

            #region repopulate actions

            foreach (var portalItem in ActionPortalLogic.ActionPortalList)
            {
                var portalObj = ObjectSpace.CreateObject<ActionPortal>();
                portalObj.ActionName = portalItem.ActionName;
            }

            ObjectSpace.CommitChanges();

            #endregion
        }

        private void RunPortalAction(ShowViewParameters svp)
        {
            var portalObj = View.CurrentObject as ActionPortal;
            if (portalObj == null) return;
            ActionPortalLogic.Execute(Application, svp, portalObj.ActionName);
        }

    }
}
