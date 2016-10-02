using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFormat;

namespace CashDiscipline.Module.Logic
{
    public class SqlStringUtil
    {
        public string CreateCommandText(List<SqlDeclareClause> clauses)
        {
            string result = string.Empty;
            foreach (var clause in clauses)
            {
                if (result != string.Empty)
                    result += "\n";
                result += string.Format("DECLARE @{0} {1} = {2}",
                    clause.ParameterName, clause.DataType, clause.CommandText);
            }
            return result;
        }
    }
}
