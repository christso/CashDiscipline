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

            genJnlAction = new SingleChoiceAction(this, "GenJnlAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            genJnlAction.Caption = "Generate Journals";
            genJnlAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            genJnlAction.ShowItemsOnClick = false;
            genJnlAction.Execute += genJnlAction_Execute;

            var choice1 = new ChoiceActionItem();
            choice1.Caption = "Generate";
            genJnlAction.Items.Add(choice1);

        }

        private SingleChoiceAction genJnlAction;
        private FinGenJournalParam _ParamObj;
        private ParamJournalGenerator journalGenerator;

        void genJnlAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            ShowGeneratorForm();
        }

        private void ShowGeneratorForm()
        {
            var objSpace = (XPObjectSpace)Application.CreateObjectSpace();
            var paramObj = FinGenJournalParam.GetInstance(objSpace);
            _ParamObj = paramObj;
            journalGenerator = new ParamJournalGenerator(paramObj, objSpace);

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
