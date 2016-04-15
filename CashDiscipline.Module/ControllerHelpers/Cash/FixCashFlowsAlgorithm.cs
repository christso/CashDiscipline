using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.BusinessObjects.Setup;
using CashDiscipline.Module.ParamObjects.Cash;
using Xafology.ExpressApp.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xafology.Spreadsheet.Attributes;
using DevExpress.Persistent.BaseImpl;
using Xafology.ExpressApp.Xpo.Import;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects;

namespace CashDiscipline.Module.ControllerHelpers.Cash
{
    public class FixCashFlowsAlgorithm : IFixCashFlows
    {
        private XPObjectSpace objSpace;
        private CashFlowFixParam paramObj;
        private Activity paramApReclassActivity;
        private Counterparty defaultCounterparty;
        private CashForecastFixTag reversalFixTag;
        private CashForecastFixTag revRecFixTag;
        private CashForecastFixTag resRevRecFixTag;
        private CashForecastFixTag payrollFixTag;
        private SetOfBooks setOfBooks;

        private List<CashFlow> cashFlowsToDelete;

        public FixCashFlowsAlgorithm(XPObjectSpace objSpace, CashFlowFixParam paramObj)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;
            // default values and parameters
            if (paramObj.ApReclassActivity == null)
                throw new InvalidOperationException("AP Reclass Activity must be defined.");
            paramApReclassActivity = objSpace.GetObjectByKey<Activity>(objSpace.GetKeyValue(paramObj.ApReclassActivity));
            defaultCounterparty = objSpace.FindObject<Counterparty>(
             CriteriaOperator.Parse("Name LIKE ?", Constants.DefaultFixCounterparty));

            var query = new XPQuery<CashForecastFixTag>(objSpace.Session);

            reversalFixTag = query
                .Where(x => x.Name == Constants.ReversalFixTag).FirstOrDefault();

            revRecFixTag = query
                .Where(x => x.Name == Constants.RevRecFixTag).FirstOrDefault();

            resRevRecFixTag = query
                .Where(x => x.Name == Constants.ResRevRecFixTag).FirstOrDefault();

            payrollFixTag = query
                .Where(x => x.Name == Constants.PayrollFixTag).FirstOrDefault();

            this.cashFlowsToDelete = new List<CashFlow>();

            setOfBooks = SetOfBooks.GetInstance(objSpace);
        }

        #region Reset

        // delete existing fixes
        public void Reset()
        {
            DeleteOrphans();
            ResetFixStatus();
        }

        // delete fixes that are not linked to any cash flow
        // because they have been unlinked
        public void DeleteOrphans()
        {
            CashFlowSnapshot currentSnapshot = GetCurrentSnapshot(objSpace.Session);

            var cashFlowFixes = new XPQuery<CashFlow>(objSpace.Session)
                .Where(cf =>
                cf.Snapshot.Oid == currentSnapshot.Oid
                && cf.Fix != null
                && (cf.Fix == reversalFixTag
                    || cf.Fix == revRecFixTag
                    || cf.Fix == resRevRecFixTag)
                && cf.ParentCashFlow == null);

            foreach (var cashFlowFix in cashFlowFixes)
                cashFlowFix.Delete();

            objSpace.CommitChanges();
        }

        public void ResetFixStatus()
        {
            CashFlowSnapshot currentSnapshot = GetCurrentSnapshot(objSpace.Session);

            var cashFlows = new XPQuery<CashFlow>(objSpace.Session)
                .Where(cf =>
                cf.Snapshot.Oid == currentSnapshot.Oid);
            foreach (var cashFlow in cashFlows)
            {
                cashFlow.IsFixeeSynced = false;
                cashFlow.IsFixerSynced = false;
                cashFlow.IsFixerFixeesSynced = false;
            }
            objSpace.CommitChanges();
        }
        #endregion

        #region Process

        public void ProcessCashFlows()
        {
            cashFlowsToDelete.Clear();

            DeleteOrphans();

            var cashFlows = GetCashFlowsToFix().OrderBy(x => x.TranDate);

            foreach (var cashFlow in cashFlows)
            {
                ProcessCashFlowsFromFixee(cashFlows, cashFlow);
            }

            objSpace.Delete(cashFlowsToDelete);
            cashFlowsToDelete.Clear();

            FinalizeStatus(cashFlows);

            objSpace.CommitChanges();
        }

        public void FinalizeStatus(IEnumerable<CashFlow> cashFlows)
        {
            // update Fixer status
            foreach (var cf in cashFlows)
            {
                cf.IsFixeeSynced = true;
                cf.IsFixerSynced = true;
                cf.IsFixerFixeesSynced = true;
                cf.Save();
            }
        }

