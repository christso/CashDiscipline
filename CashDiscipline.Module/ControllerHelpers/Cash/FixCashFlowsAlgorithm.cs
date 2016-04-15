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

namespace CashDiscipline.Module.ControllerHelpers.Cash
{
    public class FixCashFlowsAlgorithm
    {
        private XPObjectSpace objSpace;
        private CashFlowFixParam paramObj;
        private Activity paramApReclassActivity;
        private Counterparty defaultCounterparty;
        private CashForecastFixTag reversalFixTag;
        private CashForecastFixTag revRecFixTag;
        private CashForecastFixTag resRevRecFixTag;
        private CashForecastFixTag payrollFixTag;


        private List<CashFlow> cashFlowsToDelete = new List<CashFlow>();

        public FixCashFlowsAlgorithm(XPObjectSpace objSpace, CashFlowFixParam paramObj)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;
            // default values and parameters
            if (paramObj.ApReclassActivity == null)
                throw new InvalidOperationException("AP Reclass Activity must be defined.");
            paramApReclassActivity = objSpace.GetObjectByKey<Activity>(objSpace.GetKeyValue(paramObj.ApReclassActivity));
            defaultCounterparty = objSpace.FindObject<Counterparty>(
             CriteriaOperator.Parse("Name LIKE ?", "UNDEFINED"));
            reversalFixTag = objSpace.FindObject<CashForecastFixTag>(
                CriteriaOperator.Parse("Name = 'R'"));
            revRecFixTag = objSpace.FindObject<CashForecastFixTag>(
                CriteriaOperator.Parse("Name = 'RR'"));
            resRevRecFixTag = objSpace.FindObject<CashForecastFixTag>(
                CriteriaOperator.Parse("Name = 'RRR'"));
            payrollFixTag = objSpace.FindObject<CashForecastFixTag>(
                CriteriaOperator.Parse("Name = 'PY'"));
        }

        private CashFlowSnapshot GetCurrentSnapshot(Session session)
        {
            return CashFlowHelper.GetCurrentSnapshot(session);
        }

        public void ProcessCashFlows()
        {
            bool bTriggersEnabled = AppSettings.UserTriggersEnabled;

            cashFlowsToDelete.Clear();

            try
            {
                
                CashFlowSnapshot currentSnapshot = GetCurrentSnapshot(objSpace.Session);
                AppSettings.UserTriggersEnabled = false;

                // delete existing fixes that are not valid due to potential application bug, i.e. orphans.
                var cashFlowFixes = objSpace.GetObjects<CashFlow>(
                    CriteriaOperator.Parse(
                    "Snapshot = ? And Fix In (?,?,?) And ParentCashFlow Is Null",
                    currentSnapshot, reversalFixTag, revRecFixTag, resRevRecFixTag));
                objSpace.Delete(cashFlowFixes);

                var cashFlows = objSpace.GetObjects<CashFlow>(CriteriaOperator.Parse(
                "TranDate >= ? And TranDate <= ? And Fix.FixTagType != ?"
                + " And (Not IsFixerSynced Or IsFixerSynced Is Null)"
                + " And Snapshot = ?",
                paramObj.FromDate,
                paramObj.ToDate,
                CashForecastFixTagType.Ignore,
                currentSnapshot));

                // The fixee may require updating even if IsFixUpdated == True,
                // if a new fixer is entered with a criteria that fits the fixee.
                // This code will update the fixee in such a case.
                foreach (var fixer in cashFlows)
                {
                    var fixees = cashFlows.Where(delegate (CashFlow c)
                    {
                        return GetFixCriteria(c, fixer);
                    });
                    foreach (var fixee in fixees)
                    {
                        // since one fixee can have many fixers, we avoid
                        // running the algorithm twice on the same fixee
                        if (fixee.IsFixerSynced) continue;
                        FixFixee(cashFlows, fixee);
                    }
                    //fixer.IsFixUpdated = true;
                    fixer.Save();
                }

                foreach (var fixee in cashFlows)
                {
                    if (fixee.IsFixerSynced) continue;
                    FixFixee(cashFlows, fixee);
                }

                objSpace.Delete(cashFlowsToDelete);
                cashFlowsToDelete.Clear();
                objSpace.CommitChanges();
            }
            finally
            {
                AppSettings.UserTriggersEnabled = bTriggersEnabled;
            }
        }

