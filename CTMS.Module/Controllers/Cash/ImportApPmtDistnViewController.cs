using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Cash.AccountsPayable;
using CTMS.Module.BusinessObjects.ChartOfAccounts;
using CTMS.Module.ParamObjects.Import;
using Xafology.ExpressApp.Concurrency;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using LumenWorks.Framework.IO.Csv;
using System;
using System.IO;
using System.Linq;

namespace CTMS.Module.Controllers.Cash
{
    public class ImportApPmtDistnViewController : ViewController<DetailView>
    {
        public ImportApPmtDistnViewController()
        {
            TargetObjectType = typeof(ImportApPmtDistnParam);
        }


        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
            var dc = Frame.GetController<DialogController>();
            if (dc != null)
            {
                dc.AcceptAction.Execute += AcceptAction_Execute;
                dc.CanCloseWindow = false;
            }
        }

        private void AcceptAction_Execute(object sender, DevExpress.ExpressApp.Actions.SimpleActionExecuteEventArgs e)
        {
            var request = new RequestManager(Application);

            var paramObj = (ImportApPmtDistnParam)View.CurrentObject;
            if (paramObj.File.Content == null)
                throw new UserFriendlyException("No file was selected to upload.");
            var byteArray = paramObj.File.Content;
            var stream = new MemoryStream(byteArray);
            var objSpace = (XPObjectSpace)Application.CreateObjectSpace();

            Action job = null;
            job = new Action(() =>
                {
                    var schema = new ApPmtDistnCsvSchema();
                    schema.ImportStream(request, stream, true, objSpace);
                });
            request.SubmitRequest("Import ApPmtDistn CSV File", job);
            View.Close();
        }

        public class ApPmtDistnCsvSchema
        {
            public enum ApPmtDistnCsvOrds
            {
                ActualPaymentDate = 0,
                InvoiceDueDate = 1,
                Source = 2,
                CapexOpex = 3,
                OrgName = 4,
                BankAccountName = 5,
                PayGroup = 6,
                InvSource = 7,
                VendorName = 8,
                Company = 9,
                Account = 10,
                CostCentre = 11,
                Product = 12,
                SalesChannel = 13,
                Country = 14,
                Intercompany = 15,
                Project = 16,
                Location = 17,
                PoNum = 18,
                InvoiceNum = 19,
                DistributionLineNumber = 20,
                ActualPaymentAmountFx = 21,
                PaymentAmountAud = 22,
                PaymentNumber = 23,
                PaymentBatchName = 24,
                PaymentMethod = 25,
                InvoiceCurrency = 26,
                PaymentCreationDate = 27,
                LineType = 28,
                InvoiceLineDesc = 29,
                TaxCode = 30,
                PaymentCurrency = 31,
                InvoiceDate = 32,
                CapexNumber = 33,
                InvoiceId = 34,
                ExpenditureType = 35
            }

