using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Model;
using System.ComponentModel;
using DevExpress.ExpressApp;

namespace CashDiscipline.Module.BusinessObjects
{
    [ImageName("BO_List")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [ModelDefault("IsFooterVisible", "True")]
    [NavigationItem("Cash Setup")]
    [DeferredDeletion(false)]
    public class DimDate : BaseObject
    {
        public DimDate(Session session) : base(session)
        { }

        private DateTime _FullDate;

        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime FullDate
        {
            get
            {
                return _FullDate;
            }
            set
            {
                SetPropertyValue("FullDate", ref _FullDate, value);
            }
        }


        private int _DayOfMonth;
        [ModelDefault("DisplayFormat", "0")]
        [ModelDefault("EditMask", "0")]
        public int DayOfMonth
        {
            get
            {
                return _DayOfMonth;
            }
            set
            {
                SetPropertyValue("DayOfMonth", ref _DayOfMonth, value);
            }
        }


        private int _MonthNumberOfYear;
        [ModelDefault("DisplayFormat", "0")]
        [ModelDefault("EditMask", "0")]
        public int MonthNumberOfYear
        {
            get
            {
                return _MonthNumberOfYear;
            }
            set
            {
                SetPropertyValue("MonthNumberOfYear", ref _MonthNumberOfYear, value);
            }
        }


        private int _Year;
        [ModelDefault("DisplayFormat", "0")]
        [ModelDefault("EditMask", "0")]
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

        private int _DayOfWeekNumber;
        [ModelDefault("DisplayFormat", "0")]
        [ModelDefault("EditMask", "0")]
        public int DayOfWeekNumber
        {
            get
            {
                return _DayOfWeekNumber;
            }
            set
            {
                SetPropertyValue("DayOfWeekNumber", ref _DayOfWeekNumber, value);
            }
        }


        private string _MonthAbbrev;
        public string MonthAbbrev
        {
            get
            {
                return _MonthAbbrev;
            }
            set
            {
                SetPropertyValue("MonthAbbrev", ref _MonthAbbrev, value);
            }
        }

        private string _DayAbbrev;
        public string DayAbbrev
        {
            get
            {
                return _DayAbbrev;
            }
            set
            {
                SetPropertyValue("DayAbbrev", ref _DayAbbrev, value);
            }
        }

        private DateTime _WeekEnding;
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime WeekEnding
        {
            get
            {
                return _WeekEnding;
            }
            set
            {
                SetPropertyValue("WeekEnding", ref _WeekEnding, value);
            }
        }

    }
}
