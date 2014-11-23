using CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.ParamObjects.Import;
using Xafology.ExpressApp.Concurrency;
using Xafology.Utils;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        #region ANZ

        public class AnzCsvSchema
        {
            public enum AnzCsvFieldOrds
            {
                TranDate = 0,
                AccountNumber = 1,
                TranType = 7,
                TranRef = 8,
                TranAmount = 9,
                TranDescription = 10,
                TranCode = 13
            }

            public void ImportStream(RequestManager request, MemoryStream stream, bool hasHeaders, XPObjectSpace objSpace)
            {
                using (var csv = new CsvReader(new StreamReader(stream), hasHeaders))
                {
                    var mapping = objSpace.GetObjects<BankStmtAccountImportMapping>();
                    var accountLineNums = new Dictionary<Account, int>();

                    while (csv.ReadNextRecord())
                    {
                        if (request.CancellationTokenSource.IsCancellationRequested)
                            request.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                        var map = mapping.FirstOrDefault((a) =>
                            a.InputAccountText == csv[(int)AnzCsvFieldOrds.AccountNumber]);
                        if (map == null)
                        {
                            request.WriteLogLine(string.Format("Skipped line {0} as account '{1}' is not mapped",
                                csv.CurrentRecordIndex + 1, csv[(int)AnzCsvFieldOrds.AccountNumber]), false);
                            continue;
                        }

                        BankStmt bankStmt = objSpace.CreateObject<BankStmt>();

                        // TODO: Implement line number with SequentialNumber
                        if (!accountLineNums.ContainsKey(map.Account))
                        {
                            accountLineNums.Add(map.Account, 1);
                        }

                        bankStmt.Account = map.Account;

                        bankStmt.TranDate = Convert.ToDateTime(csv[(int)AnzCsvFieldOrds.TranDate]);
                        bankStmt.TranDescription = csv[(int)AnzCsvFieldOrds.TranDescription];
                        bankStmt.TranRef = csv[(int)AnzCsvFieldOrds.TranRef];
                        bankStmt.TranType = csv[(int)AnzCsvFieldOrds.TranType];
                        bankStmt.TranAmount = Convert.ToDecimal(csv[(int)AnzCsvFieldOrds.TranAmount]);
                        bankStmt.CounterCcy = map.Account.Currency;
                        if (map.Account != null)
                        {
                            // TODO: Implement line number with SequentialNumber
                            bankStmt.OraTrxNum = string.Format("{0:yyyyMMdd}-{1}-{2}",
                               bankStmt.TranDate,
                               map.Account.BankAccountNumber.Right(3),
                               accountLineNums[map.Account].ToString().PadLeft(3, '0'));
                            accountLineNums[map.Account]++;
                        }
                        bankStmt.Save();
                    }
                    objSpace.CommitChanges();
                }
            }
        }
        #endregion

        #region WBC
        public class WbcCsvSchema
        {
            public enum WbcCsvFieldOrds
            {
                TranDate = 0,
                AccountNumber = 1,
                TranRef = 8,
                TranAmount = 5,
                TranDescription = 7,
                TranCode = 6
            }

            public void ImportStream(RequestManager request, MemoryStream stream, bool hasHeaders, XPObjectSpace objSpace)
            {
                using (var csv = new CsvReader(new StreamReader(stream), hasHeaders))
                {
                    var mapping = objSpace.GetObjects<BankStmtAccountImportMapping>();
                    var accountLineNums = new Dictionary<Account, int>(); // TODO: Implement line number with SequentialNumber

                    while (csv.ReadNextRecord())
                    {
                        if (request.CancellationTokenSource.IsCancellationRequested)
                            request.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                        var map = mapping.FirstOrDefault((a) =>
                            a.InputAccountText == csv[(int)WbcCsvFieldOrds.AccountNumber]);
                        if (map == null)
                        {
                            request.WriteLogLine(string.Format("Skipped line {0} as account '{1}' is not mapped",
                                csv.CurrentRecordIndex + 1, csv[(int)WbcCsvFieldOrds.AccountNumber]), false);
                            continue;
                        }

                        BankStmt bankStmt = objSpace.CreateObject<BankStmt>();

                        // TODO: Implement line number with SequentialNumber
                        if (!accountLineNums.ContainsKey(map.Account))
                        {
                            accountLineNums.Add(map.Account, 1);
                        }

                        bankStmt.Account = map.Account;

                        bankStmt.TranDate = ConvertWbcDateTime(Convert.ToString(csv[(int)WbcCsvFieldOrds.TranDate]));
                        bankStmt.TranDescription = csv[(int)WbcCsvFieldOrds.TranDescription];
                        bankStmt.TranRef = csv[(int)WbcCsvFieldOrds.TranRef];
                        bankStmt.TranAmount = Convert.ToDecimal(csv[(int)WbcCsvFieldOrds.TranAmount]);
                        bankStmt.CounterCcy = map.Account.Currency;
                        if (map.Account != null)
                        {
                            // TODO: Implement line number with SequentialNumber
                            bankStmt.OraTrxNum = string.Format("{0:yyyyMMdd}-{1}-{2}",
                               bankStmt.TranDate,
                               map.Account.BankAccountNumber.Right(3),
                               accountLineNums[map.Account].ToString().PadLeft(3, '0'));
                            accountLineNums[map.Account]++;
                        }
                        bankStmt.Save();
                    }
                    objSpace.CommitChanges();
                }
            }

            private static DateTime ConvertWbcDateTime(string value)
            {
                var year = Convert.ToInt32(value.Substring(0, 4));
                var month = Convert.ToInt32(value.Substring(4, 2));
                var day = Convert.ToInt32(value.Substring(6, 2));
                return new DateTime(year, month, day);
            }
        }
        #endregion

        #region CBA
        public class CbaCsvSchema
        {
            public string TargetAccount;

            public CbaCsvSchema(string targetAccount)
            {
                TargetAccount = targetAccount;
            }

            public enum CbaCsvFieldOrds
            {
                TranDate = 0,
                TranDescription = 1,
                DebitAmount = 3,
                CreditAmount = 4,
                Balance = 5
            }

            public void ImportStream(RequestManager request, MemoryStream stream, bool hasHeaders, XPObjectSpace objSpace)
            {
                using (var csv = new CsvReader(new StreamReader(stream), hasHeaders))
                {
                    var account = objSpace.FindObject<Account>(Account.Fields.Name == TargetAccount);
                    if (account == null)
                        throw new UserFriendlyException(string.Format(
                            "Account {0} was not found. Please add it to the Account objects.", TargetAccount));
                    int accountLineNums = 1;

                    while (csv.ReadNextRecord())
                    {
                        if (request.CancellationTokenSource.IsCancellationRequested)
                            request.CancellationTokenSource.Token.ThrowIfCancellationRequested();

                        BankStmt bankStmt = objSpace.CreateObject<BankStmt>();

                        // TODO: Implement line number with SequentialNumber

                        bankStmt.Account = account;

                        bankStmt.TranDate = Convert.ToDateTime(csv[(int)CbaCsvFieldOrds.TranDate]);
                        bankStmt.TranDescription = csv[(int)CbaCsvFieldOrds.TranDescription];

                        string credit = Convert.ToString(csv[(int)CbaCsvFieldOrds.CreditAmount]);
                        string debit = Convert.ToString(csv[(int)CbaCsvFieldOrds.DebitAmount]);

                        bankStmt.TranAmount = (decimal)(string.IsNullOrEmpty(credit) ? 0 : Convert.ToDecimal(credit))
                                                - (decimal)(string.IsNullOrEmpty(debit) ? 0 : Convert.ToDecimal(debit));
                        bankStmt.CounterCcy = account.Currency;
                        if (account != null)
                        {
                            // TODO: Implement line number with SequentialNumber
                            bankStmt.OraTrxNum = string.Format("{0:yyyyMMdd}-{1}-{2}",
                               bankStmt.TranDate,
                               account.BankAccountNumber.Right(3),
                               accountLineNums.ToString().PadLeft(3, '0'));
                            accountLineNums++;
                        }
                        bankStmt.Save();
                    }
                    objSpace.CommitChanges();
                }
            }
        }

        #endregion

        protected void AcceptAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var request = new RequestManager(Application);

            var paramObj = (ImportBankStmtParam)View.CurrentObject;
            if (paramObj.File.Content == null)
                throw new UserFriendlyException("No file was selected to upload.");
            var byteArray = paramObj.File.Content;
            var stream = new MemoryStream(byteArray);
            var objSpace = (XPObjectSpace)Application.CreateObjectSpace();

            Action job = null;
            if (paramObj.ImportType == BankStmtImportType.ANZ)
            {
                job = new Action(() =>
                {
                    var schema = new AnzCsvSchema();
                    schema.ImportStream(request, stream, false, objSpace);
                });
            }
            else if (paramObj.ImportType == BankStmtImportType.WBC)
            {
                job = new Action(() =>
                {
                    var schema = new WbcCsvSchema();
                    schema.ImportStream(request, stream, true, objSpace);
                });
            }
            else if (paramObj.ImportType == BankStmtImportType.CBABOS)
            {
                job = new Action(() =>
                {
                    var schema = new CbaCsvSchema("VHA CBA BOS");
                    schema.ImportStream(request, stream, true, objSpace);
                });
            }
            else if (paramObj.ImportType == BankStmtImportType.CBAOP)
            {
                job = new Action(() =>
                {
                    var schema = new CbaCsvSchema("VHA CBA OP");
                    schema.ImportStream(request, stream, true, objSpace);
                });
            }
            request.SubmitRequest("Import Bank Stmt CSV File", job);
            View.Close();
        }

    }
}
