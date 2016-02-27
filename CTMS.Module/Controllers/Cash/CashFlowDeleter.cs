using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.Xpo;

namespace CTMS.Module.Controllers.Cash
{
    public class CashFlowDeleter : ICashFlowDeleter
    {
        private readonly Session session;
        private readonly DateTime fromDate;
        private readonly DateTime toDate;

        public CashFlowDeleter(Session session, DateTime fromDate, DateTime toDate)
        {
            this.session = session;
            this.fromDate = fromDate;
            this.toDate = toDate;
        }

        public void Delete()
        {
            var cashFlows = (new XPQuery<CashFlow>(session))
                .Where(c => c.TranDate >= fromDate
                    && c.TranDate <= toDate
                    && c.Snapshot.Oid == SetOfBooks.CachedInstance.CurrentCashFlowSnapshot.Oid
                );
            foreach (var cashFlow in cashFlows)
            {
                cashFlow.Delete();
            }
        }
    }
}
