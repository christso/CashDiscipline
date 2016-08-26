using CashDiscipline.Module.BusinessObjects.FinAccounting;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic
{
    public class SqlProcessorUtil
    {
        public SqlProcessorUtil(Session session)
        {
            this.session = session;
        }
        private Session session;
        public List<SqlParameter> CreateParameters(List<SqlDeclareClause> clauses)
        {
            var parameters = new List<SqlParameter>();
            using (var cmd = session.Connection.CreateCommand())
            {
                foreach (var clause in clauses)
                {
                    parameters.Add(new SqlParameter(clause.ParameterName, clause.ExecuteScalar(cmd)));
                }
            }
            return parameters;
        }

        public Guid GetServerSequentialGuid()
        {
            using (var cmd = ((SqlConnection)session.Connection).CreateCommand())
            {
                // get GUID for snapshot
                cmd.CommandText = "SELECT CAST(CAST(NEWID() AS BINARY(10)) + CAST(GETDATE() AS BINARY(6)) AS UNIQUEIDENTIFIER)";
                var newGuid = (Guid)cmd.ExecuteScalar();
                return newGuid;
            }
        }
    }
}
