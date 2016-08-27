using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Controllers;
using CashDiscipline.Module.Controllers.Cash;
using CashDiscipline.Module.Controllers.Forex;
using CashDiscipline.Module.Logic.Cash;
using CashDiscipline.Module.Logic.Forex;
using CashDiscipline.Module.ParamObjects.Cash;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.AppNavigation
{
    public delegate void ExecutablePortalAction(ActionPortalEventArgs e);

    public class ActionPortalLogic
    {
        public void ExecuteChoiceActionByCaptionPath(ActionPortalEventArgs appArgs,
            Type objType, Type controllerType, string actionName, string captionPath)
        {
            // Open navigation item
            var nav = new NavigationHelper();
            var controller = Activator.CreateInstance(controllerType) as ViewController;
            appArgs.ShowViewParameters.Controllers.Add(controller);
            nav.OpenNavigationItem(appArgs.Application, appArgs.ObjectSpace,
                appArgs.ShowViewParameters, objType);

            // Find action
            var action = (SingleChoiceAction)controller.Actions[actionName];
            if (action == null)
            {
                action = (SingleChoiceAction)controller.Actions
                    .Where(c => c.Caption == actionName)
                    .FirstOrDefault();
            }
            if (action == null)
                throw new UserFriendlyException(string.Format(
                    "Action '{0}' does not exist", actionName));

            // Find choice item
            var choice = action.FindItemByCaptionPath(captionPath);
            if (choice == null)
            {
                throw new UserFriendlyException(string.Format(
                    "Choice path '{0}' does not exist.", captionPath));
            }
            nav.DoExecuteOnActivated(controller, action, choice);
        }

        //controllerTypeName = "CashDiscipline.Module.Controllers.Cash.CashFlowViewController"
        public void ExecuteChoiceActionByCaptionPath(ActionPortalEventArgs appArgs,
            string objTypeName, string controllerTypeName, string actionId, string captionPath)
        {
            Assembly asm = typeof(CashDiscipline.Module.BusinessObjects.SetOfBooks).Assembly;

            Type objType = asm.GetType(objTypeName);
            if (objType == null)
            {
                objType = XpoTypesInfoHelper.GetTypesInfo().PersistentTypes.FirstOrDefault(
                    x => x.Name == objTypeName).Type;
            }

            Type controllerType = asm.GetType(controllerTypeName);
            ExecuteChoiceActionByCaptionPath(appArgs, objType, controllerType,
                actionId, captionPath);
        }
        public void ExecuteSimpleAction(ActionPortalEventArgs appArgs,
            Type objType, Type controllerType, string actionName)
        {
            // Open navigation item
            var nav = new NavigationHelper();
            var controller = Activator.CreateInstance(controllerType) as ViewController;
            appArgs.ShowViewParameters.Controllers.Add(controller);
            nav.OpenNavigationItem(appArgs.Application, appArgs.ObjectSpace,
                appArgs.ShowViewParameters, objType);

            // Find action
            var action = (SimpleAction)controller.Actions[actionName];
            if (action == null)
            {
                action = (SimpleAction)controller.Actions
                    .Where(c => c.Caption == actionName)
                    .FirstOrDefault();
            }
            if (action == null)
                throw new UserFriendlyException(string.Format(
                    "Action '{0}' does not exist", actionName));
            nav.DoExecuteOnActivated(controller, action);
        }

        public void ExecuteSimpleAction(ActionPortalEventArgs appArgs,
            string objTypeName, string controllerTypeName, string actionName)
        {
            Assembly asm = typeof(CashDiscipline.Module.BusinessObjects.SetOfBooks).Assembly;

            Type objType = asm.GetType(objTypeName);
            Type controllerType = asm.GetType(controllerTypeName);
            ExecuteSimpleAction(appArgs, objType, controllerType,
                actionName);
        }

        public void OpenNavigationItem(ActionPortalEventArgs appArgs, Type objType)
        {
            var nav = new NavigationHelper();
            nav.OpenNavigationItem(appArgs.Application, appArgs.ObjectSpace, appArgs.ShowViewParameters, objType);
        }

        public void OpenNavigationItem(ActionPortalEventArgs appArgs, string objTypeName)
        {
            // Get type from full name
            Assembly asm = typeof(CashDiscipline.Module.BusinessObjects.SetOfBooks).Assembly;
            Type objType = asm.GetType(objTypeName);

            // otherwise, get type from short name
            if (objType == null)
            {
                objType = XpoTypesInfoHelper.GetTypesInfo().PersistentTypes.FirstOrDefault(
                    x => x.Name == objTypeName).Type;
            }

            var nav = new NavigationHelper();
            nav.OpenNavigationItem(appArgs.Application, appArgs.ObjectSpace, appArgs.ShowViewParameters, objType);
        }
    }
}
