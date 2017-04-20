using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.SystemModule;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class AccountBalanceParamViewController : ViewController
    {
        private const string calcBalCaption = "Calculate";

        public AccountBalanceParamViewController()
        {
            TargetObjectType = typeof(AccountBalanceParam);

            var calcAction = new SimpleAction(this, "AccountBalanceParamAction", DevExpress.Persistent.Base.PredefinedCategory.Edit);
            calcAction.Caption = "Calculate";
            calcAction.Execute += CalcAction_Execute;
        }

        private void CalcAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            ObjectSpace.CommitChanges();

            var objSpace = (XPObjectSpace)ObjectSpace;
            var conn = (SqlConnection)objSpace.Session.Connection;
            using (var command = conn.CreateCommand())
            {
                command.CommandText =
@"DECLARE @FromDate datetime = (SELECT TOP 1 FromDate FROM AccountBalanceParam WHERE GCRecord IS NULL);
DECLARE @ToDate datetime = (SELECT TOP 1 ToDate FROM AccountBalanceParam WHERE GCRecord IS NULL);
DECLARE @Snapshot uniqueidentifier = (SELECT TOP 1 CurrentCashFlowSnapshot FROM SetOfBooks WHERE GCRecord IS NULL);";
                command.ExecuteNonQuery();
            }

            new GenericMessageBox(
                "ACTION COMPLETED : CALCULATE",
                "ACTION COMPLETED");
        }
    }
}
