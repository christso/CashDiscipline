using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Logic;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.BatchDelete;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class CashFlowSnapshotViewController : ViewController
    {
        public CashFlowSnapshotViewController()
        {

            var cfsAction = new SingleChoiceAction(this, "CashFlowSnapshotAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            cfsAction.Caption = "Actions";
            cfsAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            cfsAction.Execute += cfsAction_Execute;
            cfsAction.ShowItemsOnClick = true;

            //var deleteAction = new ChoiceActionItem();
            //deleteAction.Caption = "Delete";
            //cfsAction.Items.Add(deleteAction);
            
        }

        private void cfsAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ObjectSpace.CustomDeleteObjects += ObjectSpace_CustomDeleteObjects;
        }

        protected override void OnDeactivated()
        {
            ObjectSpace.CustomDeleteObjects -= ObjectSpace_CustomDeleteObjects;
            base.OnDeactivated();
        }

        private void ObjectSpace_CustomDeleteObjects(object sender, CustomDeleteObjectsEventArgs e)
        {
            DeleteSnapshots(e.Objects);
            e.Handled = true;
        }

        public void DeleteSnapshot(BaseObject snapshot)
        {
            var deleter = new BatchDeleter(ObjectSpace);
            deleter.Delete(snapshot);
        }

        public void DeleteSnapshots(IEnumerable snapshots)
        {
            var deleter = new BatchDeleter(ObjectSpace);
            deleter.Delete(snapshots);
        }
    }
}
