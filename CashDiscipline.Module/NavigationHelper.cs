using CashDiscipline.Module.Controllers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module
{
    public class NavigationHelper
    {
        public void OpenNavigationItem(XafApplication app, IObjectSpace os,
            ShowViewParameters svp, Type objType)
        {

            var obj = os.FindObject(objType, null);
            var objKey = os.GetKeyValueAsString(obj);
            var viewId = app.GetListViewId(objType);
            var shortcut = new ViewShortcut(objType, objKey, viewId);

            svp.CreatedView = app.ProcessShortcut(shortcut);
            svp.TargetWindow = TargetWindow.NewWindow;
        }

        public void DoExecuteOnActivated(ViewController controller, SimpleAction action)
        {
            EventHandler controller_activated = (sender, e) =>
            {
                action.DoExecute();
            };
            controller.Activated += controller_activated;
            controller.Deactivated += (sender, e) =>
            {
                controller.Activated -= controller_activated;
            };
        }

        public void DoExecuteOnActivated(ViewController controller, SingleChoiceAction action,
            ChoiceActionItem choice)
        {
            EventHandler executeHandler = (sender, e) =>
            {
                action.DoExecute(choice);
            };
            controller.Activated += executeHandler;
            controller.Deactivated += (sender, e) =>
            {
                controller.Activated -= executeHandler;
            };
        }

    }

}
