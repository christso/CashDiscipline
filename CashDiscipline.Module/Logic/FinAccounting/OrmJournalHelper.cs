using CashDiscipline.Module.BusinessObjects.FinAccounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.FinAccounting
{
    public class OrmJournalHelper
    {
        public static void Process<T>(IJournalHelper<T> helper, IEnumerable<FinAccount> accountMaps, IEnumerable<FinActivity> activityMaps)
        {
            var accountsToMap = accountMaps.Select(k => k.Account);
            var activitiesToMap = activityMaps.GroupBy(m => m.FromActivity).Select(k => k.Key);
            var sourceObjects = helper.GetSourceObjects(activitiesToMap, accountsToMap);
            helper.Process(sourceObjects, accountMaps, activityMaps);
        }

    }
}
