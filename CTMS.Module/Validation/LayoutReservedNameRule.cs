using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Persistent.Validation;
using DevExpress.Data.Filtering;
using CTMS.Module.BusinessObjects.User;

namespace CTMS.Module.Validation
{
    [CodeRule]
    public class LayoutReservedNameRule : RuleBase<UserViewLayout>
    {
        protected LayoutReservedNameRule(string id, ContextIdentifiers targetContextIDs)
            : base(id, targetContextIDs)
        {

        }
        protected LayoutReservedNameRule(string id, ContextIdentifiers targetContextIDs, Type targetType)
            : base(id, targetContextIDs, targetType)
        {

        }
        public LayoutReservedNameRule()
            : base("", "Save")
        {

        }
        public LayoutReservedNameRule(IRuleBaseProperties properties)
            : base(properties)
        {

        }
        protected override bool IsValidInternal(UserViewLayout target, out string errorMessageTemplate)
        {
            errorMessageTemplate = "";
            if (UserViewLayout.IsCashFlowPivotLayout(target.LayoutName))
            {
                errorMessageTemplate = "Layout name '" + target.LayoutName + "' is reserved by the system. "
                    + "Please choose another name.";
                return false;
            }
            return true;
        }
    }
}
