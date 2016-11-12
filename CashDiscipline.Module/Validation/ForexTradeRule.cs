using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Persistent.Validation;
using DevExpress.Data.Filtering;
using CashDiscipline.Module.BusinessObjects.Forex;

namespace CashDiscipline.Module.Validation
{
    [CodeRule]
    public class ForexTradeRule : RuleBase<ForexTrade>
    {
        protected ForexTradeRule(string id, ContextIdentifiers targetContextIDs)
            : base(id, targetContextIDs)
        {
            
        }
        protected ForexTradeRule(string id, ContextIdentifiers targetContextIDs, Type targetType)
            : base(id, targetContextIDs, targetType)
        {
            
        }
        public ForexTradeRule()
            : base("", "Save")
        {
            
        }
        public ForexTradeRule(IRuleBaseProperties properties)
            : base(properties)
        {

        }
        protected override bool IsValidInternal(ForexTrade target, out string errorMessageTemplate)
        {
            errorMessageTemplate = string.Empty;
            if (target.PrimaryCcyAmt == 0 || target.Rate == 0) return true;

            decimal expectedRate = target.CounterCcyAmt / target.PrimaryCcyAmt;
            if (Math.Round(target.Rate, 2) == Math.Round(expectedRate, 2))
                return true;
            errorMessageTemplate = string.Format("Rate should equal Counter Ccy Amt divded by Primary Ccy Amt, in this case, it should equal {0}", 
                Math.Round(expectedRate,2));
            return false;
        }
    }
}
