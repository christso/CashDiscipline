using CashDiscipline.Module.Logic;
using CashDiscipline.Module.Logic.Cash;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashDiscipline.Module.ServiceReference1;
using DevExpress.ExpressApp.Xpo;
using CashDiscipline.Module.Clients;
using System.Data.SqlClient;
using System.Reflection;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;

namespace CashDiscipline.Module.Controllers.Cash
{
    public class ImportApPmtDistnParamViewController : ViewController
    {
        public ImportApPmtDistnParamViewController()
        {
            TargetObjectType = typeof(ImportApPmtDistnParam);
            TargetViewType = ViewType.DetailView;

            var importAction = new SimpleAction(this, "ImportApPmtDistnAction", PredefinedCategory.ObjectsCreation);
            importAction.Caption = "Run Import";
            importAction.Execute += ImportAction_Execute;


            var resetAction = new SingleChoiceAction(this, "ResetImportApPmtDistnAction", PredefinedCategory.ObjectsCreation);
            resetAction.Caption = "Reset";
            resetAction.ShowItemsOnClick = true;
            resetAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            resetAction.Execute += ResetAction_Execute;

            var resetSqlChoice = new ChoiceActionItem();
            resetSqlChoice.Caption = "Reset SQL";
            resetAction.Items.Add(resetSqlChoice);

            var resetColumns = new ChoiceActionItem();
            resetColumns.Caption = "Reset Columns";
            resetAction.Items.Add(resetColumns);
        }

