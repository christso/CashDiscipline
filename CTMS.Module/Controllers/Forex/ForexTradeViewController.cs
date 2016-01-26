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
        }


        public static void LinkCashFlowsToForexTrades(IObjectSpace objSpace)
        {
            
        }
    }
}
