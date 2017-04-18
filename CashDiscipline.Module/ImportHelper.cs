using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module
{
    public class ImportHelper
    {
        public Type ParseType(string typeName)
        {
            Type result = null;
            switch (typeName.ToLower())
            {
                case "string":
                    result = typeof(string);
                    break;
                case "int":
                    result = typeof(int);
                    break;
                case "double":
                    result = typeof(double);
                    break;
                case "datetime":
                    result = typeof(DateTime);
                    break;
            }
            return result;
        }
    }
}
