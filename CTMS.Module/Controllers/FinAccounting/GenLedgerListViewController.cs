using CTMS.Module.BusinessObjects.FinAccounting;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

using CTMS.Module.ParamObjects.FinAccounting;

namespace CTMS.Module.Controllers.FinAccounting
{
    public class GenLedgerListViewController : ViewController
    {
        public GenLedgerListViewController()
        {
            TargetObjectType = typeof(GenLedger);
            TargetViewType = ViewType.ListView;

            genJnlAction = new SimpleAction(this, "GenJnlAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            genJnlAction.Caption = "Generate Journals";
            genJnlAction.Execute += genJnlAction_Execute;
        }

        private SimpleAction genJnlAction;
        private FinGenJournalParam _ParamObj;

        protected override void OnActivated()
        {
            base.OnActivated();
            View.ControlsCreated += View_ControlsCreated;
        }

        void View_ControlsCreated(object sender, EventArgs e)
        {
            var editor = ((ListView)View).Editor;
        }

        void genJnlAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var objSpace = Application.CreateObjectSpace();
            var paramObj = FinGenJournalParam.GetInstance(objSpace);
            _ParamObj = paramObj;
            var dialog = new Xafology.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application);
            dialog.Accepting += dialog_Accepting;
            dialog.ShowView(objSpace, paramObj);
        }

        // generate for selected CashFlows and BankStmts
        void dialog_Accepting(object sender, DevExpress.ExpressApp.SystemModule.DialogControllerAcceptingEventArgs e)
        {
            GenerateJournals(_ParamObj);
        }

        public static void GenerateJournals(FinGenJournalParam paramObj)
        {
            GenLedger.GenerateJournals(paramObj);
        }
    }
}
