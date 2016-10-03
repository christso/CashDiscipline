using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Model;
using CashDiscipline.Module.BusinessObjects.Forex;
using DevExpress.ExpressApp;
using System.ComponentModel;

namespace CashDiscipline.Module.BusinessObjects.Cash
{
    [ModelDefault("IsCloneable", "True")]
    [ImageName("BO_List")]
    [NavigationItem("Cash Setup")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [ModelDefault("IsFooterVisible", "True")]
    [DefaultProperty("Name")]
    public class Activity : BaseObject
    {
        public Activity(Session session)
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
     
        private CashFlowSource _FixSource;

        private string _Name;


        // Fields...

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

        private string _ActivityL1;
        public string ActivityL1
        {
            get
            {
                return _ActivityL1;
            }
            set
            {
                SetPropertyValue("ActivityL1", ref _ActivityL1, value);
            }
        }

        private string _ActivityL2;

        public string ActivityL2
        {
            get
            {
                return _ActivityL2;
            }
            set
            {
                SetPropertyValue("ActivityL2", ref _ActivityL2, value);
            }
        }

        private string _ActivityL3;

        public string ActivityL3
        {
            get
            {
                return _ActivityL3;
            }
            set
            {
                SetPropertyValue("ActivityL3", ref _ActivityL3, value);
            }
        }

        private string _ActivityL4;
        public string ActivityL4
        {
            get
            {
                return _ActivityL4;
            }
            set
            {
                SetPropertyValue("ActivityL4", ref _ActivityL4, value);
            }
        }

        private string _OpActivityL1;
        public string OpActivityL1
        {
            get
            {
                return _OpActivityL1;
            }
            set
            {
                SetPropertyValue("OpActivityL1", ref _OpActivityL1, value);
            }
        }

        private string _OpActivityL2;

        public string OpActivityL2
        {
            get
            {
                return _OpActivityL2;
            }
            set
            {
                SetPropertyValue("OpActivityL2", ref _OpActivityL2, value);
            }
        }

        private string _OpActivityL3;

        public string OpActivityL3
        {
            get
            {
                return _OpActivityL3;
            }
            set
            {
                SetPropertyValue("OpActivityL3", ref _OpActivityL3, value);
            }
        }

        private string _OpActivityL4;
        public string OpActivityL4
        {
            get
            {
                return _OpActivityL4;
            }
            set
            {
                SetPropertyValue("OpActivityL4", ref _OpActivityL4, value);
            }
        }


        private string _RvnuActivityL1;
        public string RvnuActivityL1
        {
            get
            {
                return _RvnuActivityL1;
            }
            set
            {
                SetPropertyValue("RvnuActivityL1", ref _RvnuActivityL1, value);
            }
        }

        private string _RvnuActivityL2;

        public string RvnuActivityL2
        {
            get
            {
                return _RvnuActivityL2;
            }
            set
            {
                SetPropertyValue("RvnuActivityL2", ref _RvnuActivityL2, value);
            }
        }


        private string _HwlActivityL1;
        public string HwlActivityL1
        {
            get
            {
                return _HwlActivityL1;
            }
            set
            {
                SetPropertyValue("HwlActivityL1", ref _HwlActivityL1, value);
            }
        }

        private string _HwlActivityL2;

        public string HwlActivityL2
        {
            get
            {
                return _HwlActivityL2;
            }
            set
            {
                SetPropertyValue("HwlActivityL2", ref _HwlActivityL2, value);
            }
        }

        private string _HedgeActivity;
        public string HedgeActivity
        {
            get
            {
                return _HedgeActivity;
            }
            set
            {
                SetPropertyValue("HedgeActivity", ref _HedgeActivity, value);
            }
        }

        private Activity _FixActivity;
        public Activity FixActivity
        {
            get
            {
                return _FixActivity;
            }
            set
            {
                SetPropertyValue("FixActivity", ref _FixActivity, value);
            }
        }

        public CashFlowSource FixSource
        {
            get
            {
                return _FixSource;
            }
            set
            {
                SetPropertyValue("FixSource", ref _FixSource, value);
            }
        }

        private CashFlowForexSettleType _ForexSettleType;
        public CashFlowForexSettleType ForexSettleType
        {
            get
            {
                return _ForexSettleType;
            }
            set
            {
                SetPropertyValue("ForexSettleType", ref _ForexSettleType, value);
            }
        }

        private string forecastFixTag;
        public string ForecastFixTag
        {
            get
            {
                return forecastFixTag;
            }
            set
            {
                SetPropertyValue("ForecastFixTag", ref forecastFixTag, value);
            }
        }

        private int _Id;
        public int Id
        {
            get { return _Id; }
            set { SetPropertyValue<int>("Id", ref _Id, value); }
        }

    }

}