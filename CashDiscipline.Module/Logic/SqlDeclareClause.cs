using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic
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

        public object ExecuteScalar(XPObjectSpace objSpace)
        {
            var conn = objSpace.Session.Connection;
            using (var cmd = conn.CreateCommand())
            {
                return ExecuteScalar(cmd);
            }
        }

        public object ExecuteScalar(IDbCommand cmd)
        {
            cmd.CommandText = "SELECT " + this.CommandText;
            var sqlValue = cmd.ExecuteScalar();
            return sqlValue;
        }
    }
}