        private void ProcessCashFlowsFromFixee(IEnumerable<CashFlow> cashFlows, CashFlow fixee)
        {      
            // mark cash flows for deletion
            foreach (var child in fixee.ChildCashFlows)
            {
                cashFlowsToDelete.Add(child);
            }

            if (fixee.CounterCcyAmt == -350)
            {
                // TODO: why does GetFixers return null?
            }

            var fixers = GetFixers(cashFlows, fixee);
            var fixer = ChooseFixer(fixers, fixee);

            if (fixer != null)
            {
                RephaseFixer(fixer);
                CreateFixes(fixer, fixee);
                fixee.Fixer = fixer;
            }
        }

        private void RephaseFixer(CashFlow cashFlow)
        {
            // adjust date of outflows
            if (cashFlow.Fix.FixTagType == CashForecastFixTagType.ScheduleOut
                && cashFlow.FixRank > 2
                && cashFlow.TranDate <= paramObj.ApayableLockdownDate)
            {
                // TODO: exclude Payroll, Progen, Bank Fee, Tax by specifying Fix in Activity
                cashFlow.TranDate = paramObj.ApayableNextLockdownDate;
            }
        }

        // This will return all cash flows which have changed after it was fixed
        public IEnumerable<CashFlow> GetCashFlowsToFix()
        {
            CashFlowSnapshot currentSnapshot = GetCurrentSnapshot(objSpace.Session);
            var cashFlows = new XPQuery<CashFlow>(objSpace.Session)
                .Where(cf =>
                cf.TranDate >= paramObj.FromDate && cf.TranDate <= paramObj.ToDate
                && cf.Snapshot.Oid == currentSnapshot.Oid
                && (cf.Fix == null || cf.Fix.FixTagType != CashForecastFixTagType.Ignore)
                && (!cf.IsFixeeSynced && !cf.IsFixerSynced) || !cf.IsFixerFixeesSynced);

            return cashFlows;
        }

        public CashFlow ChooseFixer(IEnumerable<CashFlow> fixers, CashFlow fixee)
        {
            var fixer = fixers.Where(cf => cf.CounterCcy == fixee.CounterCcy).FirstOrDefault();
            if (fixer == null)
                fixer = fixers.FirstOrDefault();

            return fixer;
        }

        public IEnumerable<CashFlow> GetFixers(IEnumerable<CashFlow> cashFlows, CashFlow fixee)
        {
            return cashFlows.Where((fixer) => GetFixCriteria(fixee, fixer) && 
                (!fixer.IsFixerSynced || !fixer.IsFixerFixeesSynced))
                .OrderBy((fixer) => fixee.TranDate);
        }

        public bool GetFixCriteria(CashFlow fixee, CashFlow fixer)
        {
            return fixee.DateUnFix >= fixer.FixFromDate && fixee.DateUnFix <= fixer.FixToDate
                        // should we do activity = fixactivity as well?
                        && fixee.FixActivity == fixer.FixActivity

                        && fixer.Status == CashFlowStatus.Forecast
                        && fixer.FixRank > fixee.FixRank
                        && (fixer.Counterparty == null || fixer.Counterparty == defaultCounterparty
                        || fixee.Counterparty == null && fixer.Counterparty == null
                        || fixee.Counterparty != null && fixer.Counterparty == fixee.Counterparty.FixCounterparty
                        )
                        && fixee.Account != null && fixee.Account.FixAccount == fixer.Account.FixAccount;
        }

        #endregion

        #region Fix Creator

        public void CreateFixes(CashFlow fixer, CashFlow fixee)
        {
            // base reversal

            var revFix = objSpace.CreateObject<CashFlow>();

            revFix.Account = fixee.Account.FixAccount ?? fixee.Account;

            revFix.ParentCashFlow = fixee;
            revFix.Activity = fixee.FixActivity;
            revFix.TranDate = fixer.TranDate;
            revFix.CounterCcy = fixer.CounterCcy;
            revFix.AccountCcyAmt = -fixee.AccountCcyAmt;
            revFix.CounterCcyAmt = -fixee.CounterCcyAmt;
            revFix.FunctionalCcyAmt = -fixee.FunctionalCcyAmt;
            revFix.Source = fixee.FixActivity.FixSource;
            revFix.Fix = reversalFixTag;
           
            // if the currencies do not match, use functional currency
            if (fixee.CounterCcy != fixer.CounterCcy)
            {
                revFix.CounterCcy = setOfBooks.FunctionalCurrency;
                revFix.CounterCcyAmt = -fixee.FunctionalCcyAmt;
            }

            // Reversal logic for AP Lockdown (i.e. payroll is excluded)
            // for Allocate FixTagType
            if (fixee.TranDate <= paramObj.ApayableLockdownDate
                && fixer.TranDate <= paramObj.ApayableLockdownDate
                && fixer.Fix.FixTagType == CashForecastFixTagType.Allocate
               && fixee.Fix != payrollFixTag)
            {
                fixer.IsReclass = true;
                revFix.IsReclass = true;

                ReclassFixeeFix(revFix, fixee);
                ReclassFixerFix(revFix, fixer);
            }

            revFix.Save();
            fixee.Save();
        }

