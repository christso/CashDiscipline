using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers
{
    public class ImportViewController : Xafology.ExpressApp.Xpo.Import.Controllers.ImportViewController
    {
        public ImportViewController()
        {

        }

        protected override void OnActivated()
        {
            base.OnActivated();
            var action = this.Actions["ImportAction"];
            action.Active.SetItemValue("Disabled", true);
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
        }
    }
}
