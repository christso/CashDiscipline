using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpo;
using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.BusinessObjects.BankStatement;

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Xpo;
using System.Diagnostics;
using CashDiscipline.Module.Logic.Cash;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class BankStmtViewController : ViewController
    {
        public BankStmtViewController()
        {
            TargetObjectType = typeof(BankStmt);

            #region Mapping
            BankStmtAction = new SingleChoiceAction(this, "RunBankStmtAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            BankStmtAction.Caption = "Actions";
            BankStmtAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            BankStmtAction.Execute += BankStmtAction_Execute;

            // Test for BankStmt to Cash Flow Forecast
            //var bankStmtCashFlowActionItem = new ChoiceActionItem();
            //bankStmtCashFlowActionItem.Caption = "Cash Flow Forecast Links";
            //BankStmtAction.Items.Add(bankStmtCashFlowActionItem);

            var mappingActionItem = new ChoiceActionItem();
            mappingActionItem.Caption = "Mapping";
            BankStmtAction.Items.Add(mappingActionItem);
            #endregion

            var reconForecastActionItem = new ChoiceActionItem();
            reconForecastActionItem.Caption = "Reconcile Forecast";
            BankStmtAction.Items.Add(reconForecastActionItem);

            var autoReconForecastActionItem = new ChoiceActionItem();
            autoReconForecastActionItem.Caption = "Auto-Reconcile Forecast";
            BankStmtAction.Items.Add(autoReconForecastActionItem);
            BankStmtAction.ShowItemsOnClick = true;
        }

        private SingleChoiceAction BankStmtAction;
        private void BankStmtAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            switch (e.SelectedChoiceActionItem.Caption)
            {
                case "Mapping":
                    ExecuteMapping(View.SelectedObjects);
                    break;
                case "Reconcile Forecast":
                    ReconcileForecastChoice(Application, View);
                    break;
                case "Auto-Reconcile Forecast":
                    var reconciler = new BankStmtForecastReconciler((XPObjectSpace)View.ObjectSpace);
                    reconciler.AutoreconcileTransfers(View.SelectedObjects);
                    break;
            }
        }
        
        private void ReconcileForecastChoice(XafApplication app, View view)
        {
            var bankStmt = (BankStmt)view.CurrentObject;
            // reconcile specific statement line
            var objSpace = app.CreateObjectSpace();

            // Show Cash flow View
            var dialog = new Xafology.ExpressApp.SystemModule.PopupDialogListViewManager(app, typeof(CashFlow), objSpace);
            dialog.Accepting += reconcileForecastDialog_Accepting;
            dialog.ShowView("CashFlow_LookupListView",
                CriteriaOperator.Parse("TranDate = ? And Status = ? And Account.Oid = ? And AccountCcyAmt = ?",
                bankStmt.TranDate, CashFlowStatus.Forecast, bankStmt.Account.Oid, bankStmt.TranAmount));
        }

        void reconcileForecastDialog_Accepting(object sender, DevExpress.ExpressApp.SystemModule.DialogControllerAcceptingEventArgs e)
        {
            var reconciler = new BankStmtForecastReconciler((XPObjectSpace)View.ObjectSpace);

            // Record the Cash Flow Forecast that was reconciled with the Bank Stmt
            reconciler.ReconcileItem((BankStmt)View.CurrentObject, (CashFlow)e.AcceptActionArgs.CurrentObject);
        }

        public void ExecuteMapping(System.Collections.IList objects)
        {
            
            var objSpace = ObjectSpace;
            var mappings = objSpace.GetObjects<BankStmtMapping>();

            foreach (BankStmt bsi in objects)
            {
                foreach (var mapping in mappings)
                {
                    // update BSI with first matching expression
                    if (bsi.Fit(mapping.CriteriaExpression))
                    {
                        if (mapping.Activity != null)
                            bsi.Activity = mapping.Activity;
                        if (mapping.Account != null)
                            bsi.Account = mapping.Account;
                        break;
                    }
                }
                bsi.Save();
            }
            objSpace.CommitChanges();
        }

        public static string ReadTextFile(string FullPath)
        {
            string strContents = null;
            System.IO.StreamReader objReader = null;

            objReader = new System.IO.StreamReader(FullPath);
            strContents = objReader.ReadToEnd();
            objReader.Close();
            return strContents;
        }
    }
}
