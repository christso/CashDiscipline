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
using CashDiscipline.Module.BusinessObjects.ChartOfAccounts;
using DevExpress.ExpressApp.Xpo;
using Xafology.ExpressApp.RowMover;
using CashDiscipline.Module.Attributes;

namespace CashDiscipline.Module.BusinessObjects.FinAccounting
{
    [ImageName("BO_List")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [ModelDefault("IsFooterVisible", "True")]
    [AutoColumnWidth(false)]
    public class FinActivity : BaseObject, IRowMoverObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public FinActivity(Session session)
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
        // singleton
        private static int NextIndex = 1;

        private int _RowIndex;
        private string _FunctionalCcyAmtExpr;
        private string _GlDescription;
        private Activity _FromActivity;
        private Activity _ToActivity;
        private string _Token;
        private FinJournalGroup _JournalGroup;
        private string _GLDescDateFormat;

        [RuleRequiredField("FinActivity.JournalGroup_RuleRequiredField", DefaultContexts.Save)]
        [Association("JournalGroup-FinActivities")]
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

        [ModelDefault("DisplayFormat", "f0")]
        [RuleRequiredField("FinActivity.Index_RuleRequiredField", DefaultContexts.Save)]
        public int RowIndex
        {
            get
            {
                return _RowIndex;
            }
            set
            {
                SetPropertyValue("RowIndex", ref _RowIndex, value);
            }
        }

        [RuleRequiredField("FinActivity.FromActivity_RuleRequiredField", DefaultContexts.Save)]
        public Activity FromActivity
        {
            get
            {
                return _FromActivity;
            }
            set
            {
                SetPropertyValue("FromActivity", ref _FromActivity, value);
            }
        }

        [RuleRequiredField("FinActivity.ToActivity_RuleRequiredField", DefaultContexts.Save)]
        public Activity ToActivity
        {
            get
            {
                return _ToActivity;
            }
            set
            {
                SetPropertyValue("ToActivity", ref _ToActivity, value);
            }
        }

        public string Token
        {
            get
            {
                return _Token;
            }
            set
            {
                SetPropertyValue("Token", ref _Token, value);
            }
        }

        [VisibleInListView(true)]
        [Size(SizeAttribute.Unlimited)]
        public string FunctionalCcyAmtExpr
        {
            get
            {
                return _FunctionalCcyAmtExpr;
            }
            set
            {
                SetPropertyValue("FunctionalCcyAmtExpr", ref _FunctionalCcyAmtExpr, value);
            }
        }

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

        public string GlDescDateFormat
        {
            get
            {
                return _GLDescDateFormat;
            }
            set
            {
                SetPropertyValue("GLDescDateFormat", ref _GLDescDateFormat, value);
            }
        }

        private GlCompany _GlCompany;
        public GlCompany GlCompany
        {
            get { return _GlCompany; }
            set { SetPropertyValue("GlCompany", ref _GlCompany, value); }
        }
        private GlAccount _GlAccount;
        public GlAccount GlAccount
        {
            get { return _GlAccount; }
            set { SetPropertyValue("GlAccount", ref _GlAccount, value); }
        }
        private GlCostCentre _GlCostCentre;
        public GlCostCentre GlCostCentre
        {
            get { return _GlCostCentre; }
            set { SetPropertyValue("GlCostCentre", ref _GlCostCentre, value); }
        }
        private GlProduct _GlProduct;
        public GlProduct GlProduct
        {
            get { return _GlProduct; }
            set { SetPropertyValue("GlProduct", ref _GlProduct, value); }
        }
        private GlSalesChannel _GlSalesChannel;
        public GlSalesChannel GlSalesChannel
        {
            get { return _GlSalesChannel; }
            set { SetPropertyValue("GlSalesChannel", ref _GlSalesChannel, value); }
        }
        private GlCountry _GlCountry;
        public GlCountry GlCountry
        {
            get { return _GlCountry; }
            set { SetPropertyValue("GlCountry", ref _GlCountry, value); }
        }
        private GlIntercompany _GlIntercompany;
        public GlIntercompany GlIntercompany
        {
            get { return _GlIntercompany; }
            set { SetPropertyValue("GlIntercompany", ref _GlIntercompany, value); }
        }
        private GlProject _GlProject;
        public GlProject GlProject
        {
            get { return _GlProject; }
            set { SetPropertyValue("GlProject", ref _GlProject, value); }
        }
        private GlLocation _GlLocation;
        public GlLocation GlLocation
        {
            get { return _GlLocation; }
            set { SetPropertyValue("GlLocation", ref _GlLocation, value); }
        }

        public void InitDefaultValues()
        {
            #region Index

            object maxIndex = Session.Evaluate<FinActivity>(CriteriaOperator.Parse("Max(RowIndex)"), null);

            if (maxIndex != null)
            {
                if ((int)maxIndex >= NextIndex)
                    NextIndex = (int)maxIndex + 1;
            }
            this.RowIndex = NextIndex;

            #endregion

            #region Accounting

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

            GlDescDateFormat = defObj.GlDescDateFormat;
            Enabled = true;
            #endregion
        }

        private FinJournalTargetObject _TargetObject;
        public FinJournalTargetObject TargetObject
        {
            get
            {
                return _TargetObject;
            }
            set
            {
                SetPropertyValue("TargetObject", ref _TargetObject, value);
            }
        }

        private bool _Enabled;
        public bool Enabled
        {
            get
            {
                return _Enabled;
            }
            set
            {
                SetPropertyValue("Enabled", ref _Enabled, value);
            }
        }
    }

    public enum FinJournalTargetObject
    {
        All,
        [DevExpress.ExpressApp.DC.XafDisplayName("Bank Stmt")]
        BankStmt,
        [DevExpress.ExpressApp.DC.XafDisplayName("Cash Flow")]
        CashFlow
    }
}
