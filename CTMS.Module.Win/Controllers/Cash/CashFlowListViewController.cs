using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Win.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Win.Controllers.Cash
{
    public class CashFlowListViewController : ViewController
    {
        public CashFlowListViewController()
        {
            TargetObjectType = typeof(CashFlow);
            TargetViewType = ViewType.ListView;
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            View.ControlsCreated += View_ControlsCreated;
        }

        void View_ControlsCreated(object sender, EventArgs e)
        {
            if (((ListView)View).Editor is GridListEditor)
            {
                ((GridListEditor)((ListView)View).Editor).GridView.OptionsView.ColumnAutoWidth = false;
            }
        }
    }
}
