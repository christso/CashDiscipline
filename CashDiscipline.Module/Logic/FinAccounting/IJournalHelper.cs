using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.FinAccounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.FinAccounting
{
    public interface IJournalHelper<T>
    {
        void Process(IEnumerable<T> sourceItems, IEnumerable<FinAccount> accountMaps, IEnumerable<FinActivity> activityMaps);
        IList<T> GetSourceObjects(IEnumerable<Activity> activitiesToMap,
            IEnumerable<Account> accountsToMap);
    }
}
