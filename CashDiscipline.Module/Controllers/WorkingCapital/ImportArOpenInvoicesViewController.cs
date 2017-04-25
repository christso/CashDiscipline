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
using LumenWorks.Framework.IO.Csv;
using System.IO;

namespace CashDiscipline.Module.Controllers.WorkingCapital
{

    public class ImportArOpenInvoicesViewController : ViewController
    {
        private const string ResetSqlChoiceCaption = "Reset SQL";

        public ImportArOpenInvoicesViewController()
        {
            TargetObjectType = typeof(ImportArOpenInvoicesParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportArOpenInvoicesAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Import";
            importAction.Execute += ImportAction_Execute;

            var reportAction = new SimpleAction(this, "ReportArOpenInvoicesAction", PredefinedCategory.ObjectsCreation);
            reportAction.Caption = "Process Report";
            reportAction.Execute += ReportAction_Execute;

            var resetAction = new SingleChoiceAction(this, "ResetImportArOpenInvoicesAction", PredefinedCategory.ObjectsCreation);
            resetAction.ShowItemsOnClick = true;
            resetAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            resetAction.Caption = "Reset";
            resetAction.Execute += ResetAction_Execute;

            var resetSqlChoice = new ChoiceActionItem();
            resetSqlChoice.Caption = ResetSqlChoiceCaption;
            resetAction.Items.Add(resetSqlChoice);
        
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
        ""database"": ""ArOpenInvoices""
      }
    ]
  }
}");

            new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                "Process Report Successful",
                "Process Report Successful"
            );
        }

        private void ResetAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            switch (e.SelectedChoiceActionItem.Caption)
            {
                case ResetSqlChoiceCaption:
                    ResetSql();
                    break;

            }
        }

        private void ResetSql()
        {

            var paramObj = (ImportArOpenInvoicesParam)View.CurrentObject;
            paramObj.CreateSql = @"CREATE TABLE {TempTable} (
[Customer Name] nvarchar(255),
[Customer Number] nvarchar(255),
[Trx Date] date,
[Due Date] date,
[Trx Type] nvarchar(255),
[Trx Number] nvarchar(255),
[Apply Date] nvarchar(255),
[Trx Status] nvarchar(255),
[Sales Order] nvarchar(255),
[Reference] nvarchar(255),
[Currency Code] nvarchar(255),
[Original Amount] nvarchar(255),
[Balance Due] nvarchar(255),
[Credited Amount] nvarchar(255),
[Adjustment Amount] nvarchar(255),
[Applied Amount] nvarchar(255),
[Original Receipt Amount] nvarchar(255),
[Overdue Days] nvarchar(255)
)";
            paramObj.PersistSql =
@"INSERT INTO ArOpenInvoices
(
    [Oid],
    [AsAtDate],
    [CustomerName],
    [CustomerNumber],
    [TrxDate],
    [DueDate],
    [TrxType],
    [TrxNumber],
    [ApplyDate],
    [TrxStatus],
    [SalesOrder],
    [Reference],
    [CurrencyCode],
    [OriginalAmount],
    [BalanceDue],
    [CreditedAmount],
    [AdjustmentAmount],
    [AppliedAmount],
    [OriginalReceiptAmount],
    [OverdueDays]
)
SELECT
    NEWID() AS Oid,
    '{AsAtDate}' AS [As At Date],
    [Customer Name],
    [Customer Number],
    [Trx Date],
    [Due Date],
    [Trx Type],
    [Trx Number],
    TRY_CAST([Apply Date] AS date),
    [Trx Status],
    [Sales Order],
    [Reference],
    [Currency Code],
    TRY_CAST([Original Amount] AS float),
    TRY_CAST([Balance Due] AS float),
    TRY_CAST([Credited Amount] AS float),
    TRY_CAST([Adjustment Amount] AS float),
    TRY_CAST([Applied Amount] AS float),
    TRY_CAST([Original Receipt Amount] AS float),
    TRY_CAST([Overdue Days] AS int)
FROM {TempTable}
";
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

        private void Import()
        {
            var paramObj = (ImportArOpenInvoicesParam)View.CurrentObject;
            var objSpace = (XPObjectSpace)ObjectSpace;

            using (var csvReader = DataObjectFactory.CreateCachedReaderFromCsv(paramObj.FilePath))
            {
                var loader = new SqlServerLoader2((SqlConnection)objSpace.Connection);
                loader.SqlStringReplacers.Add("AsAtDate", string.Format("{0:yyyy-MM-dd}", paramObj.AsAtDate.Date));
                loader.CreateSql = paramObj.CreateSql;
                loader.PersistSql = paramObj.PersistSql;
                var messagesText = loader.Execute(csvReader);

                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    messagesText,
                   "Import Successful"
                    );
            }
        }
    }
}
