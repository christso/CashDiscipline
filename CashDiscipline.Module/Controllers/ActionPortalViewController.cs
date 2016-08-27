using CashDiscipline.Module.AppNavigation;
using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.Setup;
using CashDiscipline.Module.Controllers.Cash;
using DevExpress.Data.Filtering;
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
            portalAction.ShowItemsOnClick = true;

            var runChoice = new ChoiceActionItem();
            runChoice.Caption = "Run";
            portalAction.Items.Add(runChoice);

            var resetChoice = new ChoiceActionItem();
            resetChoice.Caption = "Reset";
            portalAction.Items.Add(resetChoice);
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

            #region repopulate actions

            var softPortal = new ActionPortalLogic();
            var hardPortal = new ActionPortalHardLogic(softPortal);

            var intlObjs = ObjectSpace.GetObjects<ActionPortal>(
                CriteriaOperator.Parse("ActionPortalType = ?", ActionPortalType.Internal));
            ObjectSpace.Delete(intlObjs);

            foreach (var portalItem in hardPortal.ActionPortalList)
            {
                var portalObj = ObjectSpace.CreateObject<ActionPortal>();
                portalObj.ActionPortalType = ActionPortalType.Internal;
                portalObj.ActionName = portalItem.ActionName;
                portalObj.ActionDescription = portalItem.ActionDescription;
            }

            ObjectSpace.CommitChanges();

            #endregion
        }

        private void RunPortalAction(ShowViewParameters svp)
        {
            var softPortal = new CashDiscipline.Module.AppNavigation.ActionPortalLogic();
            var hardPortal = new ActionPortalHardLogic(softPortal);

            var portalObj = View.CurrentObject as ActionPortal;
            if (portalObj == null) return;
            var appArgs = new CashDiscipline.Module.AppNavigation.ActionPortalEventArgs(Application, ObjectSpace, svp);
            switch (portalObj.ActionPortalType)
            {
                case ActionPortalType.Internal:
                    hardPortal.ExecutePortalAction(appArgs, portalObj.ActionName);
                    break;
                case ActionPortalType.ChoiceAction:
                    softPortal.ExecuteChoiceActionByCaptionPath(
                        appArgs,
                        portalObj.ObjectType, portalObj.ControllerType,
                        portalObj.ActionName, portalObj.ActionPath);
                    break;
                case ActionPortalType.SimpleAction:
                    softPortal.ExecuteSimpleAction(
                        appArgs,
                        portalObj.ObjectType,
                        portalObj.ControllerType,
                        portalObj.ActionName);
                    break;
                case ActionPortalType.View:
                    softPortal.OpenNavigationItem(appArgs, portalObj.ObjectType);
                    break;
            }

        }

    }
}
