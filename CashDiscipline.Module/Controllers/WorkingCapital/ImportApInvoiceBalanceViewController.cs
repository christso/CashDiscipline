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
using CashDiscipline.Module.Clients;
using DevExpress.Data.Filtering;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class ImportApInvoiceBalanceViewController : ViewController
    {
        private const string resetImportSqlCaption = "Import SQL";
        private const string resetMapSqlCaption = "Map SQL";
        private const string resetActyMapSqlCaption = "Acty Map SQL";

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

            var resetAction = new SingleChoiceAction(this, "ResetImportApInvoiceBalanceAction", PredefinedCategory.ObjectsCreation);
            resetAction.Caption = "Reset SQL";
            resetAction.ShowItemsOnClick = true;
            resetAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            resetAction.Execute += ResetAction_Execute;

            var resetImportSqlChoice = new ChoiceActionItem();
            resetImportSqlChoice.Caption = resetImportSqlCaption;
            resetAction.Items.Add(resetImportSqlChoice);
        }

        private void ResetAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            try
            {
                switch (e.SelectedChoiceActionItem.Caption)
                {
                    case resetImportSqlCaption:
                        ResetImportSql();
                        break;
                }

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
            var paramObj = (ImportApInvoiceBalanceParam)View.CurrentObject;

            /*
            Func<string, string> formatSql = delegate(string sql)
            {
                return Smart.Format(sql, new
                {
                    FromDate = string.Format("{0:yyyy-MM-dd}", paramObj.FromDate.Date),
                    ToDate = string.Format("{0:yyyy-MM-dd}", paramObj.ToDate.Date),
                    MapActivitySql = paramObj.ActivityMapSql
                });
            };
            */

            try
            {
                var objSpace = (XPObjectSpace)ObjectSpace;
                
                var mapper = new ApInvoiceBalanceMapper(objSpace);
                mapper.Process(paramObj);

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

            //var importer = new ApInvoiceBalanceImporter(objSpace);

            using (var csvReader = DataObjectFactory.CreateCachedReaderFromCsv(paramObj.FilePath))
            {
                var loader = new SqlServerLoader2((SqlConnection)objSpace.Connection);
                loader.CreateSql = paramObj.CreateSql;
                loader.PersistSql = paramObj.PersistSql;
                var messagesText = loader.Execute(csvReader);

                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    messagesText,
                   "Import Successful"
                    );
            }
        }

        private void ResetImportSql()
        {
            var paramObj = (ImportApInvoiceBalanceParam)View.CurrentObject;
            paramObj.CreateSql = @"CREATE TABLE {TempTable} (
[Request id] int,
[As at Date] date,
[Supplier] nvarchar(255),
[Vendor Type] nvarchar(255),
[Category1] nvarchar(255),
[Category2] nvarchar(255),
[Invoice Number] nvarchar(255),
[Liability Company] nvarchar(10),
[Liability Account] nvarchar(10),
[Liability Cost Centre] nvarchar(10),
[Liability Intercompany] nvarchar(10),
[Invoice date] date,
[GL date] date,
[Trx currency code] nvarchar(10),
[Invoice Amount AUD] float,
[Invoice Remaining Amount AUD] float,
[Line Distribution Remaining Amount AUD] float,
[Opex  Capex  Other] nvarchar(255),
[Extra detail (Opex  Exec  Capex  Delivery Area)] nvarchar(255),
[Extra detail2 (Opex  GM  Capex  Project Number)] nvarchar(255),
[Expense Company] nvarchar(10),
[Expense Account] nvarchar(10),
[Expense Cost Centre] nvarchar(10),
[Expense Product] nvarchar(10),
[Expense Sales Channel] nvarchar(10),
[Expense Country] nvarchar(10),
[Expense Intercompany] nvarchar(10),
[Expense Project] nvarchar(10),
[Expense Location] nvarchar(10),
[Invoice Description] nvarchar(255),
[Due Date] date,
[Payment Term] nvarchar(50),
[Entered rounded orig amount] float,
[Entered rounded rem amount] float
)";
            paramObj.PersistSql = @"INSERT INTO ApInvoiceBalance
(
    [Oid],
    [RequestId],
    [AsAtDate],
    [Supplier],
    [VendorType],
    [Category1],
    [Category2],
    [InvoiceNumber],
    [LiabilityCompany],
    [LiabilityAccount],
    [LiabilityCostCentre],
    [LiabilityIntercompany],
    [InvoiceDate],
    [GlDate],
    [TrxCurrencyCode],
    [InvoiceAmountAud],
    [InvoiceRemainingAmountAud],
    [LineDistributionRemainingAmountAud],
    [Opex_Capex_Other],
    [ExtraDetail1],
    [ExtraDetail2],
    [ExpenseCompany],
    [ExpenseAccount],
    [ExpenseCostCentre],
    [ExpenseProduct],
    [ExpenseSalesChannel],
    [ExpenseCountry],
    [ExpenseIntercompany],
    [ExpenseProject],
    [ExpenseLocation],
    [InvoiceDescription],
    [DueDate],
    [PaymentTerm],
    [EnteredOrigAmount],
    [EnteredOrigRemainingAmount]
)
SELECT
    NEWID() AS Oid,
    [Request id],
    [As at Date],
    [Supplier],
    [Vendor Type],
    [Category1],
    [Category2],
    [Invoice Number],
    [Liability Company],
    [Liability Account],
    [Liability Cost Centre],
    [Liability Intercompany],
    [Invoice date],
    [GL date],
    [Trx currency code],
    [Invoice Amount AUD],
    [Invoice Remaining Amount AUD],
    [Line Distribution Remaining Amount AUD],
    [Opex  Capex  Other],
    [Extra detail (Opex  Exec  Capex  Delivery Area)],
    [Extra detail2 (Opex  GM  Capex  Project Number)],
    [Expense Company],
    [Expense Account],
    [Expense Cost Centre],
    [Expense Product],
    [Expense Sales Channel],
    [Expense Country],
    [Expense Intercompany],
    [Expense Project],
    [Expense Location],
    [Invoice Description],
    [Due Date],
    [Payment Term],
    [Entered rounded orig amount],
    [Entered rounded rem amount]
FROM {TempTable}";
        }
        
        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
        }
    }
}
