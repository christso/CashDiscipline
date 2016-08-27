using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Cash
{
    public interface ICashFlowTabular
    {
        string LastReturnMessage { get; }
        void ProcessAll();
        void ProcessCurrent(XPObjectSpace objSpace);
        void ProcessHist();
        void ProcessSshot();
    }
}
