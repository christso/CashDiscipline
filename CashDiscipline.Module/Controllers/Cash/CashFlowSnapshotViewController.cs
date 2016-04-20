using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.ControllerHelpers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class CashFlowSnapshotViewController : ViewController
    {
        public CashFlowSnapshotViewController()
        {
            // delete by setting GC Record
            // you will need to use ObjectSpace.ExecuteNonQuery.
        }

        public void DeleteSnapshot(BaseObject snapshot)
        {
            var deleter = new BatchDeleter(ObjectSpace);
            deleter.Delete(snapshot, CashFlow.Fields.Snapshot.PropertyName);
        }

        public void DeleteSnapshots(IList<CashFlowSnapshot> snapshots)
        {
            var deleter = new BatchDeleter(ObjectSpace);
            
        }
    }
}
