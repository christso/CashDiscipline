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
using Xafology.ExpressApp.Xpo.Import;

// With XPO, the data model is declared by classes (so-called Persistent Objects) that will define the database structure, and consequently, the user interface (http://documentation.devexpress.com/#Xaf/CustomDocument2600).
namespace CashDiscipline.Module.BusinessObjects.Cash.AccountsPayable
{
    [ModelDefault("IsFooterVisible", "True")]
    public class ApPmtDistn : BaseObject, IXpoImportable
    { // You can use a different base persistent class based on your requirements (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public ApPmtDistn(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here or place it only when the IsLoading property is false.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code (check out http://documentation.devexpress.com/#Xaf/CustomDocument2834 for more details).
        }
        private ApVendor _Vendor;
        private ApInvSource _InvSource;
        private ApPayGroup _PayGroup;
        private ApBankAccount _BankAccount;
        private int _Id;
        public int Id
        {
            get { return _Id; }
            set { SetPropertyValue("Id", ref _Id, value); }
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
        [ModelDefault("DisplayFormat", "f0")]
        [ModelDefault("AllowEdit", "false")]
        private int _PoNum;
        public int PoNum
        {
            get { return _PoNum; }
            set { SetPropertyValue("PoNum", ref _PoNum, value); }
        }
        private string _InvoiceNum;
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
        [ModelDefault("DisplayFormat", "f0")]
        [ModelDefault("AllowEdit", "false")]
        private int _PaymentNumber;
        public int PaymentNumber
        {
            get { return _PaymentNumber; }
            set { SetPropertyValue("PaymentNumber", ref _PaymentNumber, value); }
        }
        private string _PaymentBatchName;
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
        [Size(1000)]
        private string _InvoiceLineDesc;
        public string InvoiceLineDesc
        {
            get { return _InvoiceLineDesc; }
            set { SetPropertyValue("InvoiceLineDesc", ref _InvoiceLineDesc, value); }
        }
        private decimal _DistnLineGstAud;
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal DistnLineGstAud
        {
            get { return _DistnLineGstAud; }
            set { SetPropertyValue("DistnLineGstAud", ref _DistnLineGstAud, value); }
        }
        private decimal _DistnLineGstFx;
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal DistnLineGstFx
        {
            get { return _DistnLineGstFx; }
            set { SetPropertyValue("DistnLineGstFx", ref _DistnLineGstFx, value); }
        }
        public Account Account
        {
            get 
            {
                if (BankAccount == null) return null;
                return BankAccount.Account; 
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
       
    }
}
