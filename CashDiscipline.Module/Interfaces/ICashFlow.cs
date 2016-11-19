using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Interfaces
{
    using BusinessObjects;
    using BusinessObjects.Cash;

    public interface ICashFlow
    {
        Account Account { get; set; }
        Currency CounterCcy { get; set; }
        DateTime TranDate { get; set; }
        Counterparty Counterparty { get; set; }
        decimal AccountCcyAmt { get; set; }
        decimal CounterCcyAmt { get; set; }
        decimal FunctionalCcyAmt { get; set; }
    }
}
