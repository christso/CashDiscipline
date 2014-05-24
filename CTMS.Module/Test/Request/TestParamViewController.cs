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
            var controller = Frame.GetController<DialogController>();
            controller.Accepting += controller_Accepting;
        }

        void controller_Accepting(object sender, DialogControllerAcceptingEventArgs e)
        {
            var request = new D2NXAF.ExpressApp.Concurrency.RequestManager(Application);
            request.SubmitRequest("Test", Job1);
        }

        private void Job1()
        {
            Thread.Sleep(5000);
        }
    }
}
