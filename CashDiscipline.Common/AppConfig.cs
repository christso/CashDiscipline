using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Common
{
    public class AppConfig
    {
        public static string SsasConnectionString
        {
            get
            {
                return string.Format(
                  "Integrated Security=SSPI;Persist Security Info=True;Initial Catalog={1};Data Source={0};",
                  Constants.SsasServerName, Constants.SsasDatabase);
            }
        }
    }
}
