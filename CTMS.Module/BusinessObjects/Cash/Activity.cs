using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Model;
using CTMS.Module.BusinessObjects.Forex;

namespace CTMS.Module.BusinessObjects.Cash
{
    [ModelDefault("IsCloneable", "True")]
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
        private bool _IsArtfDefault;
        private Artf.ArtfCustomerType _ArtfCustomerType;
        private ActivityTag _Dim_1_3;
        private ActivityTag _Dim_1_2;
        private ActivityTag _Dim_1_1;
        private string _Name;
        private int _Id;

        public int Id
        {
            get { return _Id; }
            set { SetPropertyValue<int>("Id", ref _Id, value); }
        }

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

        public bool IsArtfDefault
        {
            get
            {
                return _IsArtfDefault;
            }
            set
            {
                SetPropertyValue("IsArtfDefault", ref _IsArtfDefault, value);
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

        [Association(@"Activity-BankStmt", typeof(BankStmt))]
        public XPCollection<BankStmt> BankStmt { get { return GetCollection<BankStmt>("BankStmt"); } }

        [Association("Activity-CashFlows")]
        public XPCollection<CashFlow> CashFlows
        {
            get
            {
                return GetCollection<CashFlow>("CashFlows");
            }
        }

        [Association("ArtfCustomerType-Activities")]
        public Artf.ArtfCustomerType ArtfCustomerType
        {
            get
            {
                return _ArtfCustomerType;
            }
            set
            {
                SetPropertyValue("ArtfCustomerType", ref _ArtfCustomerType, value);
            }
        }



        public new class Fields
        {
            public static OperandProperty IsArtfDefault
            {
                get
                {
                    return new OperandProperty("IsArtfDefault");
                }
            }
            public static OperandProperty ArtfCustomerType
            {
                get
                {
                    return new OperandProperty("ArtfCustomerType");
                }
            }
        }

        //[Association(@"Activity-ArtfTasks", typeof(Artf.ArtfTask))]
        //public XPCollection<Artf.ArtfTask> ArtfTask { get { return GetCollection<Artf.ArtfTask>("ArtfTask"); } }


        //public Account AccountMap
        //{
        //    get
        //    {
        //        return _AccountMap;
        //    }
        //    set
        //    {
        //        if (_AccountMap == value)
        //            return;

        //        Account prevAccountMap = _AccountMap;
        //        prevAccountMap = value;

        //        if (IsLoading) return;

        //        if (prevAccountMap != null && prevAccountMap.ActivityMap == this)
        //            prevAccountMap.ActivityMap = null;
        //        if (_AccountMap != null)
        //            _AccountMap.ActivityMap = this;
        //        OnChanged("AccountMap");
        //    }
        //}
    }

}