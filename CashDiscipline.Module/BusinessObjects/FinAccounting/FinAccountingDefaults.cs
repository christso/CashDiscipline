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

using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp.Xpo;

namespace CashDiscipline.Module.BusinessObjects.FinAccounting
{
    [ModelDefault("ImageName", "BO_List")]
    [RuleCriteria("FinAccountingDefaults_CannotDeleteSingleton", DefaultContexts.Delete, "False",
    CustomMessageTemplate = "Cannot delete Singleton.")]
    public class FinAccountingDefaults : BaseObject
    {
        public FinAccountingDefaults(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        #region GL Code
        [XafDisplayName("GlCompany")]
        private string _GlCompany;
        public string GlCompany
        {
            get { return _GlCompany; }
            set { SetPropertyValue("GlCompany", ref _GlCompany, value); }
        }
        [XafDisplayName("GlAccount")]
        private string _GlAccount;
        public string GlAccount
        {
            get { return _GlAccount; }
            set { SetPropertyValue("GlAccount", ref _GlAccount, value); }
        }
        [XafDisplayName("GlCostCentre")]
        private string _GlCostCentre;
        public string GlCostCentre
        {
            get { return _GlCostCentre; }
            set { SetPropertyValue("GlCostCentre", ref _GlCostCentre, value); }
        }
        [XafDisplayName("GlProduct")]
        private string _GlProduct;
        public string GlProduct
        {
            get { return _GlProduct; }
            set { SetPropertyValue("GlProduct", ref _GlProduct, value); }
        }
        [XafDisplayName("GlSalesChannel")]
        private string _GlSalesChannel;
        public string GlSalesChannel
        {
            get { return _GlSalesChannel; }
            set { SetPropertyValue("GlSalesChannel", ref _GlSalesChannel, value); }
        }
        [XafDisplayName("GlCountry")]
        private string _GlCountry;
        public string GlCountry
        {
            get { return _GlCountry; }
            set { SetPropertyValue("GlCountry", ref _GlCountry, value); }
        }
        [XafDisplayName("GlIntercompany")]
        private string _GlIntercompany;
        public string GlIntercompany
        {
            get { return _GlIntercompany; }
            set { SetPropertyValue("GlIntercompany", ref _GlIntercompany, value); }
        }
        [XafDisplayName("GlProject")]
        private string _GlProject;
        public string GlProject
        {
            get { return _GlProject; }
            set { SetPropertyValue("GlProject", ref _GlProject, value); }
        }
        [XafDisplayName("GlLocation")]
        private string _GlLocation;
        public string GlLocation
        {
            get { return _GlLocation; }
            set { SetPropertyValue("GlLocation", ref _GlLocation, value); }
        }
        #endregion

        private string _GlDescDateFormat;
        public string GlDescDateFormat
        {
            get
            {
                return _GlDescDateFormat;
            }
            set
            {
                SetPropertyValue("GlDescDateFormat", ref _GlDescDateFormat, value);
            }
        }

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
    }
}
