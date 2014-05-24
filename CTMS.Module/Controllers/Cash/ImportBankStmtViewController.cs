using CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using DevExpress.Data.Filtering;
using CTMS.Module.BusinessObjects;
using D2NXAF.ExpressApp;
using D2NXAF.Utils;
using D2NXAF.ExpressApp.Xpo;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using LumenWorks.Framework.IO.Csv;
using DevExpress.Xpo;
using DevExpress.ExpressApp.DC;
using System.Collections;

namespace CTMS.Module.Controllers.Cash
{
    public class ImportBankStmtViewController : ViewController
    {
        public ImportBankStmtViewController()
        {
            TargetObjectType = typeof(ImportBankStmtParam);
            TargetViewType = ViewType.DetailView;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            ((DetailView)View).ViewEditMode = ViewEditMode.Edit;
            var dc = Frame.GetController<DialogController>();
            if (dc != null)
            {
                dc.AcceptAction.Execute += AcceptAction_Execute;
                dc.CancelAction.Execute += CancelAction_Execute;
                dc.CanCloseWindow = false;
            }
        }

        protected void CancelAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            View.Close();
        }

        private enum CsvFieldOrds
        {
            TranDate = 0,
            AccountNumber = 1,
            TranType = 7,
            TranRef = 8,
            TranAmount = 9,
            TranDescription = 10,
            TranCode = 13
        }

        protected void AcceptAction_Execute(object sender, DevExpress.ExpressApp.Actions.SimpleActionExecuteEventArgs e)
        {
            var request = new D2NXAF.ExpressApp.Concurrency.RequestManager(Application);

            var paramObj = (ImportBankStmtParam)View.CurrentObject;
            if (paramObj.File.Content == null)
                throw new UserFriendlyException("No file was selected to upload.");
            var byteArray = paramObj.File.Content;
            var stream = new MemoryStream(byteArray);
            bool hasHeaders = false;
            var objSpace = (XPObjectSpace)Application.CreateObjectSpace();

            Action job = new Action(() =>
            {
                using (var csv = new CsvReader(new StreamReader(stream), hasHeaders))
                {
                    var mapping = objSpace.GetObjects<BankStmtAccountImportMapping>();
                    var accountLineNums = new Dictionary<Account, int>();

                    while (csv.ReadNextRecord())
                    {
                        if (request.CancellationTokenSource.IsCancellationRequested)
                            request.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                        BankStmt bankStmt = objSpace.CreateObject<BankStmt>();

                        var map = mapping.FirstOrDefault((a) =>
                            a.InputAccountText == csv[(int)CsvFieldOrds.AccountNumber]);
                        if (map == null) continue;
                        if (!accountLineNums.ContainsKey(map.Account))
                        {
                            accountLineNums.Add(map.Account, 1);
                        }
                        bankStmt.Account = map.Account;

                        bankStmt.TranDate = Convert.ToDateTime(csv[(int)CsvFieldOrds.TranDate]);
                        bankStmt.TranDescription = csv[(int)CsvFieldOrds.TranDescription];
                        bankStmt.TranRef = csv[(int)CsvFieldOrds.TranRef];
                        bankStmt.TranType = csv[(int)CsvFieldOrds.TranType];
                        bankStmt.TranAmount = Convert.ToDecimal(csv[(int)CsvFieldOrds.TranAmount]);
                        bankStmt.CounterCcy = map.Account.Currency;
                        if (map.Account != null)
                        {
                            bankStmt.OraTrxNum = string.Format("{0:yyyyMMdd}-{1}-{2}",
                               bankStmt.TranDate,
                               map.Account.BankAccountNumber.Right(3),
                               accountLineNums[map.Account].ToString().PadLeft(3, '0'));
                            bankStmt.Save();
                            accountLineNums[map.Account]++;
                        }
                    }
                    objSpace.CommitChanges();
                }
            });

            request.SubmitRequest("Import CSV File", job);
        }
    }
}
