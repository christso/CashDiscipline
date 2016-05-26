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
using CashDiscipline.Module.ParamObjects.FinAccounting;

namespace CashDiscipline.Module.BusinessObjects.FinAccounting
{
    [ModelDefault("ImageName", "BO_List")]
    [DefaultProperty("Name")]
    public class FinJournalGroup : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public FinJournalGroup(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
        }
        private string _Name;

        [RuleUniqueValue("JournalGroup_RuleUniqueValue", DefaultContexts.Save)]
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

        [Association("JournalGroup-FinActivities")]
        public XPCollection<FinActivity> FinActivities
        {
            get
            {
                return GetCollection<FinActivity>("FinActivities");
            }
        }

        [Association("JournalGroup-FinAccounts")]
        public XPCollection<FinAccount> FinAccounts
        {
            get
            {
                return GetCollection<FinAccount>("FinAccounts");
            }
        }
        
        protected override void OnSaved()
        {
            base.OnSaved();
            FinJournalGroupParam.SyncParamObjects(Session);
        }
    }
}
