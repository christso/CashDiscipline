using CTMS.Module.ParamObjects.Cash;
using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Controllers.Cash
{
    public class CashReportParamDetailViewController : ViewController<DetailView>
    {
        public CashReportParamDetailViewController()
        {
            TargetObjectType = typeof(CashReportParam);
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            View.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
        }
    }
}
