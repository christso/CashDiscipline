using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Win
{
    public sealed partial class CTMSWindowsFormsModule
    {
        // Extends the Application Model elements for View and Navigation Items to be able to specify custom controls via the Model Editor.
        // Refer to the http://documentation.devexpress.com/#Xaf/CustomDocument3169 help article for more information.
        public override void ExtendModelInterfaces(DevExpress.ExpressApp.Model.ModelInterfaceExtenders extenders)
        {
            base.ExtendModelInterfaces(extenders);
        }
    }

}
