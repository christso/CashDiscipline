using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp;

namespace CashDiscipline.Module.BusinessObjects.Cash
{
    [DeferredDeletion(false)]
    [ModelDefault("IsFooterVisible", "True")]
    [ModelDefault("ImageName", "BO_List")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    public class RevalDates : BaseObject
    {
        public RevalDates(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        DateTime fPeriodDate;
        [Indexed(Name = @"iPeriodDate_RevalDates", Unique = true)]
        [DbType("date")]
        [RuleUniqueValue("RevalDates_PeriodDate_RuleUniqueValue", DefaultContexts.Save)]
        public DateTime PeriodDate
        {
            get { return fPeriodDate; }
            set { SetPropertyValue<DateTime>("PeriodDate", ref fPeriodDate, value); }
        }
    }
}
