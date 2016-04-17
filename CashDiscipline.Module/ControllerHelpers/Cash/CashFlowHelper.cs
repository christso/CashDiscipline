using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.ControllerHelpers.Cash
{
    public class CashFlowHelper
    {
        public static CashFlowSnapshot GetCurrentSnapshot(Session session)
        {
            var setOfBooks = SetOfBooks.GetInstance(session);
            return session.GetObjectByKey<CashFlowSnapshot>(setOfBooks.CurrentCashFlowSnapshot.Oid);
        }

        public static DateTime GetMaxActualTranDate(Session session)
        {
            var currentSnapshot = SetOfBooks.CachedInstance.CurrentCashFlowSnapshot;

            DateTime? res = (DateTime?)session.Evaluate<CashFlow>(CriteriaOperator.Parse("Max(TranDate)"),
                    CriteriaOperator.Parse("Status = ? And Snapshot = ?", 
                    CashFlowStatus.Actual, currentSnapshot));
            if (res == null)
                return default(DateTime);
            return (DateTime)res;
        }
    }
}
