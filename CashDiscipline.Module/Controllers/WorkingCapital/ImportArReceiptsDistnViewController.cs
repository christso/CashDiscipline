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
    public class ImportArReceiptsDistnViewController : ViewController
    {
        public ImportArReceiptsDistnViewController()
        {
            TargetObjectType = typeof(ImportArReceiptDistnParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportArReceiptAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Import";
            importAction.Execute += ImportAction_Execute;

            var mapAction = new SimpleAction(this, "MapArReceiptAction", PredefinedCategory.ObjectsCreation);
            mapAction.Caption = "Map";
            mapAction.Execute += mapAction_Execute;
        }

        private void mapAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var paramObj = (ImportArReceiptDistnParam)View.CurrentObject;

            Func<string, string> formatSql = delegate (string sql)
            {
                return Smart.Format(sql, new
                {
                    FromDate = string.Format("{0:yyyy-MM-dd}", paramObj.FromDate.Date),
                    ToDate = string.Format("{0:yyyy-MM-dd}", paramObj.ToDate.Date)
                });
            };

            string sqlMap = @"exec sp_map_arreceipt_account '{FromDate}', '{ToDate}'
exec sp_map_arreceipt_customer '{FromDate}', '{ToDate}'
exec sp_map_arreceipt_final '{FromDate}', '{ToDate}'
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

        private void ImportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var importer = new ArReceiptDistnImporter((XPObjectSpace)ObjectSpace);
            var paramObj = (ImportArReceiptDistnParam)View.CurrentObject;
            var messagesText = importer.Execute(paramObj);

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                messagesText,
               "Import Successful"
                );
        }
    }
}
