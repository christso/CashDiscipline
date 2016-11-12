using CashDiscipline.Module.BusinessObjects.FinAccounting;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

using CashDiscipline.Module.ParamObjects.FinAccounting;
using DevExpress.ExpressApp.Xpo;
using CashDiscipline.Module.Logic.FinAccounting;
using DevExpress.ExpressApp.SystemModule;

namespace CashDiscipline.Module.Controllers.FinAccounting
{
    public class FinGenJournalParamViewController : ViewController
    {
        public FinGenJournalParamViewController()
        {
            TargetObjectType = typeof(FinGenJournalParam);

            SimpleAction okAction = new SimpleAction(this, "OkGenJnlction", "ExecuteActions");
            okAction.Caption = "OK";
            okAction.Execute += OkAction_Execute;

            SimpleAction cancelAction = new SimpleAction(this, "CancelGenJnlction", "ExecuteActions");
            cancelAction.Caption = "Cancel";
            cancelAction.Execute += CancelAction_Execute;

            SimpleAction deleteAction = new SimpleAction(this, "DeleteGenJnlction", "ExecuteActions");
            deleteAction.Caption = "Delete";
            deleteAction.Execute += DeleteAction_Execute;

        }

        private void DeleteAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            FinGenJournalParam paramObj = e.CurrentObject as FinGenJournalParam;
            var journalGenerator = new ParamJournalGenerator(paramObj, (XPObjectSpace)ObjectSpace);
            journalGenerator.DeleteAutoGenLedgerItems();
            ObjectSpace.CommitChanges();
            View.Close();

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
              "Auto-generated journals successfully deleted. Manual journals are untouched. Please refresh the Cash Gen Ledger Report.",
              "Auto-Journal Deletion SUCCESS");
        }

        private void OkAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

            FinGenJournalParam paramObj = e.CurrentObject as FinGenJournalParam;
            var journalGenerator = new ParamJournalGenerator(paramObj, (XPObjectSpace)ObjectSpace);
            journalGenerator.Execute();
            ObjectSpace.CommitChanges();
            View.Close();

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                "Journals successfully generated. Please refresh the Cash Gen Ledger Report.", 
                "Journal Generation SUCCESS");
        }

        private void CancelAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            View.Close();
        }

        protected override void OnActivated()
        {
            base.OnActivated();
        }
    }
}

