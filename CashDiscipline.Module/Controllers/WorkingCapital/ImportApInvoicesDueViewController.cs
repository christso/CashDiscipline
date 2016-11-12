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
    public class ImportApInvoicesDueViewController : ViewController
    {
        public ImportApInvoicesDueViewController()
        {
            TargetObjectType = typeof(ImportApInvoicesDueParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportApInvoicesDueAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Import";
            importAction.Execute += ImportAction_Execute;
        }

        private void ImportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

            var paramObj = (ImportApInvoicesDueParam)View.CurrentObject;
            var conn = (SqlConnection)((XPObjectSpace)ObjectSpace).Connection;
            var loader = new ExcelXmlToSqlServerLoader(conn);
            loader.ExcelFilePath = paramObj.FilePath;
            loader.CreateSql = @"CREATE TABLE {TempTable}
(
    Supplier nvarchar(255),
    InvoiceNumber nvarchar(255),
    InvoiceDueDate date
)";
            loader.PersistSql = @"DELETE FROM VHAFinance.dbo.ApInvoicesDueInput
INSERT INTO VHAFinance.dbo.ApInvoicesDueInput
SELECT
Supplier,
InvoiceNumber,
InvoiceDueDate
FROM {TempTable}";
            loader.ExcelSheetName = "ApInvoicesDueInput";

            var messagesText = loader.Execute();

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                messagesText,
               "Import Successful"
                );
        }
    }
}