        public void ReclassFixeeFix(CashFlow revFix, CashFlow fixee)
        {
            #region Accounts Payable

            var revRecFixee = objSpace.CreateObject<CashFlow>();
            revRecFixee.ParentCashFlow = fixee;
            revRecFixee.TranDate = revFix.TranDate;
            revRecFixee.Activity = revFix.Activity;
            revRecFixee.Account = revFix.Account;
            revRecFixee.CounterCcy = revFix.CounterCcy;
            revRecFixee.AccountCcyAmt = -1 * revFix.AccountCcyAmt;
            revRecFixee.CounterCcyAmt = -1 * revFix.CounterCcyAmt;
            revRecFixee.FunctionalCcyAmt = -1 * revFix.FunctionalCcyAmt;
            revRecFixee.Source = revFix.Source;
            revRecFixee.Activity = paramApReclassActivity;
            revRecFixee.Fix = revRecFixTag;
            revRecFixee.IsReclass = true;
            revRecFixee.Save();

            var resRevRecFixee = objSpace.CreateObject<CashFlow>();
            resRevRecFixee.Activity = revRecFixee.Activity;
            resRevRecFixee.Account = revRecFixee.Account;
            resRevRecFixee.CounterCcy = revRecFixee.CounterCcy;
            resRevRecFixee.AccountCcyAmt = -1 * revRecFixee.AccountCcyAmt;
            resRevRecFixee.FunctionalCcyAmt = -1 * revRecFixee.FunctionalCcyAmt;
            resRevRecFixee.CounterCcyAmt = -1 * revRecFixee.CounterCcyAmt;
            resRevRecFixee.Source = revRecFixee.Source;
            resRevRecFixee.TranDate = paramObj.ApayableNextLockdownDate < revFix.TranDate
                ? revFix.TranDate : paramObj.ApayableNextLockdownDate;
            resRevRecFixee.Fix = resRevRecFixTag;
            resRevRecFixee.IsReclass = false;
            resRevRecFixee.Save();

            #endregion
        }

        public void ReclassFixerFix(CashFlow revFix, CashFlow fixer)
        {
            #region Allocate
            // Reverse 'Allocate' To Reclass where <= 2 week 
            var revRecFixer = objSpace.CreateObject<CashFlow>();
            revRecFixer.Account = fixer.Account;
            revRecFixer.CounterCcy = fixer.CounterCcy;
            revRecFixer.AccountCcyAmt = -1 * fixer.AccountCcyAmt;
            revRecFixer.FunctionalCcyAmt = -1 * fixer.FunctionalCcyAmt;
            revRecFixer.CounterCcyAmt = -1 * fixer.CounterCcyAmt;
            revRecFixer.Source = fixer.Source;
            revRecFixer.Fix = revRecFixTag;
            revRecFixer.TranDate = fixer.TranDate;
            revRecFixer.Activity = paramApReclassActivity;
            revRecFixer.IsReclass = true;
            revRecFixer.Save();

            // Reverse "Reverse Allocate To Reclass" back into week 3
            var resRevRecFixer = objSpace.CreateObject<CashFlow>();
            resRevRecFixer.Activity = revRecFixer.Activity;
            resRevRecFixer.Account = revRecFixer.Account;
            resRevRecFixer.CounterCcy = revRecFixer.CounterCcy;
            resRevRecFixer.AccountCcyAmt = -1 * revRecFixer.AccountCcyAmt;
            resRevRecFixer.FunctionalCcyAmt = -1 * revRecFixer.FunctionalCcyAmt;
            resRevRecFixer.CounterCcyAmt = -1 * revRecFixer.CounterCcyAmt;
            resRevRecFixer.Source = revRecFixer.Source;
            resRevRecFixer.Fix = resRevRecFixTag;
            resRevRecFixer.TranDate = paramObj.ApayableNextLockdownDate < revFix.TranDate
                ? revFix.TranDate : paramObj.ApayableNextLockdownDate;
            resRevRecFixer.IsReclass = false;
            resRevRecFixer.Save();

            #endregion

        }

        #endregion

        #region Helpers

        private CashFlowSnapshot GetCurrentSnapshot(Session session)
        {
            return CashFlowHelper.GetCurrentSnapshot(session);
        }

        #endregion

    }
}
