using CTMS.Module.BusinessObjects.Artf;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTMS.Module.BusinessObjects.Cash;

namespace CTMS.Module.Controllers.Artf
{
    // Note: This is only used if TaskMap.IsBankTfr = False
    // Note: that the general Task is not saved by the individual methods.
    // The general task is saved by the Create() method.
    public class GlJournalTaskCreator
    {
        private Session session;
        private ArtfRecon reconObject;
        private ArtfReconTaskMap taskMap;
        private ArtfGlJournalTask t;

        public GlJournalTaskCreator(Session session, ArtfRecon reconObject, ArtfReconTaskMap taskMap, ArtfGlJournalTask task)
        {
            this.session = session;
            this.reconObject = reconObject;
            this.taskMap = taskMap;
            this.t = task;
        }
        public bool Create()
        {
            if (!TryBankAccountGlCode())
            {
                t.Save();
                return false;
            }

            #region Debit
            // Note: isMatched indicates whether a DebitGlCode was found in TaskMap
            bool isDebitMatched = DebitLedger();
            if (!isDebitMatched) isDebitMatched = DebitMap();
            if (!isDebitMatched) isDebitMatched = DebitBankStmt();
            if (!isDebitMatched) isDebitMatched = DebitCustTypeBankAccount();
            if (!isDebitMatched)
            {
                SetInvalidUnmatchedDebitTask();
                return false;
            }
            // TODO: check cases where this is needed
            //ResolveDebitMismatch();
            #endregion

            #region Credit
            bool isCreditMatched = CreditLedger();
            if (!isCreditMatched) isCreditMatched = CreditMap();
            if (!isCreditMatched) isCreditMatched = ReceiptMap();
            if (!isCreditMatched) isCreditMatched = CreditCustTypeBankAccount();
            if (!isCreditMatched)
            {
                SetInvalidUnmatchedCreditTask();
                return false;
            }

            t.GlNetDebitAmount = t.ArtfRecon.Amount;
            t.Save();
            return true;
            #endregion
        }


        private void SetInvalidUnmatchedDebitTask()
        {
            var isBankTfr = taskMap.IsBankTfr;
            var isGlJournal = taskMap.IsGlJournal;
            var isFromLedgered = t.ArtfRecon.FromLedger != null;
            if (!string.IsNullOrEmpty(t.Comments)) t.Comments += "\n";
            t.Comments += "No matching rules for Debit defined in Task Map. You may need to:";
            t.Comments += !isBankTfr ? " set IsBankTfr to True;" : "";
            t.Comments += !isGlJournal ? " specify GL Debits and Credits in Task Map;" : "";
            t.Comments += !isFromLedgered ? " create the missing ledger entry;" : "";
        }
        private void SetInvalidUnmatchedCreditTask()
        {
            var isBankTfr = taskMap.IsBankTfr;
            var isGlJournal = taskMap.IsGlJournal;
            var isToLedgered = t.ArtfRecon.ToLedger != null;
            if (!string.IsNullOrEmpty(t.Comments)) t.Comments += "\n";
            t.Comments += "No matching rules for Credit defined in Task Map. You may need to:";
            t.Comments += !isBankTfr ? " set IsBankTfr to True;" : "";
            t.Comments += !isGlJournal ? " specify GL Debits and Credits in Task Map;" : "";
            t.Comments += !isToLedgered ? " create the missing ledger entry;" : "";
        }

        private bool TryBankAccountGlCode()
        {
            if (t.ArtfRecon.BankStmt == null)
            {
                t.Comments = "No bank statement object associated with the recon object.";
                return false;
            }
            else if (t.ArtfRecon.BankStmt.Account == null)
            {
                t.Comments = "No bank account associated with the recon object.";
                return false;
            }
            else if (t.ArtfRecon.BankStmt.Account.GlCode == null)
            {
                t.Comments = "No bank account GL code associated with the recon object.";
                return false;
            }
            return true;
        }

