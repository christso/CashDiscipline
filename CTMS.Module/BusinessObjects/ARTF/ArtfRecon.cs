using System;
using System.ComponentModel;

using DevExpress.Xpo;
using DevExpress.Data.Filtering;

using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp.DC;
using GenerateUserFriendlyId.Module.BusinessObjects;
using DevExpress.ExpressApp.Security.Strategy;

using DevExpress.ExpressApp.Model;
using Cash = CTMS.Module.BusinessObjects.Cash;

namespace CTMS.Module.BusinessObjects.Artf
{
    [VisibleInReports(true)]
    [RuleCriteria("ArtfRecon_TransferFromToRule", DefaultContexts.Save, "CustomerType != BankStmt.Account.ArtfCustomerType",
        "Transfer From and Transfer To must be of different Customer Type", SkipNullOrEmptyValues=false)]
    [RuleCriteria("ArtfRecon_AmountRule", DefaultContexts.Save, "Amount > 0.00M",
        "Amount must be greater than zero", SkipNullOrEmptyValues=false)]
    //[RuleCriteria("Recon Ledger Rule", DefaultContexts.Save,
    // if ArtfTasks.
    public class ArtfRecon : UserFriendlyIdPersistentObject
    {
        public ArtfRecon(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here or place it only when the IsLoading property is false:
            // if (!IsLoading){
            //    It is now OK to place your initialization code here.
            // }
            // or as an alternative, move your initialization code into the AfterConstruction method.

        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
            ReconDate = DateTime.Now;
            SecuritySystemUser currentUser = D2NXAF.ExpressApp.StaticHelpers.GetCurrentUser(Session);
            if (currentUser != null)
                    this.CreatedBy = currentUser;
        }

        [PersistentAlias("concat('R', ToStr(SequentialNumber))")]
        public string ReconId
        {
            get
            {
                return Convert.ToString(EvaluateAlias("ReconId"));
            }
        }

        private CTMS.Module.BusinessObjects.Cash.Account _TfrToBankAccount;
        DateTime _ReconDate;
        public DateTime ReconDate
        {
            get { return _ReconDate; }
            set { SetPropertyValue("ReconDate", ref _ReconDate, value); }
        }

        decimal _Amount;
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        [RuleRequiredField("RuleRequiredField for ArtfRecon.Amount", DefaultContexts.Save)]
        public decimal Amount
        {

            get { return _Amount; }
            set
            {
                SetPropertyValue("Amount", ref _Amount, value);
                if (!IsLoading && !IsSaving)
                {
                    
                    // update ReconTotal in Master objects
                    if (BankStmt != null)
                        BankStmt.UpdateReconTotal(true);
                    if (FromLedger != null)
                        FromLedger.UpdateFromReconTotal(true);
                    if (FromReceipt != null)
                        FromReceipt.UpdateFromReconTotal(true);
                }
            }
        }
        ArtfCustomerType _CustomerType;
        [ImmediatePostData(true)]
        [RuleRequiredField("RuleRequiredField for CustomerType.Name", DefaultContexts.Save)]
        [Association(@"ArtfCustomerType-ArtfRecons")]
        public ArtfCustomerType CustomerType
        {
            get { return _CustomerType; }
            set { 
                SetPropertyValue<ArtfCustomerType>("CustomerType", ref _CustomerType, value);
            }
        }

