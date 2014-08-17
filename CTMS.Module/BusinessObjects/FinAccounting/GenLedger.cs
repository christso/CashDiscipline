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
using CTMS.Module.ControllerHelpers.FinAccounting;
using CTMS.Module.ParamObjects.FinAccounting;


namespace CTMS.Module.BusinessObjects.FinAccounting
{
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (http://documentation.devexpress.com/#Xaf/CustomDocument2701).
    public class GenLedger : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public GenLedger(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
        }

        private Activity _Activity;
        private decimal _FunctionalCcyAmt;
        private CashFlow _SrcCashFlow;
        private BankStmt _SrcBankStmt;
        private FinJournalGroup _JournalGroup;
        private GenLedgerEntryType _EntryType;
        private bool _IsActivity;

        #region Source Ledger
        [VisibleInLookupListView(false)]
        public CashFlow SrcCashFlow
        {
            get
            {
                return _SrcCashFlow;
            }
            set
            {
                SetPropertyValue("SrcCashFlow", ref _SrcCashFlow, value);
            }
        }

        [Association("BankStmt-GenLedgers")]
        [VisibleInLookupListView(false)]
        public BankStmt SrcBankStmt
        {
            get
            {
                return _SrcBankStmt;
            }
            set
            {
                var oldBankStmt = _SrcBankStmt;
                SetPropertyValue("SrcBankStmt", ref _SrcBankStmt, value);
                if (!IsLoading && !IsSaving && oldBankStmt != _SrcBankStmt)
                {
                    oldBankStmt = oldBankStmt ?? _SrcBankStmt;
                    oldBankStmt.UpdateGenLedgerTotal(true);
                }
            }
        }
        [D2NXAF.ExpressApp.MsoExcel.Reports.ExcelReportFieldAttribute]
        public GenLedgerSourceType SourceType
        {
            get
            {
                if (_SrcCashFlow != null && _SrcBankStmt == null)
                    return GenLedgerSourceType.CashFlow;
                if (_SrcBankStmt != null && _SrcCashFlow == null)
                    return GenLedgerSourceType.BankStmt;
                return GenLedgerSourceType.Unknown;
            }
        }

        [VisibleInListView(false)]
        [VisibleInDetailView(false)]
        [VisibleInLookupListView(false)]
        [D2NXAF.ExpressApp.MsoExcel.Reports.ExcelReportFieldAttribute]
        public string SrcId
        {
            get
            {
                if (string.IsNullOrEmpty(SrcBankStmt.BankStmtId))
                    return SrcCashFlow.CashFlowId;
                return SrcBankStmt.BankStmtId;
            }
        }
        [D2NXAF.ExpressApp.MsoExcel.Reports.ExcelReportFieldAttribute]
        public DateTime? SrcDate
        {
            get
            {
                if (SourceType == GenLedgerSourceType.CashFlow)
                    return _SrcCashFlow.TranDate;
                else if (SourceType == GenLedgerSourceType.BankStmt)
                    return _SrcBankStmt.TranDate;
                return null;
            }
        }
        public Account SrcAccount
        {
            get
            {
                if (SourceType == GenLedgerSourceType.CashFlow)
                    return _SrcCashFlow.Account;
                else if (SourceType == GenLedgerSourceType.BankStmt)
                    return _SrcBankStmt.Account;
                return null;
            }
        }
        [D2NXAF.ExpressApp.MsoExcel.Reports.ExcelReportFieldAttribute]
        public decimal? SrcAccountCcyAmt
        {
            get
            {
                if (SourceType == GenLedgerSourceType.CashFlow)
                    return _SrcCashFlow.AccountCcyAmt;
                else if (SourceType == GenLedgerSourceType.BankStmt)
                    return _SrcBankStmt.TranAmount;
                return null;
            }
        }
        [D2NXAF.ExpressApp.MsoExcel.Reports.ExcelReportFieldAttribute]
        public string SrcDescription
        {
            get
            {
                if (SourceType == GenLedgerSourceType.CashFlow)
                    return _SrcCashFlow.Description;
                else if (SourceType == GenLedgerSourceType.BankStmt)
                    return _SrcBankStmt.TranDescription;
                return null;
            }
        }
        #endregion
        [D2NXAF.ExpressApp.MsoExcel.Reports.ExcelReportFieldAttribute]
        [Association("JournalGroup-GenLedger")]
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

        [RuleRequiredField("GenLedger.Activity_RuleRequiredField", DefaultContexts.Save)]
        public Activity Activity
        {
            get
            {
                return _Activity;
            }
            set
            {
                SetPropertyValue("Activity", ref _Activity, value);
            }
        }

        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        [D2NXAF.ExpressApp.MsoExcel.Reports.ExcelReportFieldAttribute]
        [DevExpress.Xpo.DisplayName("Gl Functional Ccy Amt")]
        public decimal FunctionalCcyAmt
        {
            get
            {
                return _FunctionalCcyAmt;
            }
            set
            {
                SetPropertyValue("FunctionalCcyAmt", ref _FunctionalCcyAmt, value);
            }
        }

        private GlCompany _GlCompany;
        [D2NXAF.ExpressApp.MsoExcel.Reports.ExcelReportFieldAttribute]
        public GlCompany GlCompany
        {
            get { return _GlCompany; }
            set { SetPropertyValue("GlCompany", ref _GlCompany, value); }
        }
        private GlAccount _GlAccount;
        [D2NXAF.ExpressApp.MsoExcel.Reports.ExcelReportFieldAttribute]
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

        private string _GlDescription;
        [D2NXAF.ExpressApp.MsoExcel.Reports.ExcelReportFieldAttribute]
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

        public GenLedgerEntryType EntryType
        {
            get
            {
                return _EntryType;
            }
            set
            {
                SetPropertyValue("EntryType", ref _EntryType, value);
            }
        }
        [D2NXAF.ExpressApp.MsoExcel.Reports.ExcelReportFieldAttribute]
        public bool IsActivity
        {
            get
            {
            	return _IsActivity;
            }
            set
            {
                SetPropertyValue("IsActivity", ref _IsActivity, value);
            }
        }

        public static void GenerateJournals(FinGenJournalParam paramObj)
        {
            var jg = new JournalGenerator(paramObj);

            jg.Execute();
        }
    }

    public enum GenLedgerSourceType
    {
        Unknown,
        BankStmt,
        CashFlow
    }

    public enum GenLedgerEntryType
    {
        Auto,
        Manual
    }
}
