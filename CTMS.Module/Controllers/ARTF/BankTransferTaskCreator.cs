using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Editors;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;

using CTMS.Module.BusinessObjects.Artf;
using CTMS.Module.BusinessObjects.Cash;
using CTMS.Module.BusinessObjects;
using DevExpress.Data.Filtering;


namespace CTMS.Module.Controllers.Artf
{

    public class BankTransferTaskCreator
    {
        private Session session;
        private ArtfRecon reconObject;
        private ArtfReconTaskMap taskMap;
        private ArtfFundsTransferTask t;

        public BankTransferTaskCreator(Session session, ArtfRecon reconObject, ArtfReconTaskMap taskMap, ArtfFundsTransferTask task)
        {
            this.session = session;
            this.reconObject = reconObject;
            this.taskMap = taskMap;
            this.t = task;
        }

        // returns true if valid, else false
        public bool Create()
        {
            #region Transfer From
            if (!TryTransferFromBankStmt())
            {
                t.Save();
                return false;
            }
            if (TransferFromMap()) { }
            else if (TransferFromLedger()) { }
            else if (TransferFromBankStmt()) { }

            ResolveTransferFromMismatch();
            #endregion

            #region Transfer To
            if (TransferToTransfer()) { }
            else if (TransferToMap()) { }
            else if (TransferToLedger()) { }
            else if (TransferToCustType()) { }
            else if (TransferToCustTypeFirst()) { }
            else
            {
                SetInvalidTask();
                t.Save();
                return false;
            }
            ResolveTransferToMismatch();
            #endregion

            t.Save();
            return true;
        }

        #region Transfer From

        private bool TryTransferFromBankStmt()
        {
            if (t.ArtfRecon.BankStmt == null && t.ArtfRecon.BankStmt.Account == null)
            {
                t.Status = TaskStatusEnum.Invalid;
                t.Comments = "Error: Task is marked 'Is Bank Transfer' but the Reconciliation object "
                    + "is not associated with a Bank Statement object or the Bank Statement Account is null.";
                return false;
            }
            return true;
        }

        // returns True if Transfer is created
        private bool TransferFromBankStmt()
        {
            // get TfrFromBankAccount from the associated bank statement object
            t.TfrFromBankAccount = reconObject.BankStmt.Account;
            return true;
        }

        // returns True if Transfer is created
        private bool TransferFromMap()
        {
            if (taskMap.TfrFromBankAccount != null)
            {
                t.TfrFromBankAccount = taskMap.TfrFromBankAccount;
                return true;
            }
            return false;
        }

        // returns True if Transfer is created
        private bool TransferFromLedger()
        {
            if (t.ArtfRecon.FromLedger != null)
            {
                // get TfrFromBankAccount from the Ledger Gl Code
                var account = session.FindObject<Account>(Account.OperandProperties.GlCode == t.ArtfRecon.FromLedger.GlCode);
                if (account != null)
                {
                    t.TfrFromBankAccount = account;
                    return true;
                }
                return false;
            }
            return false;
        }

        private bool ResolveTransferFromMismatch()
        {
            if (t.ArtfRecon.FromLedger == null)
            {
                if (t.ArtfRecon.BankStmt.Account.GlCode != t.TfrFromBankAccount.GlCode)
                {
                    if (taskMap.CanFromLedger)
                    {
                        var tFromLedger = new ArtfLedgerTask(session) { ArtfRecon = reconObject };
                        tFromLedger.Amount = t.ArtfRecon.Amount;
                        tFromLedger.GlCode = t.TfrFromBankAccount.GlCode;
                        tFromLedger.Save();
                    }
                    else
                    {
                        // if Account is different between TfrFromBankAccount and the bank statement account
                        // then create a Gl Journal Task 
                        var tFromGlJournal2 = new ArtfGlJournalTask(session) { ArtfRecon = reconObject };
                        // TODO: if TaskMap.DebitGlCode is defined for Transfer From Voda AR?
                        if (taskMap.GlDebitGlCode == null)
                            tFromGlJournal2.GlDebitGlCode = tFromGlJournal2.ArtfRecon.BankStmt.Account.GlCode;
                        else
                            tFromGlJournal2.GlDebitGlCode = taskMap.GlDebitGlCode;
                        if (taskMap.GlCreditGlCode == null)
                            tFromGlJournal2.GlCreditGlCode = t.TfrFromBankAccount.GlCode;
                        else
                            tFromGlJournal2.GlCreditGlCode = taskMap.GlCreditGlCode;
                        tFromGlJournal2.GlNetDebitAmount = tFromGlJournal2.ArtfRecon.Amount;
                        tFromGlJournal2.Comments = "Correct GL Code mis-match "
                            + "between the original bank account that incorrectly received the amount and "
                            + "the bank account that transfers the amount.";
                        tFromGlJournal2.Save();
                    }
                    return true;
                }
                return false;
            }
            if (t.ArtfRecon.FromLedger.GlCode != t.TfrFromBankAccount.GlCode)
            {
                // if Account from Ledger Gl Code is not the t.TfrFromBankAccount
                // such as when TfrFromBankAccount member is set by ReconTaskMap object
                // then create a Gl Journal Task
                var tFromGlJournal1 = new ArtfGlJournalTask(session) { ArtfRecon = reconObject };
                tFromGlJournal1.GlDebitGlCode = tFromGlJournal1.ArtfRecon.FromLedger.GlCode;
                tFromGlJournal1.GlCreditGlCode = t.TfrFromBankAccount.GlCode;
                tFromGlJournal1.GlNetDebitAmount = tFromGlJournal1.ArtfRecon.Amount;
                tFromGlJournal1.Comments = "Correct GL Code mis-match "
                    + "between what is ledgered off and the bank account that transfers the amount.";
                tFromGlJournal1.Save();
                return true;
            }
            return false;
        }
        #endregion

