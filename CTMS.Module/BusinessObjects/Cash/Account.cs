using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using CTMS.Module.BusinessObjects.Artf;
using CTMS.Module.BusinessObjects.Payments;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Validation;

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
        private bool _IsArtfDefault;
        private ArtfGlCode _GlCode;
        private ArtfCustomerType _ArtfCustomerType;
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

        public ArtfGlCode GlCode
        {
            get
            {
                return _GlCode;
            }
            set
            {
                SetPropertyValue("GlCode", ref _GlCode, value);
            }
        }

        [Association("ArtfCustomerType-Cash_Accounts")]
        public ArtfCustomerType ArtfCustomerType
        {
            get
            {
                return _ArtfCustomerType;
            }
            set
            {
                SetPropertyValue("ArtfCustomerType", ref _ArtfCustomerType, value);
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
        
        [Association("BankAccount-ArtfRecons")]
        public XPCollection<ArtfRecon> ArtfRecon
        {
            get
            {
                return GetCollection<ArtfRecon>("ArtfRecon");
            }
        }

        public bool IsArtfDefault
        {
            get
            {
                return _IsArtfDefault;
            }
            set
            {
                SetPropertyValue("IsArtfDefault", ref _IsArtfDefault, value);
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
                if (!IsLoading && !IsSaving)
                {
                    var creditAccount = Session.FindObject<CreditAccount>(new OperandProperty("LinkedAccount") == this);
                    if (value)
                    {
                        if (creditAccount == null)
                            creditAccount = new CreditAccount(Session);
                        creditAccount.ShortName = this.Name;
                        creditAccount.LinkedAccount = this;
                        creditAccount.BankAccountNumber = this.BankAccountNumber;
                    }
                    else
                    {
                        if (creditAccount != null)
                            creditAccount.LinkedAccount = null;
                    }
                }
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

        public class OperandProperties
        {
            public static OperandProperty IsArtfDefault
            {
                get { return new OperandProperty("IsArtfDefault"); }
            }
            public static OperandProperty ArtfCustomerType
            {
                get { return new OperandProperty("ArtfCustomerType"); }
            }
            public static OperandProperty GlCode
            {
                get { return new OperandProperty("GlCode"); }
            }

        }

        //public Activity ActivityMap
        //{
        //    get
        //    {
        //        return _ActivityMap;
        //    }
        //    set
        //    {
        //        if (_ActivityMap == value)
        //            return;

        //        Activity prevActivityMap = _ActivityMap;
        //        _ActivityMap = value;

        //        if (IsLoading) return;

        //        if (prevActivityMap != null && prevActivityMap.AccountMap == this)
        //            prevActivityMap.AccountMap = null;

        //        if (_ActivityMap != null)
        //            _ActivityMap.AccountMap = this;
        //        OnChanged("ActivityMap");
        //    }
        //}

    }

}