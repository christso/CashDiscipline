using CashDiscipline.Module.ParamObjects.FinAccounting;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.FinAccounting
{
    public class SqlJournalGenerator
    {
        private readonly FinGenJournalParam paramObj;
        private readonly XPObjectSpace objSpace;
        public SqlJournalGenerator(FinGenJournalParam paramObj, XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;
        }

        public List<SqlParameter> CreateParameters()
        {
            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("FromDate", paramObj.FromDate),
                new SqlParameter("ToDate", paramObj.ToDate)
            };
            return parameters;
        }
        public void Execute()
        {
            var parameters = CreateParameters();

            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            command.Parameters.AddRange(parameters.ToArray());

            //command.ExecuteReader(System.Data.CommandBehavior.SequentialAccess);

        }
    }
}
