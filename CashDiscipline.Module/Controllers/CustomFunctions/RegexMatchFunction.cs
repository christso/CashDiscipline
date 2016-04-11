using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.Xpo.DB;
using System.Text.RegularExpressions;

namespace CashDiscipline.Module.CustomFunctions
{
    public class RegexMatchFunction : ICustomFunctionOperatorFormattable 
    {
        string ICustomFunctionOperatorFormattable.Format(Type providerType, params string[] operands)
        {
            return operands[0];
        }

        // REGEXMATCH(PATTERN, TEXT)
        object ICustomFunctionOperator.Evaluate(params object[] operands)
        {
            string pattern = (string)operands[0];
            string text = (string)operands[1];
            Regex rx = new Regex(pattern);
            var isMatch = rx.IsMatch(text);
            return isMatch;
        }

        string ICustomFunctionOperator.Name
        {
            get { return "RegexMatch"; }
        }

        Type ICustomFunctionOperator.ResultType(params Type[] operands)
        {
            return typeof(int);
        }
    }
}
