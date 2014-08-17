using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Persistent.Validation;
using DevExpress.Data.Filtering;
using CTMS.Module.BusinessObjects.Forex;

namespace CTMS.Module.Validation
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
            decimal expectedValue = Math.Round(target.CounterCcyAmt / target.Rate, 2);
            errorMessageTemplate = "";
            if (expectedValue == Math.Round(target.PrimaryCcyAmt, 2))
                return true;
            errorMessageTemplate = string.Format("Primary Ccy Amt should equal to Counter Ccy Amt divided by Rate, in this case, it should equal {0}", expectedValue);
            return false;
        }
    }
}
