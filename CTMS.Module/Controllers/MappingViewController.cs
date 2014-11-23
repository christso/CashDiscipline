using CTMS.Module.ParamObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CTMS.Module.BusinessObjects;

namespace CTMS.Module.Controllers
{
    public class MappingViewController : ViewController
    {
        public MappingViewController()
        {
            TargetObjectType = typeof(IMappingObject);

            moveAction = new SingleChoiceAction(this, "MappingMoveAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            moveAction.Caption = "Move";
            moveAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            moveAction.Execute += moveAction_Execute;

            var moveUpActionItem = new ChoiceActionItem();
            moveUpActionItem.Caption = "Up";
            moveAction.Items.Add(moveUpActionItem);

            var moveDownActionItem = new ChoiceActionItem();
            moveDownActionItem.Caption = "Down";
            moveAction.Items.Add(moveDownActionItem);

            var moveTopActionItem = new ChoiceActionItem();
            moveTopActionItem.Caption = "Top";
            moveAction.Items.Add(moveTopActionItem);

            var moveBottomActionItem = new ChoiceActionItem();
            moveBottomActionItem.Caption = "Bottom";
            moveAction.Items.Add(moveBottomActionItem);

            var moveCustomActionItem = new ChoiceActionItem();
            moveCustomActionItem.Caption = "Custom";
            moveAction.Items.Add(moveCustomActionItem);

            IndexName = "Index";
        }
        public string TargetTableName { get; set; }
        public string IndexName { get; set; }

        protected override void OnActivated()
        {
            base.OnActivated();
            if (TargetTableName == null)
                TargetTableName = View.ObjectTypeInfo.Name;
        }

        void moveAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var selectedMapping = (BaseObject)View.CurrentObject;
            if ((int)selectedMapping.GetMemberValue("Index") == 0) return;

            switch (e.SelectedChoiceActionItem.Caption)
            {
                // TODO: MOVE MAPPING ITEM
                case "Up":
                    MoveUp(selectedMapping);
                    break;
                case "Down":
                    MoveDown(selectedMapping);
                    break;
                case "Top":
                    MoveToTop(selectedMapping);
                    break;
                case "Bottom":
                    MoveToBottom(selectedMapping);
                    break;
                case "Custom":
                    var popupView = new Xafology.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application);
                    popupView.Accepting += DialogController_Accepting;
                    popupView.ShowNonPersistentView(typeof(MoveToIndexParam));
                    break;
            }
        }

        private void MoveUp(BaseObject mapping)
        {
            var session = mapping.Session;
            var prevIndex = session.ExecuteScalar(string.Format("SELECT Max([{2}]) FROM {1} WHERE [{2}] < {0} AND GCRecord IS NULL",
                mapping.GetMemberValue(IndexName), TargetTableName, IndexName));
            if (prevIndex == null)
                return;
            session.ExecuteNonQuery(string.Format("UPDATE {1} SET [{2}] = [{2}] + 1"
                + " WHERE [{2}] = {0} AND GCRecord IS NULL", prevIndex, TargetTableName, IndexName));
            mapping.SetMemberValue("Index", (int)prevIndex);
            mapping.Save();
            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();
        }

        private void MoveDown(BaseObject mapping)
        {
            var session = mapping.Session;
            var prevIndex = session.ExecuteScalar(string.Format("SELECT Min([{2}]) FROM {1} WHERE [{2}] > {0} AND GCRecord IS NULL",
                mapping.GetMemberValue(IndexName), TargetTableName, IndexName));
            if (prevIndex == null)
                return;
            session.ExecuteNonQuery(string.Format("UPDATE {1} SET [{2}] = [{2}] - 1"
                + " WHERE [{2}] = {0} AND GCRecord IS NULL", prevIndex, TargetTableName, IndexName));
            mapping.SetMemberValue(IndexName, (int)prevIndex);
            mapping.Save();
            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();
        }

        private void MoveToTop(BaseObject mapping)
        {
            // add 1 to everything above
            var session = mapping.Session;
            
            var firstIndex = session.ExecuteScalar(string.Format("SELECT Min([{1}]) FROM {0}",
                TargetTableName, IndexName));
            if (firstIndex == null)
                return;
            if ((int)firstIndex == 1)
                session.ExecuteNonQuery(string.Format("UPDATE {0} SET [{1}] = [{1}] + 1 WHERE GCRecord IS NULL",
                    TargetTableName, IndexName));
            mapping.SetMemberValue(IndexName, 1);
            mapping.Save();
            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();
        }

        private void MoveToBottom(BaseObject mapping)
        {
            var session = mapping.Session;
            var firstIndex = session.ExecuteScalar(string.Format("SELECT Max([{1}]) FROM {0} WHERE GCRecord IS NULL",
                TargetTableName, IndexName));
            if (firstIndex == null)
                return;
            mapping.SetMemberValue(IndexName, (int)firstIndex + 1);
            mapping.Save();
            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();
        }

        private void MoveToCustom(BaseObject mapping, int index)
        {
            int currentIndex = (int)mapping.GetMemberValue(IndexName);
            var session = mapping.Session;

            if (index == currentIndex) return;
            
            object targetIndex = null;

            targetIndex = session.ExecuteScalar(string.Format("SELECT [{2}] FROM {1} WHERE [{2}] = {0} AND GCRecord IS NULL",
                    index, TargetTableName, IndexName));
            if (targetIndex == null)
            {
                targetIndex = index;
            }
            else if (index < currentIndex)
            {
                session.ExecuteNonQuery(string.Format("UPDATE {1} SET [{3}] = [{3}] + 1"
                + " WHERE [{3}] >= {0} AND [{3}] < {2} AND GCRecord IS NULL", 
                targetIndex, TargetTableName, currentIndex, IndexName));
            }
            else
            {
                session.ExecuteNonQuery(string.Format("UPDATE {1} SET [{3}] = [{3}] - 1"
                + " WHERE [{3}] <= {0} AND [{3}] > {2} AND GCRecord IS NULL", 
                targetIndex, TargetTableName, currentIndex, IndexName));
            }
            mapping.SetMemberValue("Index", (int)targetIndex);
            mapping.Save();
            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();
        }

        void DialogController_Accepting(object sender, DevExpress.ExpressApp.SystemModule.DialogControllerAcceptingEventArgs e)
        {
            var paramObj = (MoveToIndexParam)e.AcceptActionArgs.CurrentObject;
            MoveToCustom((BaseObject)View.CurrentObject, paramObj.Index);
        }

        private SingleChoiceAction moveAction;
    }
}
