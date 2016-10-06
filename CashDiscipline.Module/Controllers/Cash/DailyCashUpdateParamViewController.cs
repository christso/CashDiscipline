using SmartFormat;
using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using CashDiscipline.Module.Logic;
using System.Data.SqlClient;
using CashDiscipline.Module.Logic.Cash;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class DailyCashUpdateParamViewController : ViewController
    {
        public DailyCashUpdateParamViewController()
        {
            TargetObjectType = typeof(DailyCashUpdateParam);
            TargetViewType = ViewType.DetailView;

            var runAction = new SimpleAction(this, "DailyCashUpdateRunAction", PredefinedCategory.ObjectsCreation);
            runAction.Caption = "Run";
            runAction.Execute += RunAction_Execute;

        }

        private void RunAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        { 

            ObjectSpace.CommitChanges(); // update parameters in data source
            ProcessWithValidation();
        }

        public void ProcessWithValidation()
        {
            var paramObj = (DailyCashUpdateParam)View.CurrentObject;
            var objSpace = (XPObjectSpace)Application.CreateObjectSpace();

            // check that actual data exists. If not, then check if user still wants to continue;

            var conn = (SqlConnection)objSpace.Connection;
            using (var cmd = conn.CreateCommand())
            {

                #region Validate Actuals Exist for the [To Date] parameter

                cmd.CommandText = Smart.Format(
@"DECLARE @ActualStatus int = {actualStatus}
DECLARE @ToDate datetime = (SELECT TOP 1 ToDate FROM DailyCashUpdateParam WHERE GCRecord IS NULL)
DECLARE @Snapshot uniqueidentifier = (SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks WHERE GCRecord IS NULL)

DECLARE @ApPmtDistnCount int = (SELECT COUNT(*) FROM ApPmtDistn p1
WHERE p1.PaymentDate = @ToDate
AND p1.GCRecord IS NULL)

DECLARE @BankStmtCount int = (SELECT COUNT(*) FROM BankStmt bs
WHERE bs.TranDate = @ToDate
AND bs.GCRecord IS NULL)

SELECT @ApPmtDistnCount + @BankStmtCount", new { actualStatus = Convert.ToInt32(CashFlowStatus.Actual) });

                bool exitFlag = false;

                var toDateCount = Convert.ToInt64(cmd.ExecuteScalar());
                if (toDateCount == 0)
                {
                    new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                        string.Format("No data exists in BankStmt and ApPmtDistn to upload into Actual for date {0:dd-MMM-yy}.",
                            paramObj.ToDate)
                        + " Do you wish to continue?", "WARNING",
                        (s1, e1) => { return; },
                        (s1, e1) => {
                            exitFlag = true;
                            return; });
                }

                if (exitFlag) return;

                #endregion

                #region Validate that date selected is less than today's date
                // This validation is required as we should not be running this for a forecast period
                if (paramObj.FromDate >= DateTime.Today)
                {
                    new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                        string.Format("The 'From Date' that you have selected is {0:dd-MMM-yy}, which is after yesterday's date. "
                            + " Do you wish to continue?",
                            paramObj.FromDate)
                        , "WARNING",
                        (s1, e1) => { return; },
                        (s1, e1) => {
                            exitFlag = true;
                            return; });
                }

                if (exitFlag) return;

                Process();
                #endregion
            }
        }

        public void Process()
        {
            var paramObj = (DailyCashUpdateParam)View.CurrentObject;
            var objSpace = (XPObjectSpace)Application.CreateObjectSpace();

            // process

            var bsUploader = new CashDiscipline.Module.Logic.Cash.BankStmtToCashFlowAlgorithm(objSpace, paramObj);
            bsUploader.Process();

            var apUploader = new CashDiscipline.Module.Logic.Cash.ApPmtToCashFlowAlgorithm(objSpace);
            apUploader.Process();

            using (var cmd = ((SqlConnection)objSpace.Session.Connection).CreateCommand())
            {
 
                //Delete Forex Settle Links
                cmd.CommandText = @"UPDATE ForexSettleLink SET
                GCRecord = CAST(RAND() * 2147483646 + 1 AS INT),
CashFlowIn = NULL,
CashFlowOut = NULL,
Account = NULL
WHERE EXISTS (SELECT * FROM CashFlow cf WHERE cf.GCRecord IS NOT NULL AND cf.Oid = CashFlowIn)
OR EXISTS (SELECT * FROM CashFlow cf WHERE cf.GCRecord IS NOT NULL AND cf.Oid = CashFlowOUt)
AND GCRecord IS NULL";
                cmd.ExecuteNonQuery();

                // unfix
                cmd.CommandText = @"DECLARE @Snapshot uniqueidentifier =
	(SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks WHERE GCRecord IS NULL)"
+ "\n\n"
+ FixCashFlowsAlgorithm.ResetCommandText;
                cmd.ExecuteNonQuery();
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
        }
    }
}
