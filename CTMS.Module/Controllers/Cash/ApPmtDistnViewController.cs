using CTMS.Module.BusinessObjects.Cash.AccountsPayable;
using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Controllers.Cash
{
    public class ApPmtDistnViewController : ViewControllerEx
    {
        public ApPmtDistnViewController()
        {
            TargetObjectType = typeof(ApPmtDistn);
        }
        protected override void OnActivated()
        {
            base.OnActivated();

        }
    }
}
