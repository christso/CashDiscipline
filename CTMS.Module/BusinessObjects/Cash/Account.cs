using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace CTMS.Module.BusinessObjects.Cash
{
    [ModelDefault("IsCloneable", "True")]
    [ImageName("BO_List")]
    [NavigationItem("Cash Setup")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [ModelDefault("IsFooterVisible", "True")]
    public class Account : BaseObject
    {
        public Account(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
            Currency = Session.GetObjectByKey<Currency>(SetOfBooks.CachedInstance.FunctionalCurrency.Oid);
        }

        private Account _FixAccount;
        private Currency _Currency;
        private string _BankAccountNumber;
        private int _Id;
        private string _Name;
        private bool _IsActive;

        [VisibleInListView(true)]
        public int Id
        {
            get { return _Id; }
            set { SetPropertyValue<int>("Id", ref _Id, value); }
        }

        //[Indexed(Name = @"IX_Account", Unique = true)]
        [Size(200)]
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                SetPropertyValue<string>("Name", ref _Name, value);
            }
        }

        public string BankAccountNumber
        {
            get
            {
                return _BankAccountNumber;
            }
            set
            {
                SetPropertyValue("BankAccountNumber", ref _BankAccountNumber, value);
            }
        }

        private string _UltimateOwner;
        public string UltimateOwner
        {
            get
            {
                return _UltimateOwner;
            }
            set
            {
                SetPropertyValue("UltimateOwner", ref _UltimateOwner, value);
            }
        }

        private string _Owner;
        public string Owner
        {
            get
            {
                return _Owner;
            }
            set
            {
                SetPropertyValue("Owner", ref _Owner, value);
            }
        }

        private string _BankNickName;
        public string BankNickName
        {
            get
            {
                return _BankNickName;
            }
            set
            {
                SetPropertyValue("BankNickName", ref _BankNickName, value);
            }
        }

        private string _BankGroup;
        public string BankGroup
        {
            get
            {
                return _BankGroup;
            }
            set
            {
                SetPropertyValue("BankGroup", ref _BankGroup, value);
            }
        }

        [Association("Currency-Accounts")]
        [RuleRequiredField("Account.Currency_RuleRequiredField", DefaultContexts.Save)]
        public Currency Currency
        {
            get
            {
                return _Currency;
            }
            set
            {
                SetPropertyValue("Currency", ref _Currency, value);
            }
        }

        public Account FixAccount
        {
            get
            {
                return _FixAccount;
            }
            set
            {
                SetPropertyValue("FixAccount", ref _FixAccount, value);
            }
        }

        public bool IsActive
        {
            get
            {
                return _IsActive;
            }
            set
            {
                SetPropertyValue("IsActive", ref _IsActive, value);
            }
        }

        private bool _IsCash;
        public bool IsCash
        {
            get
            {
                return _IsCash;
            }
            set
            {
                SetPropertyValue("IsCash", ref _IsCash, value);
            }
        }


        private bool _IsAtCall;
        public bool IsAtCall
        {
            get
            {
                return _IsAtCall;
            }
            set
            {
                SetPropertyValue("IsAtCall", ref _IsAtCall, value);
            }
        }

        public new class Fields
        {
            public static OperandProperty Name
            {
                get { return new OperandProperty("Name"); }
            }
            public static OperandProperty GlCode
            {
                get { return new OperandProperty("GlCode"); }
            }

        }


    }

}