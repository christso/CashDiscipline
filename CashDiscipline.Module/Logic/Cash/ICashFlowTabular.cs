using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Cash
{
    public interface ICashFlowTabular
    {
        void ProcessAll();
        void ProcessCurrent();
        void ProcessHist();
        void ProcessSshot();
    }
}
