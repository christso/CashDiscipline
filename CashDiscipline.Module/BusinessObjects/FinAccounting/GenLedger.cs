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
using CashDiscipline.Module.Logic.FinAccounting;
using CashDiscipline.Module.ParamObjects.FinAccounting;
using CashDiscipline.Module.Attributes;
using Xafology.ExpressApp.BatchDelete;
using CashDiscipline.Module.Logic.SqlServer;

namespace CashDiscipline.Module.BusinessObjects.FinAccounting
{
    [ModelDefault("ImageName", "BO_List")]
    [AutoColumnWidth(false)]
    [BatchDelete(isVisible: true, isOptimized: true)]
    public class GenLedger : BaseObject, IBatchDeletable
    { 
        public GenLedger(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            var sqlUtil = new SqlQueryUtil(Session);
            CreationDateTime = sqlUtil.GetDate();
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

        [VisibleInLookupListView(false)]
        public BankStmt SrcBankStmt
        {
            get
            {
                return _SrcBankStmt;
            }
            set
            {
                SetPropertyValue("SrcBankStmt", ref _SrcBankStmt, value);
            }
        }

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
        public Guid SrcOid
        {
            get
            {
                if (SrcBankStmt == null)
                    return SrcCashFlow.Oid;
                return SrcBankStmt.Oid;
            }
        }

        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
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

        DateTime _GlDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime GlDate
        {
            get {
                if (_GlDate == default(DateTime))
                {
                    if (_SrcCashFlow != null)
                        SetPropertyValue("GlDate", ref _GlDate, _SrcCashFlow.TranDate);
                    else if (_SrcBankStmt != null)
                        SetPropertyValue("GlDate", ref _GlDate, _SrcBankStmt.TranDate);
                }
                return _GlDate;
            }
            set {
                SetPropertyValue("GlDate", ref _GlDate, value);
            }
        }

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

        private string _GlDescription;
        [VisibleInListView(true)]
        [Size(SizeAttribute.Unlimited)]
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

        private bool _IsJournal;
        public bool IsJournal
        {
            get
            {
                return _IsJournal;
            }
            set
            {
                SetPropertyValue("IsJournal", ref _IsJournal, value);
            }
        }

        DateTime _CreationDateTime;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime CreationDateTime
        {
            get { return _CreationDateTime; }
            set { SetPropertyValue("CreationDateTime", ref _CreationDateTime, value); }
        }
    }

    public enum GenLedgerSourceType
    {
        Unknown,
        [DevExpress.ExpressApp.DC.XafDisplayName("Bank Stmt")]
        BankStmt,
        [DevExpress.ExpressApp.DC.XafDisplayName("Cash Flow")]
        CashFlow
    }

    public enum GenLedgerEntryType
    {
        Auto,
        Manual
    }
}
