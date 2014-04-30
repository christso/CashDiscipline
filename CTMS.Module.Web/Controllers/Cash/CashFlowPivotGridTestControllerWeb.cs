using CTMS.Module.BusinessObjects.User;
using CTMS.Module.HelperClasses.UI;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.PivotGrid.Web;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Web.ASPxPivotGrid;
using DevExpress.XtraPivotGrid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Web.Controllers.Cash
{
    public class CashFlowPivotGridTestControllerWeb : ViewController
    {
        private SingleChoiceAction myLayoutAction;

        public CashFlowPivotGridTestControllerWeb()
        {
            TargetViewType = ViewType.ListView;
            TargetViewId = "CashFlow_PivotGridViewTest";

            var testAction = new SimpleAction(this, "TestAction", DevExpress.Persistent.Base.PredefinedCategory.View);
            testAction.Execute += testAction_Execute;


            myLayoutAction = new SingleChoiceAction(this, "UserLayoutTestAction", DevExpress.Persistent.Base.PredefinedCategory.View);
            myLayoutAction.Caption = "Layout";
            myLayoutAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            myLayoutAction.DefaultItemMode = DefaultItemMode.FirstActiveItem;
            myLayoutAction.ShowItemsOnClick = true;
            myLayoutAction.Execute += myLayoutAction_Execute;

            var saveLayoutChoice = new ChoiceActionItem();
            saveLayoutChoice.Caption = "Save";
            myLayoutAction.Items.Add(saveLayoutChoice);

            var loadLayoutChoice = new ChoiceActionItem();
            loadLayoutChoice.Caption = "Load";
            myLayoutAction.Items.Add(loadLayoutChoice);

            var resetChoice = new ChoiceActionItem();
            resetChoice.Caption = "Reset";
            myLayoutAction.Items.Add(resetChoice);
        }

        void myLayoutAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var objSpace = Application.CreateObjectSpace();

            switch (e.SelectedChoiceActionItem.Caption)
            {
                case "Save":
                    var saveDialog = new PopupDialogListViewManager(Application, typeof(UserViewLayout), objSpace);
                    saveDialog.Accepting += saveDialog_Accepting;
                    saveDialog.ShowView();
                    break;
                case "Load":
                    var loadDialog = new PopupDialogListViewManager(Application, typeof(UserViewLayout), objSpace);
                    loadDialog.Accepting += loadDialog_Accepting;
                    loadDialog.ShowView();
                    break; 
                case "Reset":
                    myLayoutAction.Caption = "Layout";
                    ResetPivotGridLayout();
                    break;
            }
        }

        private void loadDialog_Accepting(object sender, DialogControllerAcceptingEventArgs e)
        {
            var controller = (DialogController)sender;
            var objSpace = controller.Frame.View.ObjectSpace;

            var layoutObj = (UserViewLayout)e.AcceptActionArgs.CurrentObject;
            if (layoutObj != null)
            {
                LoadPivotGridLayout(layoutObj);
            }
            objSpace.CommitChanges();
        }


        protected virtual void ResetPivotGridLayout()
        {

        }


        private void saveDialog_Accepting(object sender, DialogControllerAcceptingEventArgs e)
        {
            var layoutObj = (UserViewLayout)e.AcceptActionArgs.CurrentObject;

            // save selected layout as default for user
            var settings = UserDefaultSetting.GetUserDefaultSettings(ObjectSpace);
            settings.UserViewLayout = ObjectSpace.GetObjectByKey<UserViewLayout>(layoutObj.Session.GetKeyValue(layoutObj));
            settings.Save();
            ObjectSpace.CommitChanges();
            SavePivotGridLayout(layoutObj);
        }

        protected void SavePivotGridLayoutToStream(MemoryStream stream)
        {
            PivotGridControl.SaveLayoutToStream(stream);
        }
        protected void SavePivotGridLayout(UserViewLayout layoutObj, MemoryStream stream)
        {
            myLayoutAction.Caption = string.Format("Layout : {0}", layoutObj.LayoutName);
            layoutObj.LayoutFile = new FileData(layoutObj.Session);
            layoutObj.LayoutFile.LoadFromStream("PivotGridLayout.xml", stream);
            layoutObj.Save();
            layoutObj.Session.CommitTransaction();
        }

        protected void SavePivotGridLayout(BusinessObjects.User.UserViewLayout layoutObj)
        {
            var stream = new MemoryStream();
            PivotGridControl.SaveLayoutToStream(stream);
            stream.Position = 0;
            SavePivotGridLayout(layoutObj, stream);
        }

        private void testAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var layoutObj = ObjectSpace.FindObject<UserViewLayout>(CriteriaOperator.Parse(
                "LayoutName = ?", Constants.CashFlowPivotLayoutMonthly));
            if (layoutObj == null) return;
            var stream = new MemoryStream(layoutObj.LayoutFile.Content);
            stream.Position = 0;
            PivotGridControl.LoadLayoutFromStream(stream); // TODO: error
        }

        protected void LoadPivotGridLayout(UserViewLayout layoutObj)
        {
            //PivotGridControl.Fields.Clear();
            var stream = new MemoryStream(layoutObj.LayoutFile.Content);
            stream.Position = 0;
            PivotGridControl.LoadLayoutFromStream(stream); // TODO: error
        }

        private ASPxPivotGrid _PivotGridControl;
        public ASPxPivotGrid PivotGridControl
        {
            get
            {
                return _PivotGridControl;
            }
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();

            var pgEditor = (ASPxPivotGridListEditor)((ListView)View).Editor;
            if (pgEditor != null)
                _PivotGridControl = (ASPxPivotGrid)pgEditor.PivotGridControl;

            PivotGridControl.Fields.Clear();

            var fieldTranDate = new PivotGridField("TranDate", PivotArea.FilterArea);
            fieldTranDate.Name = "fieldTranDate ";
            fieldTranDate.Caption = "Tran Date";
            fieldTranDate.ValueFormat.FormatType = DevExpress.Utils.FormatType.Custom;
            fieldTranDate.ValueFormat.FormatString = "{0:dd-MMM-yy}";
            PivotGridControl.Fields.Add(fieldTranDate);

            var fieldAccountCcyAmt = new PivotGridField("AccountCcyAmt", PivotArea.FilterArea);
            fieldAccountCcyAmt.Name = "fieldAccountCcyAmt";
            fieldAccountCcyAmt.Caption = "Account Ccy Amt";
            fieldAccountCcyAmt.CellFormat.FormatString = "n2";
            fieldAccountCcyAmt.CellFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            PivotGridControl.Fields.Add(fieldAccountCcyAmt);
        }
    }
}
