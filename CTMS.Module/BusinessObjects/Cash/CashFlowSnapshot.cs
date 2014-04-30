using System;
using System.Linq;
using System.Text;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using System.Collections.Generic;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;

namespace CTMS.Module.BusinessObjects.Cash
{
    [DefaultProperty("Name")]
    public class CashFlowSnapshot : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public CashFlowSnapshot(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            TimeCreated = DateTime.Now;
        }

        private DateTime _TimeCreated;
        private DateTime _FromDate;

        private string _Name;

        public DateTime TimeCreated
        {
            get
            {
                return _TimeCreated;
            }
            set
            {
                SetPropertyValue("TimeCreated", ref _TimeCreated, value);
            }
        }

        [RuleUniqueValue("CashFlowSnapshot_Name_RuleUniqueValue", DefaultContexts.Save)]
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                SetPropertyValue("Name", ref _Name, value);
            }
        }

        public DateTime FromDate
        {
            get
            {
                return _FromDate;
            }
            set
            {
                SetPropertyValue("FromDate", ref _FromDate, value);
            }
        }

        [Association("CashFlowSnapshot-CashFlows"), DevExpress.Xpo.Aggregated]
        public XPCollection<CashFlow> CashFlows
        {
            get
            {
                return GetCollection<CashFlow>("CashFlows");
            }
        }

        protected override void OnDeleting()
        {
            base.OnDeleting();
        }

        public static class FieldNames
        {
            public const string Oid = "Oid";
        }
    }
}
