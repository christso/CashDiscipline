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
using CTMS.Module.BusinessObjects.ChartOfAccounts;
using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp.Xpo;

namespace CTMS.Module.BusinessObjects.FinAccounting
{
    public class FinAccountingDefaults : BaseObject
    {
        public FinAccountingDefaults(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        #region GL Code
        [XafDisplayName("GlCompany")]
        private GlCompany _GlCompany;
        public GlCompany GlCompany
        {
            get { return _GlCompany; }
            set { SetPropertyValue("GlCompany", ref _GlCompany, value); }
        }
        [XafDisplayName("GlAccount")]
        private GlAccount _GlAccount;
        public GlAccount GlAccount
        {
            get { return _GlAccount; }
            set { SetPropertyValue("GlAccount", ref _GlAccount, value); }
        }
        [XafDisplayName("GlCostCentre")]
        private GlCostCentre _GlCostCentre;
        public GlCostCentre GlCostCentre
        {
            get { return _GlCostCentre; }
            set { SetPropertyValue("GlCostCentre", ref _GlCostCentre, value); }
        }
        [XafDisplayName("GlProduct")]
        private GlProduct _GlProduct;
        public GlProduct GlProduct
        {
            get { return _GlProduct; }
            set { SetPropertyValue("GlProduct", ref _GlProduct, value); }
        }
        [XafDisplayName("GlSalesChannel")]
        private GlSalesChannel _GlSalesChannel;
        public GlSalesChannel GlSalesChannel
        {
            get { return _GlSalesChannel; }
            set { SetPropertyValue("GlSalesChannel", ref _GlSalesChannel, value); }
        }
        [XafDisplayName("GlCountry")]
        private GlCountry _GlCountry;
        public GlCountry GlCountry
        {
            get { return _GlCountry; }
            set { SetPropertyValue("GlCountry", ref _GlCountry, value); }
        }
        [XafDisplayName("GlIntercompany")]
        private GlIntercompany _GlIntercompany;
        public GlIntercompany GlIntercompany
        {
            get { return _GlIntercompany; }
            set { SetPropertyValue("GlIntercompany", ref _GlIntercompany, value); }
        }
        [XafDisplayName("GlProject")]
        private GlProject _GlProject;
        public GlProject GlProject
        {
            get { return _GlProject; }
            set { SetPropertyValue("GlProject", ref _GlProject, value); }
        }
        [XafDisplayName("GlLocation")]
        private GlLocation _GlLocation;
        public GlLocation GlLocation
        {
            get { return _GlLocation; }
            set { SetPropertyValue("GlLocation", ref _GlLocation, value); }
        }
        #endregion
        public static FinAccountingDefaults GetInstance(IObjectSpace objectSpace)
        {
            FinAccountingDefaults result = objectSpace.FindObject<FinAccountingDefaults>(null);
            if (result == null)
            {
                result = new FinAccountingDefaults(((XPObjectSpace)objectSpace).Session);
                result.Save();
            }
            return result;

        }
        protected override void OnDeleting()
        {
            throw new UserFriendlyException("This object cannot be deleted.");
        }

    }
}
