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
using CashDiscipline.Module.BusinessObjects.ChartOfAccounts;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.ControllerHelpers.FinAccounting;
using CashDiscipline.Module.ParamObjects.FinAccounting;
using Xafology.Spreadsheet.Attributes;

namespace CashDiscipline.Module.BusinessObjects.FinAccounting
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
        [ExcelReportField]
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
        [ExcelReportField]
        public Guid SrcOid
        {
            get
            {
                if (string.IsNullOrEmpty(SrcBankStmt.BankStmtId))
                    return SrcCashFlow.Oid;
                return SrcBankStmt.Oid;
            }
        }

        [ExcelReportField]
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
        [ExcelReportField]
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
        [ExcelReportField]
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
        [ExcelReportField]
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
        [ExcelReportField]
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
        [ExcelReportField]
        public GlCompany GlCompany
        {
            get { return _GlCompany; }
            set { SetPropertyValue("GlCompany", ref _GlCompany, value); }
        }
        private GlAccount _GlAccount;
        [ExcelReportField]
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
        [ExcelReportField]
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
        [ExcelReportField]
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