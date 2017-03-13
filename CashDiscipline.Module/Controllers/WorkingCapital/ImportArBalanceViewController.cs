using CashDiscipline.Common;
using CashDiscipline.Module.BusinessObjects.AccountsPayable;
using CashDiscipline.Module.Clients;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DG2NTT.AnalysisServicesHelpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers.WorkingCapital
{
    public class ImportArBalanceViewController : ViewController
    {
        public ImportArBalanceViewController()
        {
            TargetObjectType = typeof(ImportArBalanceParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportArBalanceAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Import";
            importAction.Execute += ImportAction_Execute;

            var reportAction = new SimpleAction(this, "ReportArBalanceAction", PredefinedCategory.ObjectsCreation);
            reportAction.Caption = "Process Report";
            reportAction.Execute += ReportAction_Execute;
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
        ""database"": ""ArGlBalance""
      }
    ]
  }
}");

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                "Process Report Successful",
                "Process Report Successful"
            );
        }

        private void ImportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            string messagesText = string.Empty;

            var paramObj = (ImportArBalanceParam)View.CurrentObject;
            var conn = (SqlConnection)((XPObjectSpace)ObjectSpace).Connection;
            using (var loader = new SqlServerLoader2(conn))
            {
                loader.ColumnMappings.Add("Customer", "Customer");
                loader.ColumnMappings.Add("Customer Number", "Customer Number");
                loader.ColumnMappings.Add("Collector", "Collector");
                loader.ColumnMappings.Add("Outstanding Amount", "Outstanding Amount");
                loader.ColumnMappings.Add("Current", "Current");
                loader.ColumnMappings.Add("1-30 Days", "1-30 Days");
                loader.ColumnMappings.Add("31-60 Days", "31-60 Days");
                loader.ColumnMappings.Add("61-90 Days", "61-90 Days");
                loader.ColumnMappings.Add("91-180 Days", "91-180 Days");
                loader.ColumnMappings.Add("181-360 Days", "181-360 Days");
                loader.ColumnMappings.Add("361+ Days", "361+ Days");
                loader.ColumnMappings.Add("Account", "Account");
                loader.ColumnMappings.Add("Report Date", "Report Date");

                loader.CreateSql = @"CREATE TABLE {TempTable}
(
[Customer] nvarchar(255),
[Customer Number] nvarchar(255),
[Collector] nvarchar(255),
[Outstanding Amount] float,
[Current] float,
[1-30 Days] float,
[31-60 Days] float,
[61-90 Days] float,
[91-180 Days] float,
[181-360 Days] float,
[361+ Days] float,
[Account] nvarchar(255),
[Report Date] datetime
)";
                loader.PersistSql =
    @"INSERT INTO VHAFinance.dbo.ArGlBalance 
(
[Customer],
[Customer Number],
[Collector],
[Outstanding Amount],
[Current],
[1-30 Days],
[31-60 Days],
[61-90 Days],
[91-180 Days],
[181-360 Days],
[361+ Days],
[Account],
[Report Date]
)
SELECT
[Customer],
[Customer Number],
[Collector],
[Outstanding Amount],
[Current],
[1-30 Days],
[31-60 Days],
[61-90 Days],
[91-180 Days],
[181-360 Days],
[361+ Days],
[Account],
[Report Date]
FROM {TempTable}";
                var sourceTable = DataObjectFactory.CreateTableFromExcelXml(paramObj.FilePath, "Upload");

                messagesText = loader.Execute(sourceTable);
            }

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                messagesText,
               "Import Successful"
                );
        }
    }
}
