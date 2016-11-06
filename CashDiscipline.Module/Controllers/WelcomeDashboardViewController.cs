using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CashDiscipline.Module.Controllers
{
    public class WelcomeDashboardViewController : ViewController<DashboardView>
    {
        public WelcomeDashboardViewController()
        {
            TargetViewId = "Welcome_Dashboard";
        }

        protected override void OnActivated()
        {
            base.OnActivated();
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();

            //var item = (StaticText)View.FindItem("Welcome");
        }
    }
}
