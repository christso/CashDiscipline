using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using CTMS.Module.BusinessObjects.Cash;
using DevExpress.Persistent.Validation;

namespace CTMS.Module.BusinessObjects
{
    [DefaultClassOptions]
    public class Currency : BaseObject
    {
        public Currency(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }

        int fId;
        public int Id
        {
            get { return fId; }
            set { SetPropertyValue<int>("Id", ref fId, value); }
        }

        // Fields...
        private string _Name;
        // [Indexed(Name = @"IX_Currency", Unique = true)]
        [RuleUniqueValue("Curency_Name_RuleUniqueValue", DefaultContexts.Save)]
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

        [Association(@"CounterCcy-BankStmt", typeof(BankStmt))]
        [MemberDesignTimeVisibility(false)]
        public XPCollection<BankStmt> BankStmt { get { return GetCollection<BankStmt>("BankStmt"); } }

        [Association("Currency-Accounts")]
        [MemberDesignTimeVisibility(false)]
        public XPCollection<Account> Accounts
        {
            get
            {
                return GetCollection<Account>("Accounts");
            }
        }

        [Association("Currency-CashFlows")]
        [MemberDesignTimeVisibility(false)]
        public XPCollection<CashFlow> CashFlows
        {
            get
            {
                return GetCollection<CashFlow>("CashFlows");
            }
        }

        public class FieldNames
        {
            public static string Oid = "Oid";
            public static string Name = "Name";
        }
    }

}