using CashDiscipline.Module.Attributes;
using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.BatchDelete;

namespace CashDiscipline.Module.BusinessObjects.AccountsPayable
{
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [DefaultProperty("Oid")]
    [ModelDefault("IsFooterVisible", "True")]
    [AutoColumnWidth(false)]
    [BatchDelete(isVisible: true)]
    [ModelDefault("ImageName", "BO_List")]
    public class ApInvoiceBalance : BaseObject
    {
        public ApInvoiceBalance(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }


        double fRequestId;
        [ModelDefault("DisplayFormat", "f0")]
        public double RequestId
        {
            get { return fRequestId; }
            set { SetPropertyValue<double>("RequestId", ref fRequestId, value); }
        }
        DateTime fAsAtDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime AsAtDate
        {
            get { return fAsAtDate; }
            set { SetPropertyValue<DateTime>("AsAtDate", ref fAsAtDate, value); }
        }
        string fSupplier;
        [Size(255)]
        public string Supplier
        {
            get { return fSupplier; }
            set { SetPropertyValue<string>("Supplier", ref fSupplier, value); }
        }
        string fInvoiceNumber;
        [Size(255)]
        public string InvoiceNumber
        {
            get { return fInvoiceNumber; }
            set { SetPropertyValue<string>("InvoiceNumber", ref fInvoiceNumber, value); }
        }
        string fLiabilityCompany;
        [Size(255)]
        public string LiabilityCompany
        {
            get { return fLiabilityCompany; }
            set { SetPropertyValue<string>("LiabilityCompany", ref fLiabilityCompany, value); }
        }
        string fLiabilityAccount;
        [Size(255)]
        public string LiabilityAccount
        {
            get { return fLiabilityAccount; }
            set { SetPropertyValue<string>("LiabilityAccount", ref fLiabilityAccount, value); }
        }
        string fLiabilityCostCentre;
        [Size(255)]
        public string LiabilityCostCentre
        {
            get { return fLiabilityCostCentre; }
            set { SetPropertyValue<string>("LiabilityCostCentre", ref fLiabilityCostCentre, value); }
        }
        string fLiabilityIntercompany;
        [Size(255)]
        public string LiabilityIntercompany
        {
            get { return fLiabilityIntercompany; }
            set { SetPropertyValue<string>("LiabilityIntercompany", ref fLiabilityIntercompany, value); }
        }
        DateTime fInvoiceDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime InvoiceDate
        {
            get { return fInvoiceDate; }
            set { SetPropertyValue<DateTime>("InvoiceDate", ref fInvoiceDate, value); }
        }
        DateTime fGlDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime GlDate
        {
            get { return fGlDate; }
            set { SetPropertyValue<DateTime>("GlDate", ref fGlDate, value); }
        }
        string fTrxCurrencyCode;
        [Size(255)]
        public string TrxCurrencyCode
        {
            get { return fTrxCurrencyCode; }
            set { SetPropertyValue<string>("TrxCurrencyCode", ref fTrxCurrencyCode, value); }
        }
        double fInvoiceAmountAud;
        public double InvoiceAmountAud
        {
            get { return fInvoiceAmountAud; }
            set { SetPropertyValue<double>("InvoiceAmountAud", ref fInvoiceAmountAud, value); }
        }
        double fInvoiceRemainingAmountAud;
        public double InvoiceRemainingAmountAud
        {
            get { return fInvoiceRemainingAmountAud; }
            set { SetPropertyValue<double>("InvoiceRemainingAmountAud", ref fInvoiceRemainingAmountAud, value); }
        }
        double fLineDistributionRemainingAmountAud;
        public double LineDistributionRemainingAmountAud
        {
            get { return fLineDistributionRemainingAmountAud; }
            set { SetPropertyValue<double>("LineDistributionRemainingAmountAud", ref fLineDistributionRemainingAmountAud, value); }
        }
        string fOpex_Capex_Other;
        [Size(255)]
        public string Opex_Capex_Other
        {
            get { return fOpex_Capex_Other; }
            set { SetPropertyValue<string>("Opex_Capex_Other", ref fOpex_Capex_Other, value); }
        }
        string fExtraDetail1;
        [Size(255)]
        public string ExtraDetail1
        {
            get { return fExtraDetail1; }
            set { SetPropertyValue<string>("ExtraDetail1", ref fExtraDetail1, value); }
        }
        string fExtraDetail2;
        [Size(255)]
        public string ExtraDetail2
        {
            get { return fExtraDetail2; }
            set { SetPropertyValue<string>("ExtraDetail2", ref fExtraDetail2, value); }
        }
        string fExpenseCompany;
        [Size(255)]
        public string ExpenseCompany
        {
            get { return fExpenseCompany; }
            set { SetPropertyValue<string>("ExpenseCompany", ref fExpenseCompany, value); }
        }
        string fExpenseAccount;
        [Size(255)]
        public string ExpenseAccount
        {
            get { return fExpenseAccount; }
            set { SetPropertyValue<string>("ExpenseAccount", ref fExpenseAccount, value); }
        }
        string fExpenseCostCentre;
        [Size(255)]
        public string ExpenseCostCentre
        {
            get { return fExpenseCostCentre; }
            set { SetPropertyValue<string>("ExpenseCostCentre", ref fExpenseCostCentre, value); }
        }
        string fExpenseProduct;
        [Size(255)]
        public string ExpenseProduct
        {
            get { return fExpenseProduct; }
            set { SetPropertyValue<string>("ExpenseProduct", ref fExpenseProduct, value); }
        }
        string fExpenseSalesChannel;
        [Size(255)]
        public string ExpenseSalesChannel
        {
            get { return fExpenseSalesChannel; }
            set { SetPropertyValue<string>("ExpenseSalesChannel", ref fExpenseSalesChannel, value); }
        }
        string fExpenseCountry;
        [Size(255)]
        public string ExpenseCountry
        {
            get { return fExpenseCountry; }
            set { SetPropertyValue<string>("ExpenseCountry", ref fExpenseCountry, value); }
        }
        string fExpenseIntercompany;
        [Size(255)]
        public string ExpenseIntercompany
        {
            get { return fExpenseIntercompany; }
            set { SetPropertyValue<string>("ExpenseIntercompany", ref fExpenseIntercompany, value); }
        }
        string fExpenseProject;
        [Size(255)]
        public string ExpenseProject
        {
            get { return fExpenseProject; }
            set { SetPropertyValue<string>("ExpenseProject", ref fExpenseProject, value); }
        }
        string fExpenseLocation;
        [Size(255)]
        public string ExpenseLocation
        {
            get { return fExpenseLocation; }
            set { SetPropertyValue<string>("ExpenseLocation", ref fExpenseLocation, value); }
        }
        string fInvoiceDescription;
        [Size(255)]
        public string InvoiceDescription
        {
            get { return fInvoiceDescription; }
            set { SetPropertyValue<string>("InvoiceDescription", ref fInvoiceDescription, value); }
        }
        Activity fAccountActivity;
        public Activity AccountActivity
        {
            get { return fAccountActivity; }
            set { SetPropertyValue("AccountActivity", ref fAccountActivity, value); }
        }
        Activity fCostCentreActivity;
        public Activity CostCentreActivity
        {
            get { return fCostCentreActivity; }
            set { SetPropertyValue("CostCentreActivity", ref fCostCentreActivity, value); }
        }
        Activity fActivity;
        [Size(255)]
        public Activity Activity
        {
            get { return fActivity; }
            set { SetPropertyValue("Activity", ref fActivity, value); }
        }
        DateTime fDueDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime DueDate
        {
            get { return fDueDate; }
            set { SetPropertyValue<DateTime>("DueDate", ref fDueDate, value); }
        }
        string fPaymentTerm;
        [Size(50)]
        public string PaymentTerm
        {
            get { return fPaymentTerm; }
            set { SetPropertyValue<string>("PaymentTerm", ref fPaymentTerm, value); }
        }
        DateTime fInvoiceReceivedDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime InvoiceReceivedDate
        {
            get { return fInvoiceReceivedDate; }
            set { SetPropertyValue<DateTime>("InvoiceReceivedDate", ref fInvoiceReceivedDate, value); }
        }
        string fVendorType;
        [Size(255)]
        public string VendorType
        {
            get { return fVendorType; }
            set { SetPropertyValue<string>("VendorType", ref fVendorType, value); }
        }
        string fCategory1;
        [Size(255)]
        public string Category1
        {
            get { return fCategory1; }
            set { SetPropertyValue<string>("Category1", ref fCategory1, value); }
        }
        string fCategory2;
        [Size(255)]
        public string Category2
        {
            get { return fCategory2; }
            set { SetPropertyValue<string>("Category2", ref fCategory2, value); }
        }
        double fEnteredOrigAmount;
        public double EnteredOrigAmount
        {
            get { return fEnteredOrigAmount; }
            set { SetPropertyValue<double>("EnteredOrigAmount", ref fEnteredOrigAmount, value); }
        }
        double fEnteredOrigRemainingAmount;
        public double EnteredOrigRemainingAmount
        {
            get { return fEnteredOrigRemainingAmount; }
            set { SetPropertyValue<double>("EnteredOrigRemainingAmount", ref fEnteredOrigRemainingAmount, value); }
        }
    }
}
