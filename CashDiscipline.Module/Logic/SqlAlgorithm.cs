using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic
{
    public class SqlAlgorithm
    {
        public SqlAlgorithm(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }

        private XPObjectSpace objSpace;

        public string CommandText { get; set; }

        public void Process()
        {

            using (var cmd = ((SqlConnection)objSpace.Session.Connection).CreateCommand())
            {
                cmd.CommandText = CommandText;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
