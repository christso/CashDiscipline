using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.Controllers.Forex;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;

namespace CashDiscipline.Module.ControllerHelpers.Cash
{
    public class CashFlowMapper
    {
        private readonly XPObjectSpace objSpace;

        public CashFlowMapper(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }

        public void Process(IEnumerable<CashFlow> cashFlows)
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
            foreach (var mapping in mappings)
            {
                if (cf.Fit(mapping.CriteriaExpression))
                {
                    if (mapping.FixActivity != null)
                        cf.FixActivity = mapping.FixActivity;
                    if (mapping.Fix != null)
                        cf.Fix = mapping.Fix;
                    if (mapping.FixFromDateExpr != null)
                        cf.FixFromDate = (DateTime)cf.Evaluate(CriteriaOperator.Parse(mapping.FixFromDateExpr));
                    if (mapping.FixToDateExpr != null)
                        cf.FixToDate = (DateTime)cf.Evaluate(CriteriaOperator.Parse(mapping.FixToDateExpr));
                    break;
                }
            }
            cf.Save();
        }
    }
}
