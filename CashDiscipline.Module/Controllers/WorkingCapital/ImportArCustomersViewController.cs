using CashDiscipline.Module.BusinessObjects.AccountsPayable;
using CashDiscipline.Module.Clients;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers.WorkingCapital
{
    public class ImportArCustomersViewController : ViewController
    {
        public ImportArCustomersViewController()
        {
            TargetObjectType = typeof(ImportArCustomersParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportArCustomersAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Import";
            importAction.Execute += ImportAction_Execute;
        }

        private void ImportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

            var paramObj = (ImportArCustomersParam)View.CurrentObject;
            var conn = (SqlConnection)((XPObjectSpace)ObjectSpace).Connection;
            var loader = new SqlServerLoader(conn);
            loader.CreateSql = @"CREATE TABLE {TempTable}
(
    CustomerNumber nvarchar(255),
    CustomerName nvarchar(255),
    Collector nvarchar(255),
    Activity nvarchar(255)
)";
            loader.PersistSql = @"DELETE FROM VHAFinance.dbo.ArCustomers
INSERT INTO VHAFinance.dbo.ArCustomers
SELECT
CustomerNumber,
CustomerName,
Collector,
Activity
FROM {TempTable}";
            var sourceTable = DataObjectFactory.CreateTableFromExcelXml(paramObj.FilePath, "ArCustomers");

            var messagesText = loader.Execute(sourceTable);

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                messagesText,
               "Import Successful"
                );
        }
    }
}
