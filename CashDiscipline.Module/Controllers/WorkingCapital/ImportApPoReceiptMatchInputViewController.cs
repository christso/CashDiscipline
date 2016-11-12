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
using CashDiscipline.Module.Clients;

namespace CashDiscipline.Module.Controllers.WorkingCapital
{
    public class ImportApPoReceiptMatchInputViewController : ViewController
    {
        public ImportApPoReceiptMatchInputViewController()
        {
            TargetObjectType = typeof(ImportApPoReceiptMatchInputParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportApPoReceiptMatchInputAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Import";
            importAction.Execute += ImportAction_Execute;

        }

        private void ImportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

            var messagesText = "IMPORT DAYS : \r\n" + ImportDays();
            messagesText += "\r\nIMPORT DATES : \r\n" + ImportDates();

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                messagesText,
               "Import Successful"
                );
        }

        private string ImportDays()
        {
            var paramObj = (ImportApPoReceiptMatchInputParam)View.CurrentObject;
            var conn = (SqlConnection)((XPObjectSpace)ObjectSpace).Connection;
            var loader = new SqlServerLoader(conn);

            loader.CreateSql = @"CREATE TABLE {TempTable}
(
    PoNum nvarchar(255),
    Vendor nvarchar(255),
    ForecastVendorMatchDays float,
    ForecastMatchDays float
)";
            loader.PersistSql = @"DELETE FROM VHAFinance.dbo.ApPoMatchInput
INSERT INTO VHAFinance.dbo.ApPoMatchInput (PoNum, ForecastMatchDays)
SELECT 
    PoNum,
    ForecastMatchDays
FROM {TempTable}";

            var sourceTable = DataObjectFactory.CreateTableFromExcelXml(paramObj.FilePath, "PoMatchDaysInput");
            return loader.Execute(sourceTable);
        }

        private string ImportDates()
        {
            var paramObj = (ImportApPoReceiptMatchInputParam)View.CurrentObject;

            var conn = (SqlConnection)((XPObjectSpace)ObjectSpace).Connection;
            var loader = new SqlServerLoader(conn);
            loader.CreateSql = @"CREATE TABLE {TempTable}
(
    PoNum nvarchar(255),
    ForecastMatchDate date
)";
            loader.PersistSql = @"DELETE FROM VHAFinance.dbo.ApPoMatchDateInput
INSERT INTO VHAFinance.dbo.ApPoMatchDateInput (PoNum, ForecastMatchDate)
SELECT PoNum, ForecastMatchDate FROM {TempTable}";

            var sourceTable = DataObjectFactory.CreateTableFromExcelXml(paramObj.FilePath, "ManualMatchDate");
            return loader.Execute(sourceTable);
        }
    }
}
