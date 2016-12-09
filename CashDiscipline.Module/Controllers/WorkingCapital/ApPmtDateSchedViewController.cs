using CashDiscipline.Module.BusinessObjects.AccountsPayable;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers.WorkingCapital
{
    public class ApPmtDateSchedViewController : ViewController
    {
        public ApPmtDateSchedViewController()
        {
            TargetObjectType = typeof(ApPmtDateSched);

            var calcAction = new SimpleAction(this, "ApPmtDateSchedCalcAction", PredefinedCategory.ObjectsCreation);
            calcAction.Caption = "Calculate";
            calcAction.Execute += CalcAction_Execute;
        }

        private void CalcAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            string sql = string.Empty;

            const string connString = CashDiscipline.Common.Constants.FinanceConnString;

            try
            {
                using (var conn = new SqlConnection(connString))
                using (var cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;

                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }
}
