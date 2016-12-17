using CashDiscipline.Module.Clients;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using SmartFormat;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Controllers.FinAccounting
{
    public class ImportTrialBalanceViewController : ViewController
    {
        public ImportTrialBalanceViewController()
        {
            TargetObjectType = typeof(ImportTrialBalanceParam);

            var importAction = new SimpleAction(this, "ImportTrialBalanceAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Import";
            importAction.Execute += ImportAction_Execute;
        }

        private void ImportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            string messagesText = string.Empty;

            var paramObj = (ImportTrialBalanceParam)View.CurrentObject;

            var conn = (SqlConnection)((XPObjectSpace)ObjectSpace).Connection;
            using (var loader = new SqlServerLoader2(conn))
            {
                loader.SqlStringReplacers.Add("FromDate", string.Format("{0:yyyy-MM-dd}", paramObj.FromDate.Date));
                loader.SqlStringReplacers.Add("ToDate", string.Format("{0:yyyy-MM-dd}", paramObj.ToDate.Date));

                loader.ColumnMappings.Add("Company", "Company");
                loader.ColumnMappings.Add("Account", "Account");
                loader.ColumnMappings.Add("BS or P&L", "BS or PL");
                loader.ColumnMappings.Add("Account Class", "Account Class");
                loader.ColumnMappings.Add("Account Desc", "Account Desc");
                loader.ColumnMappings.Add("Period Name", "Period Name");
                loader.ColumnMappings.Add("AUD Beg Bal", "AUD Beg Bal");
                loader.ColumnMappings.Add("AUD PTD Dr", "AUD PTD Dr");
                loader.ColumnMappings.Add("AUD PTD Cr", "AUD PTD Cr");
                loader.ColumnMappings.Add("AUD PTD Net", "AUD PTD Net");
                loader.ColumnMappings.Add("AUD End Bal", "AUD End Bal");

                loader.CreateSql = @"CREATE TABLE {TempTable}
(
[Company] [nvarchar](10) NULL,
[Account] [nvarchar](10) NULL,
[BS or PL] [nvarchar](10) NULL,
[Account Class] [nvarchar](50) NULL,
[Account Desc] [nvarchar](255) NULL,
[Period Name] [nvarchar](10) NULL,
[AUD Beg Bal] [money] NULL,
[AUD PTD Dr] [money] NULL,
[AUD PTD Cr] [money] NULL,
[AUD PTD Net] [money] NULL,
[AUD End Bal] [money] NULL
)";
                loader.PersistSql =
    @"DELETE FROM [TB_TrialBalance] WHERE DateKey BETWEEN '{FromDate}' AND '{ToDate}';

INSERT INTO [TB_TrialBalance] 
(
[Oid],
[Company],
[Account],
[BS or PL],
[Account Class],
[Account Desc],
[Period Name],
[AUD Beg Bal],
[AUD PTD Dr],
[AUD PTD Cr],
[AUD PTD Net],
[AUD End Bal],
[DateKey]
)
SELECT
NEWID() AS Oid,
[Company],
[Account],
[BS or PL],
[Account Class],
[Account Desc],
[Period Name],
[AUD Beg Bal],
[AUD PTD Dr],
[AUD PTD Cr],
[AUD PTD Net],
[AUD End Bal],
CAST ( '1-' + [Period Name] AS datetime ) AS DateKey
FROM {TempTable}";
                var sourceReader = DataObjectFactory.CreateReaderFromCsv(paramObj.FilePath);

                messagesText = loader.Execute(sourceReader);
            }

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
           messagesText,
          "Import Successful"
           );
        }
    }
}
