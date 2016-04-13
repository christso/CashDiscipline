using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.BusinessObjects.Cash;
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
            return session.GetObjectByKey<CashFlowSnapshot>(SetOfBooks.CachedInstance.CurrentCashFlowSnapshot.Oid);
        }

    }
}
