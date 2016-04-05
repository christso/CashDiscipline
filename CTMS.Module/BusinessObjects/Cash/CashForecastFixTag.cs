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
    [ImageName("BO_List")]
    [NavigationItem("Cash Setup")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [ModelDefault("IsFooterVisible", "True")]
    public class CashForecastFixTag : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public CashForecastFixTag(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
        }
        // Fields...
        private CashForecastFixTagType _FixTagType;
        private string _Description;
        private string _Name;

        [RuleUniqueValue("CashForecastFixTag_Name_RuleUniqueValue", DefaultContexts.Save)]
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


        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                SetPropertyValue("Description", ref _Description, value);
            }
        }

        public CashForecastFixTagType FixTagType
        {
            get
            {
                return _FixTagType;
            }
            set
            {
                SetPropertyValue("FixTagType", ref _FixTagType, value);
            }
        }

        [Association("CashForecastFixTag-CashFlows")]
        public XPCollection<CashFlow> CashFlows
        {
            get
            {
                return GetCollection<CashFlow>("CashFlows");
            }
        }
    }

    public enum CashForecastFixTagType
    {
        Ignore,
        Allocate,
        [DevExpress.ExpressApp.DC.XafDisplayName("Schedule In")]
        ScheduleIn,
        [DevExpress.ExpressApp.DC.XafDisplayName("Schedule Out")]
        ScheduleOut
    }
}
