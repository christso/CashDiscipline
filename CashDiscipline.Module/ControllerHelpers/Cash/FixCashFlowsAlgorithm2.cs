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
            reversalFixTag = objSpace.FindObject<CashForecastFixTag>(
                CriteriaOperator.Parse("Name = 'R'"));
            revRecFixTag = objSpace.FindObject<CashForecastFixTag>(
                CriteriaOperator.Parse("Name = 'RR'"));
            resRevRecFixTag = objSpace.FindObject<CashForecastFixTag>(
                CriteriaOperator.Parse("Name = 'RRR'"));
            payrollFixTag = objSpace.FindObject<CashForecastFixTag>(
                CriteriaOperator.Parse("Name = 'PY'"));
        }

        public void ProcessCashFlows()
        {
            CashFlowSnapshot currentSnapshot = GetCurrentSnapshot(objSpace.Session);

            var cashFlows = GetCashFlowsToFix().OrderBy(x => x.TranDate);

            foreach (var cashFlow in cashFlows)
            {
                ProcessCashFlowsFromFixer(cashFlows, cashFlow);
                ProcessCashFlowsFromFixee(cashFlows, cashFlow);
                cashFlow.IsFixerUpdated = true; 
            }

            objSpace.CommitChanges();
        }

        private void ProcessCashFlowsFromFixer(IEnumerable<CashFlow> cashFlows, CashFlow fixer)
        {
            // process from fixer
            var fixees = GetFixees(cashFlows, fixer);
            foreach (var fixee in fixees)
            {
                CreateFixes(fixer, fixee);
            }
        }


        private void ProcessCashFlowsFromFixee(IEnumerable<CashFlow> cashFlows, CashFlow fixee)
        {
            var fixer = GetFixers(cashFlows, fixee).FirstOrDefault();
            if (fixer != null)
            {
                CreateFixes(fixer, fixee);
            }
        }

        public void CreateFixes(CashFlow fixer, CashFlow fixee)
        {
            var rev = objSpace.CreateObject<CashFlow>();
            rev.TranDate = fixer.TranDate;
            rev.AccountCcyAmt = -fixee.AccountCcyAmt;
            fixee.IsFixeeUpdated = true;
        }

        // This will return all cash flows which have changed after it was fixed
        public IEnumerable<CashFlow> GetCashFlowsToFix()
        {
            CashFlowSnapshot currentSnapshot = GetCurrentSnapshot(objSpace.Session);

            var cashFlows = objSpace.GetObjects<CashFlow>(CriteriaOperator.Parse(
            "TranDate >= ? And TranDate <= ? And Fix.FixTagType != ?"
            + " And (Not IsFixerUpdated Or IsFixerUpdated Is Null"
            + " Or Not IsFixeeUpdated Or IsFixeeUpdated Is Null)"
            + " And Snapshot = ?",
            paramObj.FromDate,
            paramObj.ToDate,
            CashForecastFixTagType.Ignore,
            currentSnapshot));

            return cashFlows;
        }

        public IEnumerable<CashFlow> GetFixees(IEnumerable<CashFlow> cashFlows, CashFlow fixer)
        {
            // we add "fixee.IsFixeeUpdated == false"
            // since one fixee can have many fixers, we avoid
            // running the algorithm twice on the same fixee
            return cashFlows.Where((fixee) => GetFixCriteria(fixee, fixer) && fixee.IsFixeeUpdated == false);
        }

        public IEnumerable<CashFlow> GetFixers(IEnumerable<CashFlow> cashFlows, CashFlow fixee)
        {
            // we don't use IsFixerUpdated condition because a fixee must reference the fixer 
            // regardless of whether the fixer was updated
            return cashFlows.Where((fixer) => GetFixCriteria(fixee, fixer));
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

        private CashFlowSnapshot GetCurrentSnapshot(Session session)
        {
            return CashFlowHelper.GetCurrentSnapshot(session);
        }

    }
}
