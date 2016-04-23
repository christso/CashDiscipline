using CashDiscipline.Module.BusinessObjects.Cash;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Cash
{
    public interface IFixCashFlows
    {
        void ProcessCashFlows();
        // delete existing fixes
        void Reset();
    }
}
