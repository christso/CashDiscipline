using CashDiscipline.Module.BusinessObjects.Setup;
using CashDiscipline.Module.ParamObjects.Setup;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers.Setup
{
    public class DateDimViewController : ViewController
    {
        public DateDimViewController()
        {
            TargetObjectType = typeof(DateDim);
            createDateAction = new SimpleAction(this, "CreateDateAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            createDateAction.Caption = "Create Dates";
            createDateAction.Execute += _CreateDateAction_Execute;
        }

        void _CreateDateAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var dialog = new Xafology.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application); 
            dialog.ShowNonPersistentView(typeof(CreateDateDimParam));
        }
        private SimpleAction createDateAction;
    }
}
