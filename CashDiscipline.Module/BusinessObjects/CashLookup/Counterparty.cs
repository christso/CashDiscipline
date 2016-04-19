using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;

namespace CashDiscipline.Module.BusinessObjects.Cash
{
    [ImageName("BO_List")]
    [NavigationItem("Cash Setup")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [ModelDefault("IsFooterVisible", "True")]
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

        private CounterpartyTag _CounterpartyL1;
        public CounterpartyTag CounterpartyL1
        {
            get
            {
                return _CounterpartyL1;
            }
            set
            {
                SetPropertyValue("CounterpartyL1", ref _CounterpartyL1, value);
            }
        }

        private CounterpartyTag _CounterpartyL2;

        public CounterpartyTag CounterpartyL2
        {
            get
            {
                return _CounterpartyL2;
            }
            set
            {
                SetPropertyValue("CounterpartyL2", ref _CounterpartyL2, value);
            }
        }
    }

}