using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace CTMS.Module.BusinessObjects.Cash
{
    [ModelDefault("IsCloneable", "True")]
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
        private Bank _Bank;
        private bool _IsBankAccount;
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

        [Association(@"Account-BankStmt", typeof(BankStmt))]
        public XPCollection<BankStmt> BankStmt { get { return GetCollection<BankStmt>("BankStmt"); } }

        [Association("Account-CashFlows")]
        public XPCollection<CashFlow> CashFlows
        {
            get
            {
                return GetCollection<CashFlow>("CashFlows");
            }
        }

        public bool IsBankAccount
        {
            get
            {
                return _IsBankAccount;
            }
            set
            {
                SetPropertyValue("IsBankAccount", ref _IsBankAccount, value);
            }
        }

        [ModelDefault("LookupProperty", "ShortName")]
        public Bank Bank
        {
            get
            {
                return _Bank;
            }
            set
            {
                SetPropertyValue("Bank", ref _Bank, value);
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