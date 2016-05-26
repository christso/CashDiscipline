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
    [ImageName("BO_List")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [ModelDefault("IsFooterVisible", "True")]
    public class FinAccount : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public FinAccount(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
            if (AppSettings.UserTriggersEnabled)
                InitDefaultValues();
        }

        private FinJournalGroup _JournalGroup;
        [RuleRequiredField("FinAccount.JournalGroup_RuleRequiredField", DefaultContexts.Save)]
        [Association("JournalGroup-FinAccounts")]
        public FinJournalGroup JournalGroup
        {
            get
            {
                return _JournalGroup;
            }
            set
            {
                SetPropertyValue("JournalGroup", ref _JournalGroup, value);
            }
        }

        private Account _Account;
        [RuleRequiredField("FinAccount.Account_RuleRequiredField", DefaultContexts.Save)]
        [RuleUniqueValue("FinAccount.Account_RuleUniqueValue", DefaultContexts.Save)]
        public Account Account
        {
            get
            {
                return _Account;
            }
            set
            {
                SetPropertyValue("Account", ref _Account, value);
            }
        }

        private string _GlDescription;
        public string GlDescription
        {
            get
            {
                return _GlDescription;
            }
            set
            {
                SetPropertyValue("GlDescription", ref _GlDescription, value);
            }
        }

        private string _GlCompany;
        public string GlCompany
        {
            get { return _GlCompany; }
            set { SetPropertyValue("GlCompany", ref _GlCompany, value); }
        }
        private string _GlAccount;
        public string GlAccount
        {
            get { return _GlAccount; }
            set { SetPropertyValue("GlAccount", ref _GlAccount, value); }
        }
        private string _GlCostCentre;
        public string GlCostCentre
        {
            get { return _GlCostCentre; }
            set { SetPropertyValue("GlCostCentre", ref _GlCostCentre, value); }
        }
        private string _GlProduct;
        public string GlProduct
        {
            get { return _GlProduct; }
            set { SetPropertyValue("GlProduct", ref _GlProduct, value); }
        }
        private string _GlSalesChannel;
        public string GlSalesChannel
        {
            get { return _GlSalesChannel; }
            set { SetPropertyValue("GlSalesChannel", ref _GlSalesChannel, value); }
        }
        private string _GlCountry;
        public string GlCountry
        {
            get { return _GlCountry; }
            set { SetPropertyValue("GlCountry", ref _GlCountry, value); }
        }
        private string _GlIntercompany;
        public string GlIntercompany
        {
            get { return _GlIntercompany; }
            set { SetPropertyValue("GlIntercompany", ref _GlIntercompany, value); }
        }
        private string _GlProject;
        public string GlProject
        {
            get { return _GlProject; }
            set { SetPropertyValue("GlProject", ref _GlProject, value); }
        }
        private string _GlLocation;
        public string GlLocation
        {
            get { return _GlLocation; }
            set { SetPropertyValue("GlLocation", ref _GlLocation, value); }
        }

        public void InitDefaultValues()
        {
            var defObj = Session.FindObject<FinAccountingDefaults>(null);
            if (defObj == null) return;
            if (GlCompany == null)
                GlCompany = defObj.GlCompany;
            if (GlAccount == null)
                GlAccount = defObj.GlAccount;
            if (GlCostCentre == null)
                GlCostCentre = defObj.GlCostCentre;
            if (GlProduct == null)
                GlProduct = defObj.GlProduct;
            if (GlSalesChannel == null)
                GlSalesChannel = defObj.GlSalesChannel;
            if (GlCountry == null)
                GlCountry = defObj.GlCountry;
            if (GlIntercompany == null)
                GlIntercompany = defObj.GlIntercompany;
            if (GlProject == null)
                GlProject = defObj.GlProject;
            if (GlLocation == null)
                GlLocation = defObj.GlLocation;
        }
    }
}
