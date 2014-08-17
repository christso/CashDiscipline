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
    public class ArtfGlCode : BaseObject
    {
        public ArtfGlCode(Session session)
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
        string _Name;
        public string Name
        {
            get { return _Name; }
            set { SetPropertyValue<string>("Name", ref _Name, value); }
        }
        string _GlCompany;
        public string GlCompany
        {
            get { return _GlCompany; }
            set { SetPropertyValue<string>("GlCompany", ref _GlCompany, value); }
        }
        string _GlAccount;
        public string GlAccount
        {
            get { return _GlAccount; }
            set { SetPropertyValue<string>("GlAccount", ref _GlAccount, value); }
        }

        [Association(@"ArtfGlCode-ArtfLedgers")]
        public XPCollection<ArtfLedger> ArtfLedgers { get { return GetCollection<ArtfLedger>("ArtfLedgers"); } }
    }

}
