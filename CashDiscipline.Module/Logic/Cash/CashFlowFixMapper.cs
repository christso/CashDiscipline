using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.Controllers.Forex;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CashDiscipline.Module.Logic.Cash
{
    public class CashFlowFixMapper
    {
        private readonly XPObjectSpace objSpace;

        public CashFlowFixMapper(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }

        public void Process(IEnumerable cashFlows)
        {
            var mappings = objSpace.GetObjects<CashFlowFixMapping>();

            foreach (CashFlow cf in cashFlows)
            {
                Process(mappings, cf);
            }
            objSpace.CommitChanges();
        }

        public void Process(IEnumerable<CashFlowFixMapping> mappings, CashFlow cf)
        {
            var steps = mappings.GroupBy(m => new { m.MapStep })
                .OrderBy(g => g.Key.MapStep)
                .Select(g => g.Key.MapStep)
                .ToList<int>();

            foreach (int step in steps)
            {
                ProcessFixActivity(mappings, cf, step);
                ProcessFix(mappings, cf, step);
                ProcessFixFromDateExpr(mappings, cf, step);
                ProcessFixToDateExpr(mappings, cf, step);
            }
            cf.Save();
        }

        public void ProcessFixActivity(IEnumerable<CashFlowFixMapping> mappings, CashFlow cf, int step)
        {
            var map = mappings.Where(m =>
                m.FixActivity != null
                && m.MapStep == step
                && (cf.FixActivity == null)
                && cf.Fit(m.CriteriaExpression)
                ).FirstOrDefault();

            if (map != null)
                cf.FixActivity = map.FixActivity;
        }

        public void ProcessFix(IEnumerable<CashFlowFixMapping> mappings, CashFlow cf, int step)
        {
            var map = mappings.Where(m =>
                m.Fix != null
                && m.MapStep == step
                && (cf.Fix == null)
                && cf.Fit(m.CriteriaExpression)).FirstOrDefault();

            if (map != null)
                cf.Fix = map.Fix;
        }

        public void ProcessFixFromDateExpr(IEnumerable<CashFlowFixMapping> mappings, CashFlow cf, int step)
        {
            var map = mappings.Where(m =>
                !string.IsNullOrWhiteSpace(m.FixFromDateExpr)
                && m.MapStep == step
                && (cf.FixFromDate == default(DateTime))
                && cf.Fit(m.CriteriaExpression)).FirstOrDefault();

            if (map != null)
                cf.FixFromDate = (DateTime)cf.Evaluate(CriteriaOperator.Parse(map.FixFromDateExpr));
        }

        public void ProcessFixToDateExpr(IEnumerable<CashFlowFixMapping> mappings, CashFlow cf, int step)
        {
            var map = mappings.Where(m =>
                !string.IsNullOrWhiteSpace(m.FixToDateExpr)
                && m.MapStep == step
                && (cf.FixToDate == default(DateTime))
                && cf.Fit(m.CriteriaExpression))
                .FirstOrDefault();

            if (map != null)
                cf.FixToDate = (DateTime)cf.Evaluate(CriteriaOperator.Parse(map.FixToDateExpr));
        }
    }
}
