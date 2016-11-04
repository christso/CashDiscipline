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
using CashDiscipline.Module.ParamObjects.Import;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class BankStmtViewController : ViewController
    {
        private const string mapCaption = "Map Selected";
        private const string importCaption = "Import";
        private const string recalcFuncCaption = "Recalc Func Amt";

        public BankStmtViewController()
        {
            TargetObjectType = typeof(BankStmt);

            BankStmtAction = new SingleChoiceAction(this, "RunBankStmtAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            BankStmtAction.Caption = "Actions";
            BankStmtAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            BankStmtAction.Execute += BankStmtAction_Execute;

            var importChoice = new ChoiceActionItem();
            importChoice.Caption = importCaption;
            BankStmtAction.Items.Add(importChoice);

            var mappingChoice = new ChoiceActionItem();
            mappingChoice.Caption = mapCaption;
            BankStmtAction.Items.Add(mappingChoice);

            var reconForecastChoice = new ChoiceActionItem();
            reconForecastChoice.Caption = "Reconcile Forecast";
            BankStmtAction.Items.Add(reconForecastChoice);

            var autoReconForecastChoice = new ChoiceActionItem();
            autoReconForecastChoice.Caption = "Auto-Reconcile Forecast";
            BankStmtAction.Items.Add(autoReconForecastChoice);

            var calcFuncAmtChoice = new ChoiceActionItem();
            calcFuncAmtChoice.Caption = recalcFuncCaption;
            BankStmtAction.Items.Add(calcFuncAmtChoice);

            //var testChoice = new ChoiceActionItem();
            //testChoice.Caption = "Test";
            //BankStmtAction.Items.Add(testChoice);

            BankStmtAction.ShowItemsOnClick = true;

    
        }

        private SingleChoiceAction BankStmtAction;
        private void BankStmtAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            var caption = e.SelectedChoiceActionItem.Caption;
            switch (caption)
            {
                case mapCaption:
                    MapSelected();
                    break;
                case importCaption:
                    ShowImportBankStmtForm(e.ShowViewParameters);
                    break;
                case "Reconcile Forecast":
                    ReconcileForecastChoice(Application, View);
                    break;
                case "Auto-Reconcile Forecast":
                    var reconciler = new BankStmtForecastReconciler((XPObjectSpace)View.ObjectSpace);
                    reconciler.AutoreconcileTransfers(View.SelectedObjects);
                    break;
                case recalcFuncCaption:
                    var calculator = new BankStmtCalculator((XPObjectSpace)View.ObjectSpace);
                    calculator.Process(View.SelectedObjects);
                    break;
                case "Test":

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

        private void ShowImportBankStmtForm(ShowViewParameters svp)
        {
            var os = Application.CreateObjectSpace();
            var paramObj = ImportBankStmtParam.GetInstance(os);
            var detailView = Application.CreateDetailView(os, paramObj);
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.CreatedView = detailView;
        }

        public void MapSelected()
        {
            var mapper = new BankStmtMapper((XPObjectSpace)ObjectSpace);
            var bankStmts = View.SelectedObjects;
            mapper.Process(bankStmts);
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
