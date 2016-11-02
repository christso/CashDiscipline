using CashDiscipline.Module.Logic.Cash;
using CashDiscipline.Module.Logic.Import;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFormat;
using CashDiscipline.Common;
using DG2NTT.AnalysisServicesHelpers;
using CashDiscipline.Module.ParamObjects;

namespace CashDiscipline.Module.Controllers.WorkingCapital
{
    public class ImportApPoReceiptViewController : ViewController
    {
        public ImportApPoReceiptViewController()
        {
            TargetObjectType = typeof(ImportApPoReceiptParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportApPoReceiptAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Import";
            importAction.Execute += ImportAction_Execute;

            var mapAction = new SimpleAction(this, "MapApPoReceiptAction", PredefinedCategory.ObjectsCreation);
            mapAction.Caption = "Map";
            mapAction.Execute += MapAction_Execute;

            var alterAction = new SimpleAction(this, "alterApPoReceiptAction", PredefinedCategory.ObjectsCreation);
            alterAction.Caption = "Alter";
            alterAction.Execute += alterAction_Execute;

            var reportAction = new SimpleAction(this, "ReportApPoReceiptAction", PredefinedCategory.ObjectsCreation);
            reportAction.Caption = "Process Report";
            reportAction.Execute += ReportAction_Execute;
        }

        private void alterAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var paramObj = (ImportApPoReceiptParam)View.CurrentObject;

            string sqlAlter = @"ALTER VIEW [dbo].[ApPoReceipt] AS SELECT * FROM VHAFinance.dbo.vw_ApPoReceipt";

            const string connString = CashDiscipline.Common.Constants.SqlConnectionString;

            try
            {
                using (var conn = new SqlConnection(connString))
                using (var cmd = new SqlCommand())
                {
                    cmd.CommandTimeout = CashDiscipline.Common.Constants.SqlCommandTimeout;

                    conn.Open();
                    cmd.Connection = conn;

                    cmd.CommandText = sqlAlter;
                    cmd.ExecuteNonQuery();
                }

                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    "Alter Successful",
                   "Alter Successful"
                    );
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void ImportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var importer = new ApPoReceiptImporter((XPObjectSpace)ObjectSpace);
            var paramObj = (ImportApPoReceiptParam)View.CurrentObject;
            var messagesText = importer.Execute(paramObj);

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                messagesText,
               "Import Successful"
                );
        }

        private void MapAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var paramObj = (ImportApPoReceiptParam)View.CurrentObject;

            Func<string, string> formatSql = delegate (string sql)
            {
                return Smart.Format(sql, new
                {
                    FromDate = string.Format("{0:yyyy-MM-dd}", paramObj.FromDate.Date),
                    ToDate = string.Format("{0:yyyy-MM-dd}", paramObj.ToDate.Date)
                });
            };

            string sqlMap = @"DECLARE @FromDate date = '{FromDate}'
DECLARE @ToDate date = '{ToDate}'
exec sp_apporeceipt_update @FromDate, @ToDate
exec sp_map_apporeceipt_account @FromDate, @ToDate
exec sp_map_apporeceipt_costcentre @FromDate, @ToDate
exec sp_map_apporeceipt_final @FromDate, @ToDate
";

            const string connString = CashDiscipline.Common.Constants.FinanceConnString;

            try
            {
                using (var conn = new SqlConnection(connString))
                using (var cmd = new SqlCommand())
                {
                    cmd.CommandTimeout = CashDiscipline.Common.Constants.SqlCommandTimeout;

                    conn.Open();
                    cmd.Connection = conn;

                    cmd.CommandText = formatSql(sqlMap);
                    cmd.ExecuteNonQuery();
                }

                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    "Mapping Successful",
                   "Mapping Successful"
                    );
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void ReportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

            string serverName = Constants.SsasServerName;
            var processor = new AdomdProcessor(serverName);
            processor.ProcessCommand(@"{
  ""refresh"": {
    ""type"": ""full"",
    ""objects"": [
      {
        ""database"": ""ApPoReceipts""
      }
    ]
  }
}");

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                "Process Report Successful",
                "Process Report Successful"
            );
        }

    }
}
