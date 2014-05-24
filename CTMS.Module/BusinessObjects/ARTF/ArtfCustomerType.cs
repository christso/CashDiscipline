using System;
using System.ComponentModel;

using DevExpress.Xpo;
using DevExpress.Data.Filtering;

using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using CTMS.Module.BusinessObjects.Cash;


namespace CTMS.Module.BusinessObjects.Artf
{
    public class ArtfCustomerType : BaseObject
    {
        public ArtfCustomerType(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here or place it only when the IsLoading property is false:
            // if (!IsLoading){
            //    It is now OK to place your initialization code here.
            // }
            // or as an alternative, move your initialization code into the AfterConstruction method.
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }
        private string _Name;
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
        [Association(@"ArtfCustomerType-ArtfRecons")]
        public XPCollection<ArtfRecon> ArtfRecons { get { return GetCollection<ArtfRecon>("ArtfRecons"); } }
        
        [Association("ArtfCustomerType-Cash_Accounts")]
        public XPCollection<Account> Cash_Accounts
        {
            get
            {
                return GetCollection<Account>("Cash_Accounts");
            }
        }

        [Association("ArtfCustomerType-Activities")]
        public XPCollection<Activity> Cash_Activities
        {
            get
            {
                return GetCollection<Activity>("Cash_Activities");
            }
        }

        private ArtfSystem _System;
        [Association("ArtfSystem-ArtfCustomerType")]
        public ArtfSystem System
        {
            get
            {
                return _System;
            }
            set
            {
                SetPropertyValue("System", ref _System, value);
            }
        }

        public new class Fields
        {
            public static OperandProperty ArtfCustomerType
            {
                get { return new OperandProperty("ArtfCustomerType"); }
            }
        }
    }

}
