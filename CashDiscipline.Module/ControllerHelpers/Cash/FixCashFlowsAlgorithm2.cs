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
    public class FixCashFlowsAlgorithm2 : IFixCashFlows
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

        public FixCashFlowsAlgorithm2(XPObjectSpace objSpace, CashFlowFixParam paramObj)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;
            // default values and parameters
            if (paramObj.ApReclassActivity == null)
                throw new InvalidOperationException("AP Reclass Activity must be defined.");
            paramApReclassActivity = objSpace.GetObjectByKey<Activity>(objSpace.GetKeyValue(paramObj.ApReclassActivity));
            defaultCounterparty = objSpace.FindObject<Counterparty>(
             CriteriaOperator.Parse("Name LIKE ?", "UNDEFINED"));

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

            objSpace.CommitChanges();
        }

        private void ProcessCashFlowsFromFixee(IEnumerable<CashFlow> cashFlows, CashFlow fixee)
        {      
            // mark cash flows for deletion
            foreach (var child in fixee.ChildCashFlows)
            {
                cashFlowsToDelete.Add(child);
            }

            var fixers = GetFixers(cashFlows, fixee);
            var fixer = fixers.FirstOrDefault();
            if (fixer != null)
            {
                RephaseFixer(fixer);
                CreateFixes(fixer, fixee);
                fixee.Fixer = fixer;
            }
            fixee.IsFixeeSynced = true;
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

        public IEnumerable<CashFlow> GetFixers(IEnumerable<CashFlow> cashFlows, CashFlow fixee)
        {
            return cashFlows.Where((fixer) => GetFixCriteria(fixee, fixer) && !fixer.IsFixerSynced)
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
            revFix.ParentCashFlow = fixee;
            revFix.Account = fixee.Account.FixAccount;
            revFix.Activity = fixee.FixActivity;
            revFix.TranDate = fixer.TranDate;
            revFix.CounterCcy = fixer.CounterCcy;
            revFix.AccountCcyAmt = -fixee.AccountCcyAmt;
            revFix.FunctionalCcyAmt = -fixee.FunctionalCcyAmt;
            revFix.CounterCcyAmt = -fixee.CounterCcyAmt;
            revFix.Source = fixee.FixActivity.FixSource;
            revFix.Fix = reversalFixTag;

            // if the currencies do not match
            if (fixee.CounterCcy != fixer.CounterCcy)
            {
                revFix.CounterCcy = setOfBooks.FunctionalCurrency;
                revFix.CounterCcyAmt = -fixee.FunctionalCcyAmt;
            }

            // Reversal logic for AP Lockdown (i.e. payroll is excluded)
            // for Allocate FixTagType
            if (fixee.TranDate <= paramObj.ApayableLockdownDate
                && fixer.Fix.FixTagType == CashForecastFixTagType.Allocate
               && fixee.Fix != payrollFixTag)
            {
                revFix.IsReclass = true;

                #region Accounts Payable

                var revRecFixee = objSpace.CreateObject<CashFlow>();
                revRecFixee.ParentCashFlow = fixee;
                revRecFixee.TranDate = revFix.TranDate;
                revRecFixee.Activity = revFix.Activity;
                revRecFixee.Account = revFix.Account;
                revRecFixee.AccountCcyAmt = -1 * revFix.AccountCcyAmt;
                revRecFixee.FunctionalCcyAmt = -1 * revFix.FunctionalCcyAmt;
                revRecFixee.CounterCcyAmt = -1 * revFix.CounterCcyAmt;
                revRecFixee.Source = revFix.Source;
                revRecFixee.CounterCcy = revFix.CounterCcy;
                revRecFixee.Activity = paramApReclassActivity;
                revRecFixee.Fix = revRecFixTag;
                revRecFixee.IsReclass = true;
                revRecFixee.Save();

                var resRevRecFixee = objSpace.CreateObject<CashFlow>();
                resRevRecFixee.Activity = revRecFixee.Activity;
                resRevRecFixee.Account = revRecFixee.Account;
                resRevRecFixee.AccountCcyAmt = -1 * revRecFixee.AccountCcyAmt;
                resRevRecFixee.FunctionalCcyAmt = -1 * revRecFixee.FunctionalCcyAmt;
                resRevRecFixee.CounterCcyAmt = -1 * revRecFixee.CounterCcyAmt;
                resRevRecFixee.Source = revRecFixee.Source;
                resRevRecFixee.CounterCcy = revRecFixee.CounterCcy;
                resRevRecFixee.TranDate = paramObj.ApayableNextLockdownDate;
                resRevRecFixee.Fix = resRevRecFixTag;
                resRevRecFixee.IsReclass = false;
                resRevRecFixee.Save();

                #endregion
            }

            if (fixee.TranDate <= paramObj.ApayableLockdownDate
                && fixer.Fix.FixTagType == CashForecastFixTagType.Allocate
               && fixee.Fix != payrollFixTag)
            {
                #region Allocate
                // Reverse 'Allocate' To Reclass where <= 2 week 
                var revRecFixer = objSpace.CreateObject<CashFlow>();
                revRecFixer.Account = fixer.Account;
                revRecFixer.AccountCcyAmt = -1 * fixer.AccountCcyAmt;
                revRecFixer.FunctionalCcyAmt = -1 * fixer.FunctionalCcyAmt;
                revRecFixer.CounterCcyAmt = -1 * fixer.CounterCcyAmt;
                revRecFixer.Source = fixer.Source;
                revRecFixer.CounterCcy = fixer.CounterCcy;
                revRecFixer.Fix = revRecFixTag;
                revRecFixer.TranDate = fixer.TranDate;
                revRecFixer.Activity = paramApReclassActivity;
                revRecFixer.IsReclass = true;
                revRecFixer.Save();

                // Reverse "Reverse Allocate To Reclass" back into week 3
                var resRevRecFixer = objSpace.CreateObject<CashFlow>();
                resRevRecFixer.Activity = revRecFixer.Activity;
                resRevRecFixer.Account = revRecFixer.Account;
                resRevRecFixer.AccountCcyAmt = -1 * revRecFixer.AccountCcyAmt;
                resRevRecFixer.FunctionalCcyAmt = -1 * revRecFixer.FunctionalCcyAmt;
                resRevRecFixer.CounterCcyAmt = -1 * revRecFixer.CounterCcyAmt;
                resRevRecFixer.Source = revRecFixer.Source;
                resRevRecFixer.CounterCcy = revRecFixer.CounterCcy;
                resRevRecFixer.Fix = resRevRecFixTag;
                resRevRecFixer.TranDate = paramObj.ApayableNextLockdownDate;
                resRevRecFixer.IsReclass = false;
                resRevRecFixer.Save();
                #endregion
            }
            revFix.Save();
            fixee.Save();
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