            public void ImportStream(RequestManager request, MemoryStream stream, bool hasHeaders, XPObjectSpace objSpace)
            {
                var apSources = objSpace.GetObjects<ApSource>();
                var apPayGroups = objSpace.GetObjects<ApPayGroup>();
                var apInvSources = objSpace.GetObjects<ApInvSource>();
                var apVendors = objSpace.GetObjects<ApVendor>();
                var glCompany = objSpace.GetObjects<GlCompany>();
                var glAccount = objSpace.GetObjects<GlAccount>();
                var glCostCentre = objSpace.GetObjects<GlCostCentre>();
                var glProduct = objSpace.GetObjects<GlProduct>();
                var glSalesChannel = objSpace.GetObjects<GlSalesChannel>();
                var glCountry = objSpace.GetObjects<GlCountry>();
                var glIntercompany = objSpace.GetObjects<GlIntercompany>();
                var glProject = objSpace.GetObjects<GlProject>();
                var glLocation = objSpace.GetObjects<GlLocation>();
                var currencies = objSpace.GetObjects<Currency>();
                var apBankAccount = objSpace.GetObjects<ApBankAccount>();

                // TODO: add lookup objects which do not exist

                using (var csv = new CsvReader(new StreamReader(stream), hasHeaders))
                {
                    while (csv.ReadNextRecord())
                    {
                        if (request.CancellationTokenSource.IsCancellationRequested)
                            request.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                        var apPmtDistn = objSpace.CreateObject<ApPmtDistn>();

                        apPmtDistn.PaymentDate = Convert.ToDateTime(csv[(int)ApPmtDistnCsvOrds.ActualPaymentDate]);
                        apPmtDistn.InvoiceDueDate = Convert.ToDateTime(csv[(int)ApPmtDistnCsvOrds.InvoiceDueDate]);
                        apPmtDistn.Source = apSources.FirstOrDefault(x => x.Name == csv[(int)ApPmtDistnCsvOrds.Source]);
                        apPmtDistn.PayGroup = apPayGroups.FirstOrDefault(x => x.Name == csv[(int)ApPmtDistnCsvOrds.PayGroup]);
                        apPmtDistn.InvSource = apInvSources.FirstOrDefault(x => x.Name == csv[(int)ApPmtDistnCsvOrds.InvSource]);
                        apPmtDistn.Vendor = apVendors.FirstOrDefault(x => x.Name == csv[(int)ApPmtDistnCsvOrds.VendorName]);
                        apPmtDistn.GlCompany = glCompany.FirstOrDefault(x => x.Code == csv[(int)ApPmtDistnCsvOrds.Company]);
                        apPmtDistn.GlAccount = glAccount.FirstOrDefault(x => x.Code == csv[(int)ApPmtDistnCsvOrds.Account]);
                        apPmtDistn.GlCostCentre = glCostCentre.FirstOrDefault(x => x.Code == csv[(int)ApPmtDistnCsvOrds.CostCentre]);
                        apPmtDistn.GlProduct = glProduct.FirstOrDefault(x => x.Code == csv[(int)ApPmtDistnCsvOrds.Product]);
                        apPmtDistn.GlSalesChannel = glSalesChannel.FirstOrDefault(x => x.Code == csv[(int)ApPmtDistnCsvOrds.SalesChannel]);
                        apPmtDistn.GlCountry = glCountry.FirstOrDefault(x => x.Code == csv[(int)ApPmtDistnCsvOrds.Country]);
                        apPmtDistn.GlIntercompany = glIntercompany.FirstOrDefault(x => x.Code == csv[(int)ApPmtDistnCsvOrds.Intercompany]);
                        apPmtDistn.GlProject = glProject.FirstOrDefault(x => x.Code == csv[(int)ApPmtDistnCsvOrds.Project]);
                        apPmtDistn.GlLocation = glLocation.FirstOrDefault(x => x.Code == csv[(int)ApPmtDistnCsvOrds.Location]);
                        apPmtDistn.PoNum = ConvertStringToInt(csv[(int)ApPmtDistnCsvOrds.PoNum]);
                        apPmtDistn.InvoiceNum = csv[(int)ApPmtDistnCsvOrds.InvoiceNum];
                        apPmtDistn.PaymentAmountFx = ConvertStringToDecimal(csv[(int)ApPmtDistnCsvOrds.ActualPaymentAmountFx]);
                        apPmtDistn.PaymentAmountAud = ConvertStringToDecimal(csv[(int)ApPmtDistnCsvOrds.PaymentAmountAud]);
                        apPmtDistn.PaymentNumber = ConvertStringToInt(csv[(int)ApPmtDistnCsvOrds.PaymentNumber]);
                        apPmtDistn.PaymentBatchName = csv[(int)ApPmtDistnCsvOrds.PaymentBatchName];
                        apPmtDistn.InvoiceCurrency = currencies.FirstOrDefault(x => x.Name == csv[(int)ApPmtDistnCsvOrds.InvoiceCurrency]);
                        apPmtDistn.PaymentCreationDate = Convert.ToDateTime(csv[(int)ApPmtDistnCsvOrds.PaymentCreationDate]);
                        apPmtDistn.InvoiceLineDesc = csv[(int)ApPmtDistnCsvOrds.InvoiceLineDesc];
                        apPmtDistn.PaymentCurrency = currencies.FirstOrDefault(x => x.Name == csv[(int)ApPmtDistnCsvOrds.PaymentCurrency]);
                        var accountMap = apBankAccount.FirstOrDefault((x) => x.BankAccountName == csv[(int)ApPmtDistnCsvOrds.BankAccountName]);
                        apPmtDistn.BankAccount = accountMap;
                    }
                    objSpace.CommitChanges();
                }
            }

            private static decimal ConvertStringToDecimal(string value)
            {
                if (string.IsNullOrEmpty(value))
                    return 0;
                else
                    return Convert.ToDecimal(value);
            }

            private static int ConvertStringToInt(string value)
            {
                if (string.IsNullOrEmpty(value))
                    return 0;
                else
                    return Convert.ToInt32(value);
            }
        }
    }
}
