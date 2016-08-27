using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.AppNavigation
{
    public class ActionPortalEventArgs
    {
        public ActionPortalEventArgs(XafApplication application,
            IObjectSpace objectSpace, ShowViewParameters svp)
        {
            this.application = application;
            this.os = objectSpace;
            this.svp = svp;
        }

        private XafApplication application;
        private IObjectSpace os;
        private ShowViewParameters svp;

        public XafApplication Application
        {
            get { return application; }
            set { application = value; }
        }

        public IObjectSpace ObjectSpace
        {
            get { return os; }
            set { os = value; }
        }

        public ShowViewParameters ShowViewParameters
        {
            get { return svp; }
            set { svp = value; }
        }
    }
}
