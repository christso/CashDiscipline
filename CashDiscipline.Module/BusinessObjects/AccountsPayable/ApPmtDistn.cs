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

using Xafology.ExpressApp.Xpo.Import;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Attributes;
using Xafology.ExpressApp.BatchDelete;

namespace CashDiscipline.Module.BusinessObjects.AccountsPayable
{
    [ModelDefault("IsFooterVisible", "True")]
    [ModelDefault("ImageName", "BO_List")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [AutoColumnWidth(false)]
    [BatchDelete(isVisible: true)]
    public class ApPmtDistn : BaseObject
    {
        public ApPmtDistn(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here or place it only when the IsLoading property is false.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        private DateTime _PaymentDate;
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat","dd-MMM-yy")]
        public DateTime PaymentDate
        {
            get { return _PaymentDate; }
            set { SetPropertyValue("PaymentDate", ref _PaymentDate, value); }
        }
        private ApSource _Source;
        public ApSource Source
        {
            get
            {
                return _Source;
            }
            set
            {
                SetPropertyValue("Source", ref _Source, value);
            }
        }

        // use Account and lookup from BankAccountName instead?

        private ApBankAccount _BankAccount;
        public ApBankAccount BankAccount
        {
            get
            {
                return _BankAccount;
            }
            set
            {
                SetPropertyValue("BankAccount", ref _BankAccount, value);
            }
        }

        private ApPayGroup _PayGroup;
        public ApPayGroup PayGroup
        {
            get
            {
                return _PayGroup;
            }
            set
            {
                SetPropertyValue("PayGroup", ref _PayGroup, value);
            }
        }

        private ApInvSource _InvSource;
        public ApInvSource InvSource
        {
            get
            {
                return _InvSource;
            }
            set
            {
                SetPropertyValue("InvSource", ref _InvSource, value);
            }
        }

        private ApVendor _Vendor;
        public ApVendor Vendor
        {
            get
            {
                return _Vendor;
            }
            set
            {
                SetPropertyValue("Vendor", ref _Vendor, value);
            }
        }
        private string _GlCompany;
        [Size(10)]
        public string GlCompany
        {
            get { return _GlCompany; }
            set { SetPropertyValue("GlCompany", ref _GlCompany, value); }
        }
        private string _GlAccount;
        [Size(10)]
        public string GlAccount
        {
            get { return _GlAccount; }
            set { SetPropertyValue("GlAccount", ref _GlAccount, value); }
        }
        private string _GlCostCentre;
        [Size(10)]
        public string GlCostCentre
        {
            get { return _GlCostCentre; }
            set { SetPropertyValue("GlCostCentre", ref _GlCostCentre, value); }
        }
        private string _GlProduct;
        [Size(10)]
        public string GlProduct
        {
            get { return _GlProduct; }
            set { SetPropertyValue("GlProduct", ref _GlProduct, value); }
        }
        private string _GlSalesChannel;
        [Size(10)]
        public string GlSalesChannel
        {
            get { return _GlSalesChannel; }
            set { SetPropertyValue("GlSalesChannel", ref _GlSalesChannel, value); }
        }
        private string _GlCountry;
        [Size(10)]
        public string GlCountry
        {
            get { return _GlCountry; }
            set { SetPropertyValue("GlCountry", ref _GlCountry, value); }
        }
        private string _GlIntercompany;
        [Size(10)]
        public string GlIntercompany
        {
            get { return _GlIntercompany; }
            set { SetPropertyValue("GlIntercompany", ref _GlIntercompany, value); }
        }
        private string _GlProject;
        [Size(10)]
        public string GlProject
        {
            get { return _GlProject; }
            set { SetPropertyValue("GlProject", ref _GlProject, value); }
        }
        private string _GlLocation;
        [Size(10)]
        public string GlLocation
        {
            get { return _GlLocation; }
            set { SetPropertyValue("GlLocation", ref _GlLocation, value); }
        }

        private int _PoNum;
        [ModelDefault("DisplayFormat", "f0")]
        public int PoNum
        {
            get { return _PoNum; }
            set { SetPropertyValue("PoNum", ref _PoNum, value); }
        }
        private string _InvoiceNum;
        [Size(255)]
        public string InvoiceNum
        {
            get { return _InvoiceNum; }
            set { SetPropertyValue("InvoiceNum", ref _InvoiceNum, value); }
        }
        private decimal _PaymentAmountFx;
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal PaymentAmountFx
        {
            get { return _PaymentAmountFx; }
            set { SetPropertyValue("PaymentAmountFx", ref _PaymentAmountFx, value); }
        }
        private decimal _PaymentAmountAud;
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal PaymentAmountAud
        {
            get { return _PaymentAmountAud; }
            set { SetPropertyValue("PaymentAmountAud", ref _PaymentAmountAud, value); }
        }
        
        private int _PaymentNumber;
        [ModelDefault("DisplayFormat", "f0")]
        public int PaymentNumber
        {
            get { return _PaymentNumber; }
            set { SetPropertyValue("PaymentNumber", ref _PaymentNumber, value); }
        }
        private string _PaymentBatchName;
        [Size(50)]
        public string PaymentBatchName
        {
            get { return _PaymentBatchName; }
            set { SetPropertyValue("PaymentBatchName", ref _PaymentBatchName, value); }
        }
        private Currency _InvoiceCurrency;
        public Currency InvoiceCurrency
        {
            get { return _InvoiceCurrency; }
            set { SetPropertyValue("InvoiceCurrency", ref _InvoiceCurrency, value); }
        }
        private DateTime _PaymentCreationDate;
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime PaymentCreationDate
        {
            get { return _PaymentCreationDate; }
            set { SetPropertyValue("PaymentCreationDate", ref _PaymentCreationDate, value); }
        }
        
        private string _InvoiceLineDesc;
        [Size(1000)]
        public string InvoiceLineDesc
        {
            get { return _InvoiceLineDesc; }
            set { SetPropertyValue("InvoiceLineDesc", ref _InvoiceLineDesc, value); }
        }

        private Account _Account;
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

        private Activity _Activity;
        public Activity Activity
        {
            get { return _Activity; }
            set { SetPropertyValue("Activity", ref _Activity, value); }
        }
        private Counterparty _Counterparty;
        public Counterparty Counterparty
        {
            get { return _Counterparty; }
            set { SetPropertyValue("Counterparty", ref _Counterparty, value); }
        }
        private string _SummaryDescription;
        [Size(255)]
        public string SummaryDescription
        {
            get { return _SummaryDescription; }
            set { SetPropertyValue("SummaryDescription", ref _SummaryDescription, value); }
        }
        private Currency _PaymentCurrency;
        public Currency PaymentCurrency
        {
            get { return _PaymentCurrency; }
            set { SetPropertyValue("PaymentCurrency", ref _PaymentCurrency, value); }
        }
        private DateTime _InvoiceDueDate;
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime InvoiceDueDate
        {
            get { return _InvoiceDueDate; }
            set { SetPropertyValue("InvoiceDueDate", ref _InvoiceDueDate, value); }
        }

        private DateTime _InvoiceCreationDate;
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime InvoiceCreationDate
        {
            get
            {
                return _InvoiceCreationDate;
            }
            set
            {
                SetPropertyValue("InvoiceCreationDate", ref _InvoiceCreationDate, value);
            }
        }

        private string _CapexOpex;
        [Size(50)]
        public string CapexOpex
        {
            get
            {
                return _CapexOpex;
            }
            set
            {
                SetPropertyValue("CapexOpex", ref _CapexOpex, value);
            }
        }

        [Size(50)]
        private string _OrgName;
        public string OrgName
        {
            get
            {
                return _OrgName;
            }
            set
            {
                SetPropertyValue("OrgName", ref _OrgName, value);
            }
        }

        private string _LineType;
        [Size(50)]
        public string LineType
        {
            get
            {
                return _LineType;
            }
            set
            {
                SetPropertyValue("LineType", ref _LineType, value);
            }
        }

        private string _TaxCode;
        [Size(10)]
        public string TaxCode
        {
            get
            {
                return _TaxCode;
            }
            set
            {
                SetPropertyValue("TaxCode", ref _TaxCode, value);
            }
        }

        private DateTime _InvoiceDate;
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime InvoiceDate
        {
            get
            {
                return _InvoiceDate;
            }
            set
            {
                SetPropertyValue("InvoiceDate", ref _InvoiceDate, value);
            }
        }

        private string _ExpenditureType;
        [Size(255)]
        public string ExpenditureType
        {
            get
            {
                return _ExpenditureType;
            }
            set
            {
                SetPropertyValue("ExpenditureType", ref _ExpenditureType, value);
            }
        }

        private int _ProjectNumber;
        [ModelDefault("DisplayFormat", "f0")]
        public int ProjectNumber
        {
            get
            {
                return _ProjectNumber;
            }
            set
            {
                SetPropertyValue("ProjectNumber", ref _ProjectNumber, value);
            }
        }

        private DateTime _InvoiceReceivedDate;
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime InvoiceReceivedDate
        {
            get
            {
                return _InvoiceReceivedDate;
            }
            set
            {
                SetPropertyValue("InvoiceReceivedDate", ref _InvoiceReceivedDate, value);
            }
        }

        private DateTime _PaymentReceiptDate;
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime PaymentReceiptDate
        {
            get
            {
                return _PaymentReceiptDate;
            }
            set
            {
                SetPropertyValue("PaymentReceiptDate", ref _PaymentReceiptDate, value);
            }
        }

        private string _PaymentTerms;
        [Size(50)]
        public string PaymentTerms
        {
            get
            {
                return _PaymentTerms;
            }
            set
            {
                SetPropertyValue("PaymentTerms", ref _PaymentTerms, value);
            }
        }

        private int _InvoiceId;
        [ModelDefault("DisplayFormat", "f0")]
        public int InvoiceId
        {
            get
            {
                return _InvoiceId;
            }
            set
            {
                SetPropertyValue("InvoiceId", ref _InvoiceId, value);
            }
        }

        private string _VendorNumber;
        [ModelDefault("DisplayFormat", "f0")]
        public string VendorNumber
        {
            get
            {
                return _VendorNumber;
            }
            set
            {
                SetPropertyValue("VendorNumber", ref _VendorNumber, value);
            }
        }

        private CashFlow _CashFlow;
        [Association("CashFlow-ApPmtDistns")]
        public CashFlow CashFlow
        {
            get
            {
                return _CashFlow;
            }
            set
            {
                SetPropertyValue("CashFlow", ref _CashFlow, value);
            }
        }

        private InputSourceType _InputSource;
        public InputSourceType InputSource
        {
            get
            {
                return _InputSource;
            }
            set
            {
                SetPropertyValue("InputSource", ref _InputSource, value);
            }
        }

        public enum InputSourceType
        {
            Oracle = 0,
            Manual = 1
        }
    }
}
