using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.BusinessObjects.Forex;
using CTMS.Module.Controllers.Forex;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Interfaces
{
    public class CashFlowBuilder : ICashFlow
    {
        public Account Account { get; set; }
        public Currency CounterCcy { get; set; }
        public DateTime TranDate { get; set; }
        public Counterparty Counterparty { get; set; }
        public decimal AccountCcyAmt { get; set; }
        public decimal CounterCcyAmt { get; set; }
        public decimal FunctionalCcyAmt { get; set; }
        public IEnumerable<ForexTrade> ForexTrades { get; set; }

        public CashFlowBuilder()
        {

        }

        public void LinkPrimaryCashFlow(CashFlow cashFlow)
        {
            if (cashFlow == null)
                throw new ArgumentException("CashFlow");

            SetCashFlowProperties(cashFlow);

            foreach (var ft in ForexTrades)
            {
                ft.PrimaryCashFlow = cashFlow;
            }
        }

        public void LinkCounterCashFlow(CashFlow cashFlow)
        {
            if (cashFlow == null)
                throw new ArgumentException("CashFlow");

            SetCashFlowProperties(cashFlow);

            foreach (var ft in ForexTrades)
            {
                ft.CounterCashFlow = cashFlow;
            }
        }

        private void SetCashFlowProperties(CashFlow cashFlow)
        {
            cashFlow.Account = Account;
            cashFlow.CounterCcy = CounterCcy;
            cashFlow.TranDate = TranDate;
            cashFlow.AccountCcyAmt = AccountCcyAmt;
            cashFlow.FunctionalCcyAmt = FunctionalCcyAmt;
        }
    }
}
