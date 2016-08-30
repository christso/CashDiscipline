using CashDiscipline.Module.BusinessObjects.FinAccounting;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

using CashDiscipline.Module.ParamObjects.FinAccounting;
using DevExpress.ExpressApp.Xpo;
using CashDiscipline.Module.Logic.FinAccounting;

namespace CashDiscipline.Module.Controllers.FinAccounting
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
        private JournalGenerator journalGenerator;

        void genJnlAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var objSpace = (XPObjectSpace)Application.CreateObjectSpace();
            var paramObj = FinGenJournalParam.GetInstance(objSpace);
            _ParamObj = paramObj;
            journalGenerator = new JournalGenerator(paramObj, objSpace);

            var dialog = new Xafology.ExpressApp.SystemModule.PopupDialogDetailViewManager(Application);
            dialog.Accepting += dialog_Accepting;
            dialog.ShowView(objSpace, paramObj);
        }

        // generate for selected CashFlows and BankStmts
        void dialog_Accepting(object sender, DevExpress.ExpressApp.SystemModule.DialogControllerAcceptingEventArgs e)
        {
            journalGenerator.Execute();
        }
    }
}
