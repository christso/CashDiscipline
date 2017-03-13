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
    public class ImportApPoReceiptBalanceViewController : ViewController
    {
        public ImportApPoReceiptBalanceViewController()
        {
            TargetObjectType = typeof(ImportApPoReceiptBalanceParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportApPoReceiptBalanceAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Import";
            importAction.Execute += ImportAction_Execute;

            var mapAction = new SimpleAction(this, "MapApPoReceiptBalanceAction", PredefinedCategory.ObjectsCreation);
            mapAction.Caption = "Map";
            mapAction.Execute += MapAction_Execute;

            var reportAction = new SimpleAction(this, "ReportApReceiptBalanceAction", PredefinedCategory.ObjectsCreation);
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
        ""database"": ""ApPayables""
      }
    ]
  }
}");

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                "Process Report Successful",
                "Process Report Successful"
            );
        }

        private void MapAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var paramObj = (ImportApPoReceiptBalanceParam)View.CurrentObject;

            Func<string, string> formatSql = delegate (string sql)
            {
                return Smart.Format(sql, new
                {
                    AsAtDate = string.Format("{0:yyyy-MM-dd}", paramObj.AsAtDate.Date),
                    PrevAsAtDate = string.Format("{0:yyyy-MM-dd}", paramObj.PrevAsAtDate.Date)
                });
            };

            string sqlMap =
@"DECLARE @PrevAsAtDate date = '{PrevAsAtDate}'
DECLARE @AsAtDate date = '{AsAtDate}'

/* Map Activity */

exec sp_map_apaccrualpo_account @PrevAsAtDate, @AsAtDate
exec sp_map_apaccrualpo_costcentre @PrevAsAtDate, @AsAtDate
exec sp_map_apaccrualpo_final @PrevAsAtDate, @AsAtDate

/* Match with PO Receipts */

exec sp_apporeceiptmatches_update @PrevAsAtDate, @AsAtDate

/* Retrieve PO Description */

UPDATE ApAccrualPo
SET [Po Description] =
(
	SELECT TOP 1 ApPoReceipt.[Item Description]
	FROM ApPoReceipt
	WHERE ApPoReceipt.[Po Num] = ApAccrualPo.[PoNumber]
	AND ApPoReceipt.[Receipt Date] = 
	(
		SELECT 
			MAX([ApPoReceipt].[Receipt Date])
		FROM [ApPoReceipt]
		WHERE ApPoReceipt.[Po Num] = ApAccrualPo.[PoNumber]
		AND [ApPoReceipt].[Receipt Date] <= [ApAccrualPo].[AsAtDate]
	)
),
[Item Count] = (SELECT COUNT(*) FROM
	(
		SELECT DISTINCT ApPoReceipt.[Item Description]
		FROM ApPoReceipt
		WHERE ApPoReceipt.[Po Num] = ApAccrualPo.[PoNumber]
		AND ApPoReceipt.[Receipt Date] = 
		(
			SELECT 
				MAX([ApPoReceipt].[Receipt Date])
			FROM [ApPoReceipt]
			WHERE ApPoReceipt.[Po Num] = ApAccrualPo.[PoNumber]
			AND [ApPoReceipt].[Receipt Date] <= [ApAccrualPo].[AsAtDate]
		)
	) T1
)
WHERE ApAccrualPo.[AsAtDate] BETWEEN @PrevAsAtDate AND @AsAtDate

/* Retrieve Contract Description */

UPDATE ApAccrualPo SET
[Contract Description] =
(
 SELECT PoContract.[Contract Description]
 FROM PoContract
 WHERE PoContract.[Standard Po Num] = ApAccrualPo.PoNumber
)
WHERE ApAccrualPo.[AsAtDate] BETWEEN @PrevAsAtDate AND @AsAtDate

/* Retrieve Payment Terms */

UPDATE ApAccrualPo
SET [Payment Terms] =
(
	SELECT aph.[Vendor Site Payment Terms]
	FROM ApPoHeader aph
	WHERE aph.[Po Num] = ApAccrualPo.PoNumber
)
WHERE ApAccrualPo.[AsAtDate] BETWEEN @PrevAsAtDate AND @AsAtDate
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
            var importer = new ApPoReceiptBalanceImporter((XPObjectSpace)ObjectSpace);
            var paramObj = (ImportApPoReceiptBalanceParam)View.CurrentObject;
            var messagesText = importer.Execute(paramObj);

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                messagesText,
               "Import Successful"
                );
        }
    }
}
