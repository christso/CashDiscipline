using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpo;
using CTMS.Module.BusinessObjects;

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Xpo;
using System.Diagnostics;

namespace CTMS.Module.Controllers.Cash
{
    public class BankStmtViewController : ViewControllerEx
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
                    AutoreconcileForexTrades(View.SelectedObjects);
                    break;
            }
        }

        private void ReconcileForecastChoice(XafApplication app, View view)
        {
            var bankStmt = (BankStmt)view.CurrentObject;
            // reconcile specific statement line
            var objSpace = app.CreateObjectSpace();

            // Show Cash flow View
            var dialog = new D2NXAF.ExpressApp.SystemModule.PopupDialogListViewManager(app, typeof(CashFlow), objSpace);
            dialog.Accepting += reconcileForecastDialog_Accepting;
            dialog.ShowView("CashFlow_LookupListView",
                CriteriaOperator.Parse("TranDate = ? And Status = ? And Account.Oid = ? And AccountCcyAmt = ?",
                bankStmt.TranDate, CashFlowStatus.Forecast, bankStmt.Account.Oid, bankStmt.TranAmount));
        }

        void reconcileForecastDialog_Accepting(object sender, DevExpress.ExpressApp.SystemModule.DialogControllerAcceptingEventArgs e)
        {
            // TODO: can selected objects be cast into a List?
            // Record the Cash Flow Forecast that was reconciled with the Bank Stmt
            BankStmtCashFlowForecast bsCff = ReconcileForecast(((XPObjectSpace)View.ObjectSpace).Session, (BankStmt)View.CurrentObject, (CashFlow)e.AcceptActionArgs.CurrentObject);
            ChangeBankStmtToCashFlowForecast(bsCff.BankStmt, bsCff.CashFlow);
        }

        // Record the Cash Flow Forecast that was reconciled with the Bank Stmt

        private static BankStmtCashFlowForecast ReconcileForecast(Session session, BankStmt bankStmtObj, CashFlow cashFlowObj)
        {
            // ensure both objects belong to the same session
            BankStmt bankStmt = bankStmtObj;
            if (session != bankStmtObj.Session)
                bankStmt = session.GetObjectByKey<BankStmt>(session.GetKeyValue(bankStmtObj));
            CashFlow cashFlow = cashFlowObj;
            if (session != cashFlowObj.Session)
                cashFlow = session.GetObjectByKey<CashFlow>(session.GetKeyValue(cashFlowObj));

            // associate the bank stmt object with the cash flow forecast object using a link object
            var bsCff = (BankStmtCashFlowForecast)session.FindObject<BankStmtCashFlowForecast>(CriteriaOperator.Parse(
                "BankStmt = ?", bankStmt));
            if (bsCff == null)
            {
                bsCff = new BankStmtCashFlowForecast(session);
                bsCff.BankStmt = bankStmt;
                bsCff.CashFlow = cashFlow;
            }
            session.CommitTransaction();
            return bsCff;
        }

        // Auto-reconcile BankStmt with ForexTrades
        public static void AutoreconcileForexTrades(System.Collections.IEnumerable bankStmts)
        {
            // get session from first BankStmt. This assumes the same sessions are being used.
            Session session = null;
            foreach (BankStmt bs in bankStmts)
            {
                session = bs.Session;
                break;
            }
            if (session == null) return;
            var transferActivity = session.GetObjectByKey<Activity>(
                SetOfBooks.CachedInstance.ForexSettleActivity.Oid);

            foreach (BankStmt bsi in bankStmts)
            {
                //Debug.Print(string.Format("Autoreconcile Activity {0} with {1}", bsi.Activity.Name, transferActivity.Name));
                if (bsi.Activity.Oid != transferActivity.Oid) continue;
                var cf = session.FindObject<CashFlow>(CriteriaOperator.Parse(
                    "TranDate = ? And Activity = ? And AccountCcyAmt = ?", 
                    bsi.TranDate, transferActivity, bsi.TranAmount));
                if (cf == null) break;

                BankStmtCashFlowForecast bsCff = ReconcileForecast(session, bsi, cf);
                ChangeBankStmtToCashFlowForecast(bsCff.BankStmt, bsCff.CashFlow);

            }
            session.CommitTransaction();
        }

        private static void ChangeBankStmtToCashFlowForecast(BankStmt bankStmt, CashFlow cashFlow)
        {
            bool oldCalculateEnabled = bankStmt.CalculateEnabled;
            bankStmt.CalculateEnabled = false;

            bankStmt.CounterCcy = cashFlow.CounterCcy;
            bankStmt.CounterCcyAmt = cashFlow.CounterCcyAmt;
            bankStmt.FunctionalCcyAmt = cashFlow.FunctionalCcyAmt;
            bankStmt.SummaryDescription = cashFlow.Description;
            bankStmt.Counterparty = cashFlow.Counterparty;

            bankStmt.CalculateEnabled = oldCalculateEnabled;
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
