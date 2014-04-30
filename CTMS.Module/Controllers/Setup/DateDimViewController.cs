using CTMS.Module.BusinessObjects.Setup;
using CTMS.Module.ParamObjects.Setup;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Controllers.Setup
{
    public class DateDimViewController : ViewControllerEx
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
            ShowNonPersistentPopupDialogDetailView(Application, typeof(CreateDateDimParam));
        }
        private SimpleAction createDateAction;
    }
}
