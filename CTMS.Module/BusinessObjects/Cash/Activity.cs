using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Model;
using CTMS.Module.BusinessObjects.Forex;
using DevExpress.ExpressApp;

namespace CTMS.Module.BusinessObjects.Cash
{
    [ModelDefault("IsCloneable", "True")]
    [ImageName("BO_List")]
    [NavigationItem("Cash Setup")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [ModelDefault("IsFooterVisible", "True")]
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

        private ActivityTag _ActivityL1;
        public ActivityTag ActivityL1
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

        private ActivityTag _ActivityL2;

        public ActivityTag ActivityL2
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

        private ActivityTag _ActivityL3;

        public ActivityTag ActivityL3
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

        private ActivityTag _ActivityL4;
        public ActivityTag ActivityL4
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

        private ActivityTag _OpActivityL1;
        public ActivityTag OpActivityL1
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

        private ActivityTag _OpActivityL2;

        public ActivityTag OpActivityL2
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

        private ActivityTag _OpActivityL3;

        public ActivityTag OpActivityL3
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

        private ActivityTag _OpActivityL4;
        public ActivityTag OpActivityL4
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


        private ActivityTag _HedgeActivity;
        public ActivityTag HedgeActivity
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

        private int _Id;
        public int Id
        {
            get { return _Id; }
            set { SetPropertyValue<int>("Id", ref _Id, value); }
        }

    }

}