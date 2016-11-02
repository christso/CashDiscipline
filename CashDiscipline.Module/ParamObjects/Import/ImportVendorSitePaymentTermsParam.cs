using CashDiscipline.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.SystemModule;


namespace CashDiscipline.Module.ParamObjects.Import
{
    [Xafology.ExpressApp.Attributes.AutoCreatableObjectAttribute]
    [ModelDefault("ImageName", "BO_List")]
    [NavigationItem("Import")]
    public class ImportVendorSitePaymentTermsParam : BaseObject
    {
        public ImportVendorSitePaymentTermsParam(Session session) : base(session)
        {
            session.LockingOption = LockingOption.None;
        }

        private DateTime _AsAtDate;

        public DateTime AsAtDate
        {
            get
            {
                return _AsAtDate;
            }
            set
            {
                SetPropertyValue("AsAtDate", ref _AsAtDate, value);
            }
        }

        private string _FilePath;
        [Size(SizeAttribute.Unlimited)]
        public string FilePath
        {
            get
            {
                return _FilePath;
            }
            set
            {
                SetPropertyValue("FilePath", ref _FilePath, value);
            }
        }
    }
}