        #region Debit
        private bool DebitLedger()
        {
            if (t.ArtfRecon.FromLedger != null && t.ArtfRecon.FromLedger.GlCode != null)
            {
                // Compute Debit GL Code from Ledger
                t.GlDebitGlCode = t.ArtfRecon.FromLedger.GlCode;
                return true;
            }
            return false;
        }
        private bool DebitBankStmt()
        {
            if (!taskMap.CanFromLedger && t.ArtfRecon.BankStmt.Account.GlCode != null)
            {
                // Comput Debit GL Code from BankStmt
                t.GlDebitGlCode = t.ArtfRecon.BankStmt.Account.GlCode;
                return true;
            }
            return false;
        }
        private bool DebitMap()
        {
            if (taskMap.GlDebitGlCode != null)
            {
                // Default Debit GL Code 
                // (note that this is mutually exclusive to GlDebitBaccountCustType)
                t.GlDebitGlCode = taskMap.GlDebitGlCode;
                return true;
            }
            return false;
        }
        private bool DebitCustTypeBankAccount()
        {
            if (taskMap.GlDebitBaccountCustType != null)
            {
                // Default Debit GL Code for Bank Account
                // (note that this is mutually exclusive to GlDebitGlCode)
                if (taskMap.GlDebitBaccountCustType == reconObject.BankStmt.Account.ArtfCustomerType)
                    t.GlDebitGlCode = reconObject.TfrToBankAccount.GlCode;
                else if (taskMap.GlDebitBaccountCustType == reconObject.CustomerType)
                    t.GlDebitGlCode = reconObject.BankStmt.Account.GlCode;
                else if (taskMap.GlDebitBaccountCustType.Cash_Accounts.Count != 0)
                    // get debit bank account of first cust type
                    t.GlDebitGlCode = taskMap.GlDebitBaccountCustType.Cash_Accounts[0].GlCode;
                else
                    return false;
                return true;
            }
            return false;
        }
        private bool ResolveDebitMismatch()
        {
            if (t.GlDebitGlCode != t.ArtfRecon.BankStmt.Account.GlCode)
            {
                if (taskMap.CanFromLedger)
                {
                    var tLedger = new ArtfLedgerTask(session) { ArtfRecon = reconObject };
                    tLedger.Amount = t.ArtfRecon.Amount;
                    tLedger.GlCode = t.GlDebitGlCode;
                    tLedger.Save();
                }
                else
                {
                    // create Gl journal to match Gl codes
                    var tGlJournal = new ArtfGlJournalTask(session) { ArtfRecon = reconObject };
                    tGlJournal.GlCreditGlCode = t.GlDebitGlCode;
                    tGlJournal.GlDebitGlCode = t.ArtfRecon.BankStmt.Account.GlCode;
                    tGlJournal.GlNetDebitAmount = t.ArtfRecon.Amount;
                    tGlJournal.Comments = "Correct GL Code mis-match "
                                + "between the original bank account that incorrectly received the amount and "
                                + "the Gl Code that was debited for that bank account.";
                    tGlJournal.Save();
                }
                return true;
            }
            return false;
        }
        #endregion

        #region Credit
        private bool CreditLedger()
        {
            if (t.ArtfRecon.ToLedger != null && t.ArtfRecon.ToLedger.GlCode != null)
            {
                // Compute Credit GL Code from Ledger
                t.GlCreditGlCode = t.ArtfRecon.ToLedger.GlCode;
                return true;
            }
            return false;
        }
        private bool CreditMap()
        {
            if (taskMap.GlCreditGlCode != null)
            {
                // Default Credit GL Code 
                // (note that this is mutually exclusive to GlCreditBaccountCustType)
                t.GlCreditGlCode = taskMap.GlCreditGlCode;
                return true;
            }
            return false;
        }
        private bool ReceiptMap()
        {
            if (t.ArtfRecon.ToReceipt != null)
            {
                var account = t.ArtfRecon.ToReceipt.RemittanceAccount;
                t.GlCreditGlCode = account.GlCode;
                return true;
            }
            return false;
        }
        private bool CreditCustTypeBankAccount()
        {
            if (taskMap.GlCreditBaccountCustType != null)
            {
                // Default Debit GL Code for Bank Account
                // (note that this is mutually exclusive to GlCreditGlCode)
                if (taskMap.GlCreditBaccountCustType.Cash_Accounts.Count != 0)
                {
                    // get credit bank account of first cust type
                    var accounts = taskMap.GlCreditBaccountCustType.Cash_Accounts;
                    var defaultAccount = accounts.First<Account>(
                        delegate(Account x) { return x.IsArtfDefault; });

                    if (defaultAccount != null)
                    {
                        t.GlCreditGlCode = defaultAccount.GlCode;
                    }
                    else
                    {
                        t.GlCreditGlCode = accounts[0].GlCode;
                    }
                }
                else
                    return false;
                return true;
            }
            return false;
        }
        #endregion
    }
}
