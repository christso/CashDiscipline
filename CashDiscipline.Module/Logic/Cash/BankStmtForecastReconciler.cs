﻿using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.BusinessObjects.BankStatement;
using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Cash
{
    public class BankStmtForecastReconciler
    {
        private XPObjectSpace objSpace;
        public BankStmtForecastReconciler(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }

        public void ReconcileItem(BankStmt bankStmt, CashFlow cashFlow)
        {
            // TODO: can selected objects be cast into a List?
            // Record the Cash Flow Forecast that was reconciled with the Bank Stmt
            BankStmtCashFlowForecast bsCff = ReconcileForecast(objSpace.Session,
                bankStmt, cashFlow);
            ChangeBankStmtToCashFlowForecast(bsCff.BankStmt, bsCff.CashFlow);
        }

        // Auto-reconcile BankStmt with ForexTrades
        public void AutoreconcileTransfers(System.Collections.IEnumerable bankStmts)
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

        #region Static Members

        private static BankStmtCashFlowForecast ReconcileForecast(Session session, BankStmt bankStmtObj, CashFlow cashFlowObj)
        {
            // use the same session for both BankStmt and CashFlow object
            BankStmt bankStmt = bankStmtObj;
            if (session != bankStmtObj.Session)
                bankStmt = session.GetObjectByKey<BankStmt>(session.GetKeyValue(bankStmtObj));
            CashFlow cashFlow = cashFlowObj;
            if (session != cashFlowObj.Session)
                cashFlow = session.GetObjectByKey<CashFlow>(session.GetKeyValue(cashFlowObj));

            // associate the bank stmt object with the cash flow forecast object using a link object (BankStmtCashFlowForecast)
            // if link object does not exist, then create it
            var bsCff = session.FindObject<BankStmtCashFlowForecast>(CriteriaOperator.Parse(
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
        #endregion

    }
}