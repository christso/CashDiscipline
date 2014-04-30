using System;
using System.ComponentModel;

using DevExpress.Xpo;
using DevExpress.Data.Filtering;

using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;

using CTMS.Module.BusinessObjects.Cash;

namespace CTMS.Module.BusinessObjects.Artf
{
    [RuleCriteria("GL Debit", DefaultContexts.Save, "Not ([GlDebitGlCode] Is Not Null And [GlDebitBaccountCustType] Is Not Null)",
        "No more than one GL Debit is allowed.", SkipNullOrEmptyValues = false)]
    [RuleCriteria("GL Credit", DefaultContexts.Save, "Not ([GlCreditGlCode] Is Not Null And [GlCreditBaccountCustType] Is Not Null)",
        "No more than one GL Credit is allowed.", SkipNullOrEmptyValues = false)]
    public class ArtfReconTaskMap : BaseObject
    {
        public ArtfReconTaskMap(Session session)
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
        }
        // Fields...
        private bool _IsBankTfr;
        private Account _TfrToBankAccount;
        private Account _TfrFromBankAccount;
        private ArtfCustomerType _GlDebitBaccountCustType;
        private ArtfCustomerType _GlCreditBaccountCustType;
        private string _Description;
        private string _Comments;
        private ArtfGlCode _GlDebitGlCode;
        private ArtfGlCode _GlCreditGlCode;
        private bool _CanFromLedger;
        private bool _CanToLedger;
        private ArtfCustomerType _ToCustomerType;
        private ArtfCustomerType _FromCustomerType;

        [ToolTip("Customer Type that incorrectly received the amount")]
        public ArtfCustomerType FromCustomerType
        {
            get
            {
                return _FromCustomerType;
            }
            set
            {
                SetPropertyValue("FromCustomerType", ref _FromCustomerType, value);
            }
        }

        [ToolTip("Customer Type that will receive the amount")]
        public ArtfCustomerType ToCustomerType
        {
            get
            {
                return _ToCustomerType;
            }
            set
            {
                SetPropertyValue("ToCustomerType", ref _ToCustomerType, value);
            }
        }

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [ToolTip("Whether this is a GL Journal")]
        public bool IsGlJournal
        {
            get
            {
                return (GlCreditGlCode != null && GlDebitGlCode != null && GlCreditBaccountCustType == null && GlDebitBaccountCustType == null)
                    || (GlCreditGlCode != null && GlDebitGlCode == null && GlCreditBaccountCustType == null && GlDebitBaccountCustType != null)
                    || (GlCreditGlCode == null && GlDebitGlCode != null && GlCreditBaccountCustType != null && GlDebitBaccountCustType == null);
            }
        }

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [ToolTip("Whether the GL Journal is entered correctly")]
        public bool IsGlJournalValid
        {
            get
            {
                bool isValid = IsGlJournal;
                if (!(GlCreditGlCode == null && GlDebitGlCode != null && GlCreditBaccountCustType != null && GlDebitBaccountCustType == null))
                    isValid = false;
                return isValid;
            }
        }

        [ToolTip("Default GL Code to be credited")]
        public ArtfGlCode GlCreditGlCode
        {
            get
            {
                return _GlCreditGlCode;
            }
            set
            {
                SetPropertyValue("GlCreditGlCode", ref _GlCreditGlCode, value);
            }
        }
        [ToolTip("Default GL Code to be debited")]
        public ArtfGlCode GlDebitGlCode
        {
            get
            {
                return _GlDebitGlCode;
            }
            set
            {
                SetPropertyValue("GlDebitGlCode", ref _GlDebitGlCode, value);
            }
        }
        [ToolTip("Default Bank Account for the Transfer From Customer Type")]
        public ArtfCustomerType GlCreditBaccountCustType
        {
            get
            {
                return _GlCreditBaccountCustType;
            }
            set
            {
                SetPropertyValue("GlCreditBaccountCustType", ref _GlCreditBaccountCustType, value);
            }
        }
        [ToolTip("Default Bank Account for the Transfer To Customer Type")]
        public ArtfCustomerType GlDebitBaccountCustType
        {
            get
            {
                return _GlDebitBaccountCustType;
            }
            set
            {
                SetPropertyValue("GlDebitBaccountCustType", ref _GlDebitBaccountCustType, value);
            }
        }

        [ToolTip("Whether a bank transfer is required")]
        public bool IsBankTfr
        {
            get
            {
                return _IsBankTfr;
            }
            set
            {
                SetPropertyValue("IsBankTfr", ref _IsBankTfr, value);
            }
        }
        [ToolTip("Bank account transferring the funds is always set to this value")]
        public Account TfrFromBankAccount
        {
            get
            {
                return _TfrFromBankAccount;
            }
            set
            {
                SetPropertyValue("TfrFromBankAccount", ref _TfrFromBankAccount, value);
            }
        }

        [ToolTip("Bank account receiving the transferred funds is always set to this value")]
        public Account TfrToBankAccount
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
        [ToolTip("Whether a sub-ledger entry is preferred over a general-ledger entry for the transfer orgination")]
        public bool CanFromLedger
        {
            get
            {
                return _CanFromLedger;
            }
            set
            {
                SetPropertyValue("CanFromLedger", ref _CanFromLedger, value);
            }
        }
        [ToolTip("Whether a sub-ledger entry is preferred over a general-ledger entry for the transfer destination")]
        public bool CanToLedger
        {
            get
            {
                return _CanToLedger;
            }
            set
            {
                SetPropertyValue("CanToLedger", ref _CanToLedger, value);
            }
        }
        [ToolTip("Description for your own reference")]
        [Size(SizeAttribute.Unlimited)]
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                SetPropertyValue("Description", ref _Description, value);
            }
        }
        [ToolTip("Comments for the general task")]
        [Size(SizeAttribute.Unlimited)]
        public string Comments
        {
            get
            {
                return _Comments;
            }
            set
            {
                SetPropertyValue("Comments", ref _Comments, value);
            }
        }


    }

}
