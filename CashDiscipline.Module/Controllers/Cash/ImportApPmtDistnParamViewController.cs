﻿using CashDiscipline.Module.Logic;
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

            using (var csvReader = DataObjectFactory.CreateReaderFromCsv(paramObj.FilePath))
            {
                var loader = new SqlServerLoader((SqlConnection)objSpace.Connection);

                loader.CreateSql = @"CREATE TABLE {TempTable} (
[Actual Payment Date] date,
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
[Actual Payment Amount Fx SUM] float,
[Payment Amount Aud SUM] float,
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
[Project Number] nvarchar(255)
)";
                loader.PersistSql = @"/* Insert Lookups */
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
[InvoiceId],
[DistributionLineNumber],
[InputSource]
)
SELECT
CAST(CAST(NEWID() AS BINARY(10)) + CAST(GETDATE() AS BINARY(6)) AS UNIQUEIDENTIFIER) AS Oid,
tp.[Actual Payment Date],
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
tp.[Actual Payment Amount Fx SUM],
tp.[Payment Amount Aud SUM],
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
tp.[Invoice Id],
tp.[Distribution Line Number],
0 AS InputSource
FROM {TempTable} tp";
                var messagesText = loader.Execute(csvReader);

                new Xafology.ExpressApp.SystemModule.GenericMessageBox(
                    messagesText,
                   "Import Successful"
                    );
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
        }
    }
}
