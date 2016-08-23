using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.DatabaseUpdate
{
    public class DbUtils
    {
        public static int ExecuteNonQueryCommand(XPObjectSpace os, string commandText, bool silent)
        {
            var conn = (SqlConnection)os.Session.Connection;
            if (conn == null) return -1;

            Tracing.Tracer.LogText("ExecuteNonQueryCommand: '{0}', silent={1}", new object[]
            {
                commandText,
                silent
            });

            int result;
            try
            {
                Server server = new Server(new ServerConnection(conn));
                result = server.ConnectionContext.ExecuteNonQuery(commandText);
            }
            catch (Exception exception)
            {
                Tracing.Tracer.LogError(exception);
                if (!silent)
                {
                    throw;
                }
                result = -1;
            }
            return result;
        }

    }
}
