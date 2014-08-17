using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using DevExpress.ExpressApp.Web.Templates;
using DevExpress.ExpressApp.Templates;

namespace CTMS.Web
{
    public partial class LogonTemplateContent1 : TemplateContent
    {
        public override IActionContainer DefaultContainer
        {
            get { return null; }
        }
        public override void SetStatus(ICollection<string> statusMessages)
        {
        }
        public override object ViewSiteControl
        {
            get
            {
                return viewSiteControl;
            }
        }

    }
}
