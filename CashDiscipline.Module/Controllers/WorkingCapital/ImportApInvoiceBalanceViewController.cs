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

namespace CashDiscipline.Module.Controllers.Cash
{
    public class ImportApInvoiceBalanceViewController : ViewController
    {
        public ImportApInvoiceBalanceViewController()
        {
            TargetObjectType = typeof(ImportApInvoiceBalanceParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportApInvoiceBalanceAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Import";
            importAction.Execute += ImportAction_Execute;

            var mapAction = new SimpleAction(this, "MapApInvoiceBalanceAction", PredefinedCategory.ObjectsCreation);
            mapAction.Caption = "Map";
            mapAction.Execute += MapAction_Execute;

            var reportAction = new SimpleAction(this, "ReportApInvoiceBalanceAction", PredefinedCategory.ObjectsCreation);
            reportAction.Caption = "Process Report";
            reportAction.Execute += ReportAction_Execute;
        }

        private void ReportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            
            string serverName = Constants.SsasServerName;
            var processor = new ServerProcessor(serverName, "ApPayables");
            processor.ProcessDatabase();

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                "Process Report Successful",
                "Process Report Successful"
            );
        }

        private void MapAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var paramObj = (ImportApInvoiceBalanceParam)View.CurrentObject;

            Func<string, string> formatSql = delegate(string sql)
            {
                return Smart.Format(sql, new
                {
                    FromDate = string.Format("{0:yyyy-MM-dd}", paramObj.FromDate.Date),
                    ToDate = string.Format("{0:yyyy-MM-dd}", paramObj.ToDate.Date)
                });
            };

            string sqlVendor = @"INSERT INTO Vendors
SELECT DISTINCT T1.[Supplier] FROM ApTradeCreditor T1
WHERE T1.AsAtDate BETWEEN '{FromDate}' AND '{ToDate}'
AND T1.[Supplier] NOT IN (SELECT Vendor FROM Vendors)";

            string sqlMapActivity = @"exec sp_map_apcreditor_account '{FromDate}', '{ToDate}'
exec sp_map_apcreditor_costcentre '{FromDate}', '{ToDate}'
exec sp_map_apcreditor_final '{FromDate}', '{ToDate}'";

            string sqlInvoices = @"INSERT INTO ApInvoices
SELECT DISTINCT T1.InvoiceNumber, T1.InvoiceDate
FROM ApTradeCreditor T1
WHERE T1.InvoiceNumber NOT IN (SELECT ApInvoices.InvoiceNum FROM ApInvoices)
AND T1.InvoiceNumber IS NOT NULL
AND T1.AsAtDate BETWEEN '{FromDate}' AND '{ToDate}'";

            string sqlInvoiceReceivedDate = @"UPDATE ApTradeCreditor SET 
InvoiceReceivedDate =
(
	SELECT MAX(T1.[Invoice Received Date])
	FROM ApInvoiceHeader T1
	WHERE T1.[Vendor Name] = ApTradeCreditor.Supplier
		AND T1.[Invoice Num] = ApTradeCreditor.InvoiceNumber
)
WHERE ApTradeCreditor.AsAtDate BETWEEN '{FromDate}' AND '{ToDate}'";

            const string connString = CashDiscipline.Common.Constants.FinanceConnString;

            try
            {
                using (var conn = new SqlConnection(connString))
                using (var cmd = new SqlCommand())
                {
                    conn.Open();
                    cmd.Connection = conn;

                    cmd.CommandText = formatSql(sqlVendor);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = formatSql(sqlMapActivity);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = formatSql(sqlInvoices);
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = formatSql(sqlInvoiceReceivedDate);
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


        private void ImportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                Import();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void Import()
        {
            var paramObj = (ImportApInvoiceBalanceParam)View.CurrentObject;

            var objSpace = (XPObjectSpace)ObjectSpace;
            var importer = new ApInvoiceBalanceImporter(objSpace);

            var messagesText = importer.Execute(paramObj.FilePath);
            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                messagesText,
               "Import Successful"
                );
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
        }
    }
}
