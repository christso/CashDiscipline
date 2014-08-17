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
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (http://documentation.devexpress.com/#Xaf/CustomDocument2701).
    public class CashFlowNote : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public CashFlowNote(Session session)
            : base(session)
        {
        }
        
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
        }

        private string _Text;
        private int _TranDay;
        private int _TranMonth;
        private int _TranYear;
        private ActivityTag _Dim_1_3;
        private ActivityTag _Dim_1_2;
        private ActivityTag _Dim_1_1;

        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                SetPropertyValue("Text", ref _Text, value);
            }
        }

        public int TranDay
        {
            get
            {
                return _TranDay;
            }
            set
            {
                SetPropertyValue("TranDay", ref _TranDay, value);
            }
        }

        public int TranMonth
        {
            get
            {
                return _TranMonth;
            }
            set
            {
                SetPropertyValue("TranMonth", ref _TranMonth, value);
            }
        }

        public int TranYear
        {
            get
            {
                return _TranYear;
            }
            set
            {
                SetPropertyValue("TranYear", ref _TranYear, value);
            }
        }

        public ActivityTag Dim_1_1
        {
            get
            {
                return _Dim_1_1;
            }
            set
            {
                SetPropertyValue("Dim_1_1", ref _Dim_1_1, value);
            }
        }

        public ActivityTag Dim_1_2
        {
            get
            {
                return _Dim_1_2;
            }
            set
            {
                SetPropertyValue("Dim_1_2", ref _Dim_1_2, value);
            }
        }

        public ActivityTag Dim_1_3
        {
            get
            {
                return _Dim_1_3;
            }
            set
            {
                SetPropertyValue("Dim_1_3", ref _Dim_1_3, value);
            }
        }
    }
}
