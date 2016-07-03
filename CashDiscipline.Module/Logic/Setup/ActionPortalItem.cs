using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Setup
{
    public class ActionPortalItem
    {
        public string ActionName { get; set;  }

        public ExecutablePortalAction ExecutableAction { get; set; }
    }
}