        // TODO: get session from fixee instead of objspace
        private void FixFixee(IList<CashFlow> cashFlows, CashFlow fixee)
        {

            // delete existing fixes
            foreach (var child in fixee.ChildCashFlows)
            {
                if (child != fixee)
                    cashFlowsToDelete.Add(child);
            }
            //while (fixee.ChildCashFlows.Count != 0)
            //    fixee.ChildCashFlows[0].Delete();

            var fixers = cashFlows.Where(delegate (CashFlow c)
            {
                return GetFixCriteria(fixee, c);
            }).OrderBy(c => c.TranDate);

            var fixersWithCcy = fixers.Where(c => c.CounterCcy == fixee.CounterCcy);

            // fix with CounterCcy but use AUD if not found
            CashFlow fixer = fixersWithCcy.FirstOrDefault();
            if (fixer == null)
                fixer = fixers.FirstOrDefault();

            if (fixer == null) return;

            // base
            fixee.Fixer = fixer;

            // reversal
            var revFix = objSpace.CreateObject<CashFlow>();
            revFix.ParentCashFlow = fixee;
            revFix.Account = fixee.Account.FixAccount;
            revFix.Activity = fixee.FixActivity;
            revFix.TranDate = fixer.TranDate;
            revFix.AccountCcyAmt = -fixee.AccountCcyAmt;
            revFix.FunctionalCcyAmt = -fixee.FunctionalCcyAmt;
            revFix.CounterCcyAmt = -fixee.CounterCcyAmt;
            revFix.CounterCcy = fixer.CounterCcy;
            revFix.Source = fixee.FixActivity.FixSource;
            revFix.Fix = reversalFixTag;

            // Reversal logic for AP Lockdown (i.e. payroll is excluded)

            if (fixee.TranDate <= paramObj.ApayableLockdownDate
                 && fixer.Fix.FixTagType == CashForecastFixTagType.Allocate
                && fixee.Fix != payrollFixTag)
            {
                revFix.IsReclass = true;

                #region Accounts Payable
                // Reverse Reversal To Reclass where AP <= 2 weeks and Fixer is Allocate
                // i.e. reversal will have no effect in this case
                var revRecFix = objSpace.CreateObject<CashFlow>();
                CopyCashFlowFixDefaults(revFix, revRecFix, -1);
                revRecFix.Activity = paramApReclassActivity;
                revRecFix.Fix = revRecFixTag;
                revRecFix.IsReclass = true;

                // Reverse AP 'reversal reclass' back into week 3
                var resrevrecFix = objSpace.CreateObject<CashFlow>();
                CopyCashFlowFixDefaults(revRecFix, resrevrecFix, -1);
                resrevrecFix.TranDate = paramObj.ApayableNextLockdownDate;
                resrevrecFix.Fix = resRevRecFixTag;
                resrevrecFix.IsReclass = false;
                #endregion

                #region Allocate
                // Reverse 'Allocate' To Reclass where <= 2 week 
                // TODO: get Fix Source instead of underlying source
                var revRecFixer = objSpace.CreateObject<CashFlow>();
                CopyCashFlowFixDefaults(fixer, revRecFixer, -1);
                revRecFixer.Fix = revRecFixTag;
                revRecFixer.TranDate = paramObj.ApayableLockdownDate;
                revRecFixer.Activity = paramApReclassActivity;
                revRecFixer.IsReclass = true;

                // Reverse "Reverse Allocate To Reclass" back into week 3
                var resRevRecFix = objSpace.CreateObject<CashFlow>();
                CopyCashFlowFixDefaults(revRecFixer, resRevRecFix, -1);
                resRevRecFix.Fix = resRevRecFixTag;
                resRevRecFix.TranDate = paramObj.ApayableNextLockdownDate;
                resRevRecFix.IsReclass = false;
                #endregion

                revRecFix.Save();
                resrevrecFix.Save();
                revRecFixer.Save();
                resRevRecFix.Save();
            }
            revFix.Save();
            fixee.IsFixerSynced = true;
            fixee.Save();
        }

        private bool GetFixCriteria(CashFlow fixee, CashFlow fixer)
        {
            return fixee.DateUnFix >= fixer.FixFromDate && fixee.DateUnFix <= fixer.FixToDate
                        && fixee.FixActivity == fixer.FixActivity
                        // should we do activity = fixactivity as well?
                        && fixer.Status == CashFlowStatus.Forecast
                        && fixer.FixRank > fixee.FixRank
                        && (fixer.Counterparty == null || fixer.Counterparty == defaultCounterparty
                        || fixee.Counterparty == null && fixer.Counterparty == null
                        || fixee.Counterparty != null && fixer.Counterparty == fixee.Counterparty.FixCounterparty)
                        && fixee.Account != null && fixee.Account.FixAccount == fixer.Account.FixAccount;
        }

        private void CopyCashFlowFixDefaults(CashFlow source, CashFlow target, decimal amtFactor = 1)
        {
            target.ParentCashFlow = source.ParentCashFlow;
            target.TranDate = source.TranDate;
            target.ParentCashFlow = source.ParentCashFlow;
            target.Fixer = source.Fixer;
            target.Activity = source.Activity;
            target.Account = source.Account;
            target.AccountCcyAmt = amtFactor * source.AccountCcyAmt;
            target.FunctionalCcyAmt = amtFactor * source.FunctionalCcyAmt;
            target.CounterCcyAmt = amtFactor * source.CounterCcyAmt;
            target.Source = source.Source;
            target.CounterCcy = source.CounterCcy;
        }
    }
}
