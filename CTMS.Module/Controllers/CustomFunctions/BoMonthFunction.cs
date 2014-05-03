using DevExpress.Data.Filtering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.CustomFunctions
{
    public class BoMonthFunction : ICustomFunctionOperatorFormattable
    {
        string ICustomFunctionOperatorFormattable.Format(Type providerType, params string[] operands)
        {
            return operands[0];
        }

        object ICustomFunctionOperator.Evaluate(params object[] operands)
        {
            var dt = (DateTime)operands[0];
            return HelperClasses.StaticHelpers.DateObject(dt.Year, dt.Month, 1);
        }

        string ICustomFunctionOperator.Name
        {
            get { return "BoMonth"; }
        }

        Type ICustomFunctionOperator.ResultType(params Type[] operands)
        {
            return typeof(DateTime);
        }
    }
}
