using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using D2NXAF.ExpressApp.Concurrency;
using D2NXAF.ExpressApp.SystemModule;

namespace CTMS.Module.Test
{
    public class RequestTestController : ViewController
    {
        public RequestTestController()
        {
            var myAction = new SimpleAction(this, "MyAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            myAction.Execute += myAction_Execute;
        }

        void myAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var dialog = new PopupDialogDetailViewManager(Application);
            dialog.ShowNonPersistentView(typeof(TestParam));
        }

        private void dialogAccepting(object sender, ShowViewParameters e)
        {
            var request = new D2NXAF.ExpressApp.Concurrency.RequestManager(Application);
            request.SubmitRequest("Job 1", Job1);
        }

        public void Job1()
        {
            Thread.Sleep(6000);
        }
    }
}
