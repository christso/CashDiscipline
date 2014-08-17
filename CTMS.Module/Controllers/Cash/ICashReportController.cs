using CTMS.Module.ParamObjects.Cash;
using DevExpress.ExpressApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Controllers.Cash
{
    public interface ICashReportController
    {
        void SetupView(CashReportParam paramObj);
        BoolList Active { get; }
    }
}
