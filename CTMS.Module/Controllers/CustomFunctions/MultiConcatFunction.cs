using System;
using DevExpress.Data.Filtering;
using DevExpress.Xpo.DB;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace CTMS.Module.CustomFunctions
{
    public class MultiConcatFunction : ICustomFunctionOperatorFormattable 
    {
        string ICustomFunctionOperatorFormattable.Format(Type providerType, params string[] operands)
        {
            return operands[0];
        }

        //public string MultiConcat(string separator, string[] values)
        //{

        //}

        object ICustomFunctionOperator.Evaluate(params object[] operands)
        {
            List<string> valuesList = new List<string>();
            for (int i = 1; i < operands.Length; i++)
            {
                object o = operands[i];
                if (o != null)
                {
                    valuesList.Add((string)o);
                }
            }
            string separator = Convert.ToString(operands[0]);
            return string.Join(separator, valuesList.ToArray<string>());
        }

        string ICustomFunctionOperator.Name
        {
            get { return "MultiConcat"; }
        }

        Type ICustomFunctionOperator.ResultType(params Type[] operands)
        {
            return typeof(string);
        }
    }
}