        [DataSourceProperty("CustomerType.Cash_Accounts")]
        [Association("BankAccount-ArtfRecons")]
        [VisibleInListView(false)]
        [ModelDefault("LookupProperty", "Name")]
        public CTMS.Module.BusinessObjects.Cash.Account TfrToBankAccount
        {
            get
            {
                return _TfrToBankAccount;
            }
            set
            {
                SetPropertyValue("TfrToBankAccount", ref _TfrToBankAccount, value);
            }
        }

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [ModelDefault("LookupProperty", "Name")]
        public CTMS.Module.BusinessObjects.Cash.Account TfrToBankAccountFinal
        {
            get
            {
                // get default bank account
                var defaultBankAcc = Session.FindObject<Cash.Account>(Cash.Account.OperandProperties.IsArtfDefault == new OperandValue(true)
                    & Cash.Account.OperandProperties.ArtfCustomerType == CustomerType);
                return TfrToBankAccount == null ? defaultBankAcc : TfrToBankAccount;
            }
        }
        string fCustomerAccount;
        [VisibleInListView(false)]
        public string CustomerAccount
        {
            get { return fCustomerAccount; }
            set { SetPropertyValue<string>("CustomerAccount", ref fCustomerAccount, value); }
        }
        string fCustomerInvoice;
        [VisibleInListView(false)]
        public string CustomerInvoice
        {
            get { return fCustomerInvoice; }
            set { SetPropertyValue<string>("CustomerInvoice", ref fCustomerInvoice, value); }
        }
        string _Comments;
        [VisibleInListView(false)]
        [Size(SizeAttribute.Unlimited)]
        public string Comments
        {
            get { return _Comments; }
            set { SetPropertyValue<string>("Comments", ref _Comments, value); }
        }

        #region BankStmt

        // Recon Total
        Cash.BankStmt _BankStmt;
        [DevExpress.Xpo.DisplayName("BS Id")]
        [RuleRequiredField("RuleRequiredField for BankStmt", DefaultContexts.Save)]
        [Association(@"BankStmt-ArtfRecons")]
        public Cash.BankStmt BankStmt
        {
            get { return _BankStmt; }
            set
            {
                Cash.BankStmt oldBankStmt = _BankStmt;
                SetPropertyValue("BankStmt", ref _BankStmt, value);
                if (!IsLoading && !IsSaving && oldBankStmt != _BankStmt)
                {
                    oldBankStmt = oldBankStmt ?? _BankStmt;
                    oldBankStmt.UpdateReconTotal(true);
                }
            }
        }
        #endregion

        #region FromLedger
        ArtfLedger _FromLedger;
        [Association(@"ArtfFromLedger-ArtfRecons")]
        [EditorAlias("D2NLookupPropertyEditor")]
        [ImmediatePostData(true)]
        [VisibleInListView(false)]
        [ModelDefault("LookupProperty", "LedgerId")]
        public ArtfLedger FromLedger
        {
            get { return _FromLedger; }
            set
            {
                ArtfLedger oldLedger = _FromLedger;
                SetPropertyValue("Ledger", ref _FromLedger, value);
                if (!IsLoading && !IsSaving && oldLedger != _FromLedger)
                {
                    oldLedger = oldLedger ?? _FromLedger;
                    oldLedger.UpdateFromReconTotal(true);
                }
            }
        }
        //[VisibleInListView(false)]
        //public decimal? FromLedgerAmount
        //{
        //    get
        //    {
        //        if (_FromLedger == null) return null;
        //        return _FromLedger.Amount;
        //    }
        //}
        //[VisibleInListView(false)]
        //public decimal? FromLedgerUnreconTotal
        //{             
        //    get 
        //    {
        //        if (_FromLedger == null)
        //            return null;
        //        return _FromLedger.Amount - _FromLedger.FromReconTotal; 
        //    }
        //}
        #endregion

        #region FromReceipt
        ArtfReceipt _FromReceipt;
        [VisibleInListView(false)]
        [Association(@"ArtfFromReceipt-ArtfRecons")]
        [ModelDefault("LookupProperty", "ReceiptAlias")]
        public ArtfReceipt FromReceipt
        {
            get { return _FromReceipt; }
            set
            {
                ArtfReceipt oldReceipt = _FromReceipt;
                SetPropertyValue("Receipt", ref _FromReceipt, value);
                if (!IsLoading && !IsSaving && oldReceipt != _FromReceipt)
                {
                    oldReceipt = oldReceipt ?? _FromReceipt;
                    oldReceipt.UpdateFromReconTotal(true);
                }
            }
        }
        //[VisibleInListView(false)]
        //public decimal? FromReceiptAmount
        //{
        //    get
        //    {
        //        if (_FromReceipt == null)
        //            return null;
        //        return _FromReceipt.Amount;
        //    }
        //}
        //[VisibleInListView(false)]
        //public decimal? FromReceiptUnreconTotal
        //{
        //    get 
        //    {
        //        if (_FromReceipt == null)
        //            return null;
        //        return _FromReceipt.Amount - _FromReceipt.FromReconTotal; 
        //    }
        //}
        #endregion

