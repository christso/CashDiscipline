using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;

namespace CTMS.Module.BusinessObjects.Cash
{
    public class Counterparty : BaseObject
    {
        public Counterparty(Session session)
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

        int _Id;
        public int Id
        {
            get { return _Id; }
            set { SetPropertyValue<int>("Id", ref _Id, value); }
        }

        // Fields...
        private string _Name;
        // [Indexed(Name = @"IX_Counterparty", Unique = true)]
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

        private Counterparty _FixCounterparty;
        public Counterparty FixCounterparty
        {
            get
            {
                return _FixCounterparty;
            }
            set
            {
                SetPropertyValue("FixCounterparty", ref _FixCounterparty, value);
            }
        }

        [Association(@"Counterparty-BankStmt", typeof(BankStmt))]
        public XPCollection<BankStmt> BankStmt { get { return GetCollection<BankStmt>("BankStmt"); } }

        [Association("Counterparty-CashFlows")]
        public XPCollection<CashFlow> CashFlows
        {
            get
            {
                return GetCollection<CashFlow>("CashFlows");
            }
        }

    }

}