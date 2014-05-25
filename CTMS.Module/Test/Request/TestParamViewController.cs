using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTMS.Module.Test
{
    public class TestParamViewController : ViewController<DetailView>
    {
        public TestParamViewController()
        {
            TargetObjectType = typeof(TestParam);
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            View.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            //var controller = Frame.GetController<DialogController>();
            //controller.Accepting += controller_Accepting;
        }

        void controller_Accepting(object sender, DialogControllerAcceptingEventArgs e)
        {
            //View.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.View;

            //var request = new D2NXAF.ExpressApp.Concurrency.RequestManager(Application);
            //request.SubmitRequest("Test", Job1);
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            var controller = Frame.GetController<DialogController>();
            controller.Accepting -= controller_Accepting;
        }

        private void Job1()
        {
            Thread.Sleep(5000);
        }
    }
}