        #region ToLedger
        ArtfLedger _ToLedger;
        [Association(@"ArtfToLedger-ArtfRecons")]
        [EditorAlias("D2NLookupPropertyEditor")]
        [ImmediatePostData(true)]
        [VisibleInListView(false)]
        [ModelDefault("LookupProperty", "LedgerId")]
        public ArtfLedger ToLedger
        {
            get { return _ToLedger; }
            set
            {
                ArtfLedger oldLedger = _ToLedger;
                SetPropertyValue("Ledger", ref _ToLedger, value);
                if (!IsLoading && !IsSaving && oldLedger != _ToLedger)
                {
                    oldLedger = oldLedger ?? _ToLedger;
                    oldLedger.UpdateFromReconTotal(true);
                    UpdateCustomerType(); // default Customer Type when it's null
                }
            }
        }
        //[VisibleInListView(false)]
        //[ModelDefault("EditMask", "n2")]
        //[ModelDefault("DisplayFormat", "n2")]
        //public decimal? ToLedgerAmount
        //{
        //    get
        //    {
        //        if (_ToLedger == null) return null;
        //        return _ToLedger.Amount;
        //    }
        //}
        //[VisibleInListView(false)]
        //[ModelDefault("EditMask", "n2")]
        //[ModelDefault("DisplayFormat", "n2")]
        //public decimal? ToLedgerUnreconTotal
        //{
        //    get
        //    {
        //        if (_ToLedger == null)
        //            return null;
        //        return _ToLedger.Amount - _ToLedger.FromReconTotal;
        //    }
        //}
        #endregion

        #region ToReceipt
        ArtfReceipt _ToReceipt;
        [VisibleInListView(false)]
        [ModelDefault("LookupProperty", "ReceiptAlias")]
        [Association(@"ArtfToReceipt-ArtfRecons")]
        public ArtfReceipt ToReceipt
        {
            get { return _ToReceipt; }
            set
            {
                ArtfReceipt oldReceipt = _ToReceipt;
                SetPropertyValue("Receipt", ref _ToReceipt, value);
                if (!IsLoading && !IsSaving && oldReceipt != _ToReceipt)
                {
                    oldReceipt = oldReceipt ?? _ToReceipt;
                    oldReceipt.UpdateFromReconTotal(true);
                }
            }
        }
        //[VisibleInListView(false)]
        //public decimal? ToReceiptAmount
        //{
        //    get
        //    {
        //        if (_ToReceipt == null)
        //            return null;
        //        return _ToReceipt.Amount;
        //    }
        //}
        //[VisibleInListView(false)]
        //public decimal? ToReceiptUnreconTotal
        //{
        //    get
        //    {
        //        if (_ToReceipt == null)
        //            return null;
        //        return _ToReceipt.Amount - _ToReceipt.FromReconTotal;
        //    }
        //}
        #endregion

        // default Customer Type when it's null
        public void UpdateCustomerType()
        {
            // TBI
        }

        [Association(@"ArtfRecon-ArtfTasks"), DevExpress.Xpo.Aggregated]
        public XPCollection<ArtfTask> ArtfTasks
        {
            get { return GetCollection<ArtfTask>("ArtfTasks"); }
        }

        SecuritySystemUser _CreatedBy;
        [VisibleInListView(false)]
        [ModelDefault("AllowEdit", "False")]
        public SecuritySystemUser CreatedBy
        {
            get
            {
                return _CreatedBy;
            }
            set
            {
                SetPropertyValue("CreatedBy", ref _CreatedBy, value);
            }
        }

    }

}