        #region Transfer To
       
        private bool TransferToTransfer()
        {
            if (t.ArtfRecon.TfrToBankAccount != null)
            {
                // use account from user-entry
                t.TfrToBankAccount = t.ArtfRecon.TfrToBankAccount;
                return true;
            }
            return false;
        }
        private bool TransferToMap()
        {
            if (taskMap.TfrToBankAccount != null)
            {
                // use account from task map
                t.TfrToBankAccount = taskMap.TfrToBankAccount;
                return true;
            }
            return false;
        }
        private bool TransferToLedger()
        {
            // get associated bank account for ledger Gl Code
            Account ledgerBankAccount;
            if (t.ArtfRecon.ToLedger != null)
            {
                ledgerBankAccount = session.FindObject<Account>(Account.OperandProperties.GlCode == t.ArtfRecon.ToLedger.GlCode);
                if (ledgerBankAccount != null)
                {
                    // use account for ledger GL Code
                    t.TfrToBankAccount = ledgerBankAccount;
                    return true;
                }
            }
            return false;
        }
        private bool TransferToCustType()
        {
            // get default bank account for the customer type
            Account custTypeBankAccount =
                session.FindObject<Account>(
                Account.OperandProperties.IsArtfDefault == new OperandValue(true)
                & Account.OperandProperties.ArtfCustomerType == t.ArtfRecon.CustomerType);
            if (custTypeBankAccount != null)
            {
                // use default bank account for customer type
                t.TfrToBankAccount = custTypeBankAccount;
                return true;
            }
            return false;
        }
        private bool TransferToCustTypeFirst()
        {
            if (reconObject.CustomerType.Cash_Accounts.Count != 0)
            {
                // use first account that appears for customer type
                t.TfrToBankAccount = reconObject.CustomerType.Cash_Accounts[0];
                return true;
            }
            return false;
        }
        private void SetInvalidTask()
        {
            t.Status = TaskStatusEnum.Invalid;
            t.Comments = "Error: Task is marked 'Is Bank Transfer' but no bank account is of Customer Type '" + reconObject.CustomerType.Name + "'.";
        }
        private bool ResolveTransferToMismatch()
        {
            // if Account from Ledger Gl Code is not the t.TfrToBankAccount
            if (t.ArtfRecon.ToLedger != null && t.ArtfRecon.ToLedger.GlCode != t.TfrToBankAccount.GlCode)
            {
                // create a Gl Journal Task
                var tGlJournal = new ArtfGlJournalTask(session) { ArtfRecon = reconObject };
                tGlJournal.GlDebitGlCode = t.TfrToBankAccount.GlCode;
                tGlJournal.GlCreditGlCode = tGlJournal.ArtfRecon.ToLedger.GlCode;
                tGlJournal.GlNetDebitAmount = tGlJournal.ArtfRecon.Amount;
                tGlJournal.Comments = "Correct GL Code mis-match "
                    + "between what is ledgered off and the bank account that receives the transferred amount.";
                tGlJournal.Save();
                return true;
            }
            return false;
        }
        #endregion
    }
}
