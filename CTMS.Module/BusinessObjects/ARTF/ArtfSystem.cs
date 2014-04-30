using System;
using System.ComponentModel;

using DevExpress.Xpo;
using DevExpress.Data.Filtering;

using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;

namespace CTMS.Module.BusinessObjects.Artf
{
    public class ArtfSystem : BaseObject
    {

        public ArtfSystem(Session session)
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
        [Association(@"ArtfSystem-ArtfLedgers")]
        public XPCollection<ArtfLedger> ArtfLedgers { get { return GetCollection<ArtfLedger>("ArtfLedgers"); } }
        [Association(@"ArtfSystem-ArtfReceipts")]
        public XPCollection<ArtfReceipt> ArtfReceipts { get { return GetCollection<ArtfReceipt>("ArtfReceipts"); } }
        [Association("ArtfSystem-ArtfCustomerType")]
        public XPCollection<ArtfCustomerType> ArtfCustomerTypes { get { return GetCollection<ArtfCustomerType>("ArtfCustomerTypes"); } }

    }

}
