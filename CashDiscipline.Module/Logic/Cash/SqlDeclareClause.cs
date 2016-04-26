using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Cash
{
    public class SqlDeclareClause
    {
        public SqlDeclareClause(string variableName, string dataType, string commandText)
        {
            this.ParameterName = variableName;
            this.DataType = dataType;
            this.CommandText = commandText;
        }

        public string ParameterName { get; set; }
        public string DataType { get; set; }
        public string CommandText { get; set; }
    }
}
