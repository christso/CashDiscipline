using CTMS.Module.ParamObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Controllers
{
    public class MoveToIndexParamViewController : ViewController
    {
        public MoveToIndexParamViewController()
        {
            TargetObjectType = typeof(MoveToIndexParam);
            TargetViewType = ViewType.DetailView;
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
        }
    }
}
