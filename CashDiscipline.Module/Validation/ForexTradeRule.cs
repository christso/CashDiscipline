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
    //[CodeRule]
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
            decimal expectedRate = target.CounterCcyAmt / target.PrimaryCcyAmt;
            errorMessageTemplate = "";
            if (Math.Round(target.Rate, 4) == Math.Round(expectedRate, 4))
                return true;
            errorMessageTemplate = string.Format("Rate should equal Counter Ccy Amt divded by Primary Ccy Amt, in this case, it should equal {0}", expectedRate);
            return false;
        }
    }
}
