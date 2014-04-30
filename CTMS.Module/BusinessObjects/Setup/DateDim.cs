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

namespace CTMS.Module.BusinessObjects.Setup
{
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (http://documentation.devexpress.com/#Xaf/CustomDocument2701).
    public class DateDim : XPCustomObject 
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public DateDim(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
        }

        private DayOfWeek _DayOfWeek;
        private int _Year;
        private string _MonthName;

        private DateTime _DateKey;

        [VisibleInListView(true)]
        [VisibleInDetailView(true)]
        [Key(false), Browsable(true)]
        public DateTime DateKey
        {
            get
            {
                return _DateKey;
            }
            set
            {
                SetPropertyValue("DateKey", ref _DateKey, value);
            }
        }

        public string MonthName
        {
            get
            {
                return _MonthName;
            }
            set
            {
                SetPropertyValue("MonthName", ref _MonthName, value);
            }
        }

        [ModelDefault("DisplayFormat", "D")]
        public int Year
        {
            get
            {
                return _Year;
            }
            set
            {
                SetPropertyValue("Year", ref _Year, value);
            }
        }

        public DayOfWeek DayOfWeek
        {
            get
            {
                return _DayOfWeek;
            }
            set
            {
                SetPropertyValue("DayOfWeek", ref _DayOfWeek, value);
            }
        }

        public string DayOfWeekShort
        {
            get
            {
                return _DayOfWeek.ToString().Substring(0,3);
            }
        }
    }
}
