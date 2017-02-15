using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.ParamObjects.Import;
using Xafology.Utils;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using SDP.ParserUtils;
using System;
using System.IO;
using System.Text;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using CashDiscipline.Module.Logic.Forex;
using CashDiscipline.Module.Logic;
using CashDiscipline.Module.ServiceReference1;
using System.Data;
using System.Data.SqlClient;
using CashDiscipline.Module.Clients;

namespace CashDiscipline.Module.Controllers.Forex
{
    public class ImportForexParamViewController : ViewController
    {
        public ImportForexParamViewController()
        {
            TargetObjectType = typeof(ImportForexRatesParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportWbcForexRatesAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Run Import";
            importAction.Execute += ImportAction_Execute;
            
        }

        private void ImportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var messageText = WbcImport();

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
               messageText,
               "Import Successful"
               );
        }

        public string WbcImport()
        {
            var table = GetWbcDataTable();
            var paramObj = View.CurrentObject as ImportForexRatesParam;
            using (var loader = new SqlServerLoader2((SqlConnection)paramObj.Session.Connection))
            {
                loader.ColumnMappings.Add("ConversionDate", "ConversionDate");
                loader.ColumnMappings.Add("FromCcyCode", "FromCcyCode");
                loader.ColumnMappings.Add("Rate", "Rate");

                //loader.TempTableName = "TmpForexRate1";
                loader.CreateSql = @"
CREATE TABLE {TempTable} (
ConversionDate datetime,
FromCcyCode nvarchar(50),
Rate [decimal](19, 6)
);";

                var convDate = table.Rows[0]["ConversionDate"];
                loader.SqlStringReplacers.Add("ConvDate", string.Format("{0:yyyy-MM-dd}", convDate));

                loader.PersistSql = @"DELETE FROM dbo.ForexRate WHERE ConversionDate = '{ConvDate}'
INSERT INTO ForexRate
(
Oid,
ConversionDate,
FromCurrency,
ToCurrency,
ConversionRate
)

SELECT
NEWID() AS Oid,
'{ConvDate}' AS ConversionDate,
(SELECT c1.Oid FROM Currency c1 WHERE c1.Name LIKE 'AUD' AND c1.GCRecord IS NULL) AS FromCurrency,
c2.Oid AS ToCurrency,
t1.Rate
FROM {TempTable} t1
INNER JOIN Currency c2 ON c2.Name LIKE t1.FromCcyCode AND c2.GCRecord IS NULL
WHERE t1.Rate <> 0.00

UNION ALL

SELECT
NEWID(),
'{ConvDate}',
c2.Oid AS FromCurrency,
(SELECT c1.Oid FROM Currency c1 WHERE c1.Name LIKE 'AUD' AND c1.GCRecord IS NULL) AS ToCurrency,
1 / t1.Rate AS Rate
FROM {TempTable} t1
INNER JOIN Currency c2 ON c2.Name LIKE t1.FromCcyCode AND c2.GCRecord IS NULL
WHERE t1.Rate <> 0.00
";


                var messagesText = loader.Execute(table);
                return messagesText;
            }
        }

        public DataTable GetWbcDataTable()
        {
            var paramObj = View.CurrentObject as ImportForexRatesParam;
            var filePath = paramObj.FileName;

            var table = new DataTable("RateTable");
            var convDateColumn = new DataColumn("ConversionDate", typeof(DateTime));
            var ccyColumn = new DataColumn("FromCcyCode", typeof(string));
            var rateColumn = new DataColumn("Rate", typeof(double));
            table.Columns.Add(convDateColumn);
            table.Columns.Add(ccyColumn);
            table.Columns.Add(rateColumn);

            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
            {
                int lineNumber = 0;
                DateTime convDate = default(DateTime);
                string dateText = "";
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    lineNumber++;
                    if (lineNumber == 3)
                    {
                        //ensure that length argument does not exceed the line length
                        dateText = line.Substring(60, 20 + line.Length - 80);
                        convDate = DateTime.Parse(dateText);
                    }
                    if (lineNumber < 11)
                        continue;
                    if (lineNumber > 59)
                        break;

                    var ccyCode = line.Substring(35, 3);
                    var ttBuyText = line.Substring(41, 6);
                    var chqBuyText = line.Substring(50, 6);
                    var noteBuyText = line.Substring(59, 6);
                    var ttSellText = line.Substring(68, 6);

                    double ttBuy = 0;
                    double ttSell = 0;
                    double ttMid = 0;
                    bool ttBuyFlag = double.TryParse(ttBuyText, out ttBuy);
                    bool ttSellFlag = double.TryParse(ttSellText, out ttSell);

                    if (ttBuyFlag && !ttSellFlag)
                        ttSell = ttBuy;
                    else if (!ttBuyFlag && ttSellFlag)
                        ttBuy = ttSell;

                    ttMid = (ttBuy + ttSell) / 2;
                    var row = table.NewRow();
                    row[ccyColumn] = ccyCode;
                    row[rateColumn] = ttMid;
                    row[convDateColumn] = convDate;
                    table.Rows.Add(row);
                }
            }
            return table;
        }
        
    
        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
        }        
    }
}
