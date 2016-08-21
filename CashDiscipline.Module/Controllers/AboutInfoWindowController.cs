using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers
{
    public class AboutInfoWindowController : WindowController
    {
        protected override void OnActivated()
        {
            base.OnActivated();
            AboutInfo.Instance.Version = CashDiscipline.Module.AssemblyInfo.Version;
        }
    }
}
