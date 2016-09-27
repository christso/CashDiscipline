using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.SqlServer
{
    public class SqlQueryUtil
    {
        public SqlQueryUtil(Session session)
        {
            this.session = session;
        }
        private Session session;

        private bool IsSqlServerSession
        {
            get
            {
                var conn = session.Connection as SqlConnection;
                if (conn == null) return false;
                return true;
            }
        }

        public DateTime GetDate()
        {
            if (!IsSqlServerSession)
                return DateTime.Now;

            var dateTime = Convert.ToDateTime(session.ExecuteScalar("SELECT GETDATE()"));
            return dateTime;
        }
    }
}
