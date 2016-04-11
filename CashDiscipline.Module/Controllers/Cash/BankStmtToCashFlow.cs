using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class BankStmtToCashFlow
    {
        private readonly IObjectSpace objectSpace;
        private readonly DateTime fromDate;
        private readonly DateTime toDate;
        private readonly ICashFlowDeleter deleter;

        public BankStmtToCashFlow(IObjectSpace objectSpace,
            DateTime fromDate, DateTime toDate,
            ICashFlowDeleter deleter)
        {
            if (objectSpace == null)
                throw new ArgumentException("objectSpace");
            if (deleter == null)
                deleter = new CashFlowDeleter(((XPObjectSpace)objectSpace).Session,
                    fromDate, toDate);
            this.objectSpace = objectSpace;
            this.fromDate = fromDate;
            this.toDate = toDate;
            this.deleter = deleter;
        }

        public BankStmtToCashFlow(IObjectSpace objectSpace,
            DateTime fromDate, DateTime toDate) 
            : this(objectSpace, fromDate, toDate, null)
        {

        }

        public void Process()
        {
            DeleteCashFlows();
            IEnumerable<CashFlow> cashFlowQuery = GroupCashFlows();
            objectSpace.CommitChanges();
        }

        public IEnumerable<CashFlow> GroupCashFlows()
        {
            var criteria = CriteriaOperator.Parse(
                "TranDate Between (?, ?)",
                fromDate, toDate);

            var bsQuery = objectSpace.GetObjects<BankStmt>(criteria);

            return bsQuery.GroupBy(bs => new
            {
                bs.TranDate,
                bs.Account,
                bs.Activity,
                bs.Counterparty,
                bs.SummaryDescription,
                bs.CounterCcy,
                bs.CounterCcyAmt,
                bs.FunctionalCcyAmt,
                bs.ForexSettleType
            })
            .Select(g =>
            {
                var cf = new CashFlow(((XPObjectSpace)objectSpace).Session)
                {
                    TranDate = g.Key.TranDate,
                    Account = g.Key.Account,
                    Activity = g.Key.Activity,
                    Counterparty = g.Key.Counterparty,
                    Description = g.Key.SummaryDescription,
                    CounterCcy = g.Key.CounterCcy,
                    CounterCcyAmt = g.Sum(s => s.CounterCcyAmt),
                    AccountCcyAmt = g.Sum(s => s.TranAmount),
                    FunctionalCcyAmt = g.Sum(s => s.FunctionalCcyAmt),
                    ForexSettleType = g.Key.ForexSettleType
                };
                cf.BankStmts.AddRange(g.AsEnumerable<BankStmt>());
                return cf;
            }).ToArray();
        }

        public void DeleteCashFlows()
        {
            this.deleter.Delete();
        }
    }
}
