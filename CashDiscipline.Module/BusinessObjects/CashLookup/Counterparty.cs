using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using CashDiscipline.Module.Logic.SqlServer;
using DevExpress.Persistent.Validation;

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
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            var sqlUtil = new SqlQueryUtil(Session);
            DateTimeCreated = sqlUtil.GetDate();
        }

        private string _Name;
        [Indexed(Name = @"iName_Counterparty", Unique = false)]
        [RuleUniqueValue("Counterparty_Name_RuleUniqueValue", DefaultContexts.Save)]
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

        private string _CounterpartyL1;
        public string CounterpartyL1
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

        private string _CounterpartyL2;

        public string CounterpartyL2
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

        private DateTime _DateTimeCreated;

        public DateTime DateTimeCreated
        {
            get
            {
                return _DateTimeCreated;
            }
            set
            {
                SetPropertyValue("DateTimeCreated", ref _DateTimeCreated, value);
            }
        }
    }

}