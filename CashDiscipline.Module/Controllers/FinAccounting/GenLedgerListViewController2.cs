using CashDiscipline.Module.BusinessObjects.FinAccounting;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

using CashDiscipline.Module.ParamObjects.FinAccounting;
using CashDiscipline.Module.Logic.FinAccounting;

namespace CashDiscipline.Module.Controllers.FinAccounting
{
    public class GenLedgerListViewController2 : ViewController
    {
        public GenLedgerListViewController2()
        {
            TargetObjectType = typeof(GenLedger);
            TargetViewType = ViewType.ListView;

            genJnlAction = new SimpleAction(this, "GenJnlAction2", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            genJnlAction.Caption = "Generate Journals 2";
            genJnlAction.Execute += genJnlAction_Execute;

        }

        private SimpleAction genJnlAction;
        private FinGenJournalParam _ParamObj;

        protected override void OnActivated()
        {
            base.OnActivated();
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
            var jg = new SqlJournalGenerator(_ParamObj);
            jg.Execute();
        }
    }
}