        private void ResetAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            try
            {
                switch(e.SelectedChoiceActionItem.Caption)
                {
                    case "Reset SQL":
                        ResetSql();
                        break;
                    case "Reset Columns":
                        ResetColumns();
                        break;
                }
                
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
            var paramObj = View.CurrentObject as ImportApPmtDistnParam;
            var objSpace = (XPObjectSpace)ObjectSpace;


            var csvColumns = objSpace.GetObjects<ImportApPmtDistnColumn>()
                .Where(x => x.ImportApPmtDistnParam == paramObj)
                .OrderBy(x => x.Ordinal);
     
            using (var csvReader = DataObjectFactory.CreateCachedReaderFromCsv(paramObj.FilePath))
            {
                var ih = new ImportHelper();

                foreach (var csvColumn in csvColumns)
                {
                    string name = csvColumn.Name;
                    Type type = ih.ParseType(csvColumn.TypeName);
                    csvReader.Columns.Add(new LumenWorks.Framework.IO.Csv.Column() { Name = name, Type = type });
                }


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

        private void ResetColumns()
        {
            ImportApPmtDistnColumn obj = null;
            var objs = ObjectSpace.GetObjects<ImportApPmtDistnColumn>();
            ObjectSpace.Delete(objs);

            obj = CreateImportColumn(ObjectSpace, 1, "Payment Date", "DateTime");
            obj = CreateImportColumn(ObjectSpace, 2, "Invoice Due Date", "DateTime");
            obj = CreateImportColumn(ObjectSpace, 3, "Source", "string");
            obj = CreateImportColumn(ObjectSpace, 4, "Capex Opex", "string");
            obj = CreateImportColumn(ObjectSpace, 5, "Org Name", "string");
            obj = CreateImportColumn(ObjectSpace, 6, "Bank Account Name", "string");
            obj = CreateImportColumn(ObjectSpace, 7, "Pay Group", "string");
            obj = CreateImportColumn(ObjectSpace, 8, "Inv Source", "string");
            obj = CreateImportColumn(ObjectSpace, 9, "Vendor Name", "string");
            obj = CreateImportColumn(ObjectSpace, 10, "Company", "string");
            obj = CreateImportColumn(ObjectSpace, 11, "Account", "string");
            obj = CreateImportColumn(ObjectSpace, 12, "Cost Centre", "string");
            obj = CreateImportColumn(ObjectSpace, 13, "Product", "string");
            obj = CreateImportColumn(ObjectSpace, 14, "Sales Channel", "string");
            obj = CreateImportColumn(ObjectSpace, 15, "Country", "string");
            obj = CreateImportColumn(ObjectSpace, 16, "Intercompany", "string");
            obj = CreateImportColumn(ObjectSpace, 17, "Project", "string");
            obj = CreateImportColumn(ObjectSpace, 18, "Location", "string");
            obj = CreateImportColumn(ObjectSpace, 19, "Po Num", "string");
            obj = CreateImportColumn(ObjectSpace, 20, "Invoice Num", "string");
            obj = CreateImportColumn(ObjectSpace, 21, "Invoice Creation Date", "DateTime");
            obj = CreateImportColumn(ObjectSpace, 22, "Distribution Line Number", "int");
            obj = CreateImportColumn(ObjectSpace, 23, "Payment Amount Fx", "double");
            obj = CreateImportColumn(ObjectSpace, 24, "Payment Amount Aud", "double");
            obj = CreateImportColumn(ObjectSpace, 25, "Payment Number", "int");
            obj = CreateImportColumn(ObjectSpace, 26, "Payment Batch Name", "string");
            obj = CreateImportColumn(ObjectSpace, 27, "Payment Method", "string");
            obj = CreateImportColumn(ObjectSpace, 28, "Invoice Currency", "string");
            obj = CreateImportColumn(ObjectSpace, 29, "Payment Creation Date", "DateTime");
            obj = CreateImportColumn(ObjectSpace, 30, "Line Type Lookup Code", "string");
            obj = CreateImportColumn(ObjectSpace, 31, "Invoice Line Desc", "string");
            obj = CreateImportColumn(ObjectSpace, 32, "Tax Code", "string");
            obj = CreateImportColumn(ObjectSpace, 33, "Payment Currency", "string");
            obj = CreateImportColumn(ObjectSpace, 34, "Invoice Date", "DateTime");
            obj = CreateImportColumn(ObjectSpace, 35, "Capex Number", "string");
            obj = CreateImportColumn(ObjectSpace, 36, "Invoice Id", "int");
            obj = CreateImportColumn(ObjectSpace, 37, "Expenditure Type", "string");
            obj = CreateImportColumn(ObjectSpace, 38, "Project Number", "string");
            obj = CreateImportColumn(ObjectSpace, 39, "Vendor Number", "string");
        }

        private ImportApPmtDistnColumn CreateImportColumn(IObjectSpace os, int ord, string name, string typeName)
        {
            var obj = os.CreateObject<ImportApPmtDistnColumn>();
            obj.ImportApPmtDistnParam = (ImportApPmtDistnParam)View.CurrentObject;
            obj.Ordinal = ord;
            obj.Name = name;
            obj.TypeName = typeName;
            return obj;
        }

        private void ResetSql()
        {
            var paramObj = (ImportApPmtDistnParam)View.CurrentObject;
            paramObj.CreateSql = @"CREATE TABLE {TempTable} (
[Payment Date] date,
[Invoice Due Date] date,
[Source] nvarchar(50),
[Capex Opex] nvarchar(50),
[Org Name] nvarchar(50),
[Bank Account Name] nvarchar(100),
[Pay Group] nvarchar(50),
[Inv Source] nvarchar(50),
[Vendor Name] nvarchar(255),
[Company] nvarchar(10),
[Account] nvarchar(10),
[Cost Centre] nvarchar(10),
[Product] nvarchar(10),
[Sales Channel] nvarchar(10),
[Country] nvarchar(10),
[Intercompany] nvarchar(10),
[Project] nvarchar(10),
[Location] nvarchar(10),
[Po Num] nvarchar(255),
[Invoice Num] nvarchar(255),
[Invoice Creation Date] date,
[Distribution Line Number] int,
[Payment Amount Fx] float,
[Payment Amount Aud] float,
[Payment Number] int,
[Payment Batch Name] nvarchar(50),
[Payment Method] nvarchar(50),
[Invoice Currency] nvarchar(10),
[Payment Creation Date] date,
[Line Type Lookup Code] nvarchar(50),
[Invoice Line Desc] nvarchar(255),
[Tax Code] nvarchar(50),
[Payment Currency] nvarchar(10),
[Invoice Date] date,
[Capex Number] nvarchar(50),
[Invoice Id] int,
[Expenditure Type] nvarchar(255),
[Project Number] nvarchar(255),
[Vendor Number] nvarchar(100)
)";
            paramObj.PersistSql = @"/* Insert Lookups */
INSERT INTO ApSource (Oid, Name)
SELECT NEWID() AS Oid,
	p.*
FROM
(
SELECT DISTINCT
	p.Source
FROM {TempTable} p
WHERE p.Source NOT IN (SELECT s.Name FROM ApSource s WHERE s.GCRecord IS NULL)
) p

INSERT INTO ApBankAccount (Oid, BankAccountName)
SELECT NEWID() AS Oid,
	p.*
FROM
(
SELECT DISTINCT
	p.[Bank Account Name]
FROM {TempTable} p
WHERE p.[Bank Account Name] NOT IN (SELECT a.BankAccountName FROM ApBankAccount a WHERE a.GCRecord IS NULL)
) p

INSERT INTO ApPayGroup (Oid, Name)
SELECT NEWID() AS Oid,
	p.*
FROM
(
SELECT DISTINCT
	p.[Pay Group]
FROM {TempTable} p
WHERE p.[Pay Group] NOT IN (SELECT a.Name FROM ApPayGroup a WHERE a.GCRecord IS NULL)
) p

INSERT INTO ApInvSource (Oid, Name)
SELECT NEWID() AS Oid,
	p.*
FROM
(
SELECT DISTINCT
	p.[Inv Source]
FROM {TempTable} p
WHERE p.[Inv Source] NOT IN (SELECT a.Name FROM ApInvSource a WHERE a.GCRecord IS NULL)
) p

INSERT INTO ApVendor (Oid, Name)
SELECT NEWID() AS Oid,
	p.*
FROM
(
SELECT DISTINCT
	p.[Vendor Name]
FROM {TempTable} p
WHERE p.[Vendor Name] NOT IN (SELECT a.Name FROM ApVendor a WHERE a.GCRecord IS NULL)
) p

INSERT INTO Currency (Oid, Name)
SELECT NEWID() AS Oid,
	p.*
FROM
(
SELECT DISTINCT
	p.[Payment Currency]
FROM {TempTable} p
WHERE p.[Payment Currency] NOT IN (SELECT a.Name FROM Currency a WHERE a.GCRecord IS NULL)
) p

INSERT INTO Currency (Oid, Name)
SELECT NEWID() AS Oid,
	p.*
FROM
(
SELECT DISTINCT
	p.[Invoice Currency]
FROM {TempTable} p
WHERE p.[Invoice Currency] NOT IN (SELECT a.Name FROM Currency a WHERE a.GCRecord IS NULL)
) p

/* Import Fact Table */
INSERT INTO ApPmtDistn
(
[Oid],
[PaymentDate],
[Source],
[BankAccount],
[PayGroup],
[InvSource],
[Vendor],
[GlCompany],
[GlAccount],
[GlCostCentre],
[GlProduct],
[GlSalesChannel],
[GlCountry],
[GlIntercompany],
[GlProject],
[GlLocation],
[PoNum],
[InvoiceNum],
[PaymentAmountFx],
[PaymentAmountAud],
[PaymentNumber],
[PaymentBatchName],
[InvoiceCurrency],
[PaymentCreationDate],
[InvoiceLineDesc],
[PaymentCurrency],
[InvoiceDueDate],
[InvoiceCreationDate],
[CapexOpex],
[OrgName],
[LineType],
[TaxCode],
[InvoiceDate],
[ExpenditureType],
[ProjectNumber],
[VendorNumber],
[InvoiceId],
[DistributionLineNumber],
[InputSource]
)
SELECT
CAST(CAST(NEWID() AS BINARY(10)) + CAST(GETDATE() AS BINARY(6)) AS UNIQUEIDENTIFIER) AS Oid,
tp.[Payment Date],
(SELECT s.Oid FROM ApSource s WHERE s.Name LIKE tp.[Source] AND s.GCRecord IS NULL) AS Source,
(SELECT a.Oid FROM ApBankAccount a WHERE a.BankAccountName LIKE tp.[Bank Account Name] AND a.GCRecord IS NULL) AS BankAccount,
(SELECT a.Oid FROM ApPayGroup a WHERE a.Name LIKE tp.[Pay Group] AND a.GCRecord IS NULL) AS PayGroup,
(SELECT a.Oid FROM ApInvSource a WHERE a.Name LIKE tp.[Inv Source] AND a.GCRecord IS NULL) AS InvSource,
(SELECT a.Oid FROM ApVendor a WHERE a.Name LIKE tp.[Vendor Name] AND a.GCRecord IS NULL) AS Vendor,
tp.[Company],
tp.[Account],
tp.[Cost Centre],
tp.[Product],
tp.[Sales Channel],
tp.[Country],
tp.[Intercompany],
tp.[Project],
tp.[Location],
TRY_CONVERT(int, tp.[Po Num]),
tp.[Invoice Num],
tp.[Payment Amount Fx],
tp.[Payment Amount Aud],
tp.[Payment Number],
tp.[Payment Batch Name],
(SELECT a.Oid FROM Currency a WHERE a.Name LIKE tp.[Invoice Currency] AND a.GCRecord IS NULL) AS InvoiceCurrency,
tp.[Payment Creation Date],
tp.[Invoice Line Desc],
(SELECT a.Oid FROM Currency a WHERE a.Name LIKE tp.[Payment Currency] AND a.GCRecord IS NULL) AS [Payment Currency],
tp.[Invoice Due Date],
tp.[Invoice Creation Date],
tp.[Capex Opex],
tp.[Org Name],
tp.[Line Type Lookup Code],
tp.[Tax Code],
tp.[Invoice Date],
tp.[Expenditure Type],
TRY_CONVERT(int, tp.[Project Number]),
tp.[Vendor Number],
tp.[Invoice Id],
tp.[Distribution Line Number],
0 AS InputSource
FROM {TempTable} tp";
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
        }
    }
}
