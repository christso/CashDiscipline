using CTMS.Module.BusinessObjects.Forex;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Controllers.Forex
{
    public class ForexTradeViewController : ViewController
    {
        public ForexTradeViewController()
        {
            TargetObjectType = typeof(ForexTrade);
            
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            View.ControlsCreated += View_ControlsCreated;
        }

        void View_ControlsCreated(object sender, EventArgs e)
        {
            var objs = View.SelectedObjects;
            foreach (ForexTrade obj in objs)
            {
                obj.SyncEnabled = true;
            }
        }

        public static void LinkCashFlowsToForexTrades(IObjectSpace objSpace)
        {
            
        }
    }
}
