using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Model;
using CashDiscipline.Module.Attributes;
using Xafology.ExpressApp.BatchDelete;

namespace CashDiscipline.Module.BusinessObjects.AccountsReceivable
{
    [DefaultProperty("Oid")]
    [ModelDefault("IsFooterVisible", "True")]
    [AutoColumnWidth(false)]
    [ModelDefault("ImageName", "BO_List")]
    [BatchDelete(isVisible: true)]
    public class ArOpenInvoices : BaseObject
    {
        public ArOpenInvoices(Session session)
            : base(session)
        {
        }

        DateTime fAsAtDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime AsAtDate
        {
            get { return fAsAtDate; }
            set { SetPropertyValue<DateTime>("AsAtDate", ref fAsAtDate, value); }
        }
        string fCustomerName;
        [Size(255)]
        public string CustomerName
        {
            get { return fCustomerName; }
            set { SetPropertyValue<string>("CustomerName", ref fCustomerName, value); }
        }
        string fCustomerNumber;
        [Size(30)]
        public string CustomerNumber
        {
            get { return fCustomerNumber; }
            set { SetPropertyValue<string>("CustomerNumber", ref fCustomerNumber, value); }
        }
        DateTime fTrxDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime TrxDate
        {
            get { return fTrxDate; }
            set { SetPropertyValue<DateTime>("TrxDate", ref fTrxDate, value); }
        }
        DateTime fDueDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime DueDate
        {
            get { return fDueDate; }
            set { SetPropertyValue<DateTime>("DueDate", ref fDueDate, value); }
        }
        string fTrxType;
        [Size(10)]
        public string TrxType
        {
            get { return fTrxType; }
            set { SetPropertyValue<string>("TrxType", ref fTrxType, value); }
        }
        string fTrxNumber;
        [Size(255)]
        public string TrxNumber
        {
            get { return fTrxNumber; }
            set { SetPropertyValue<string>("TrxNumber", ref fTrxNumber, value); }
        }
        DateTime fApplyDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime ApplyDate
        {
            get { return fApplyDate; }
            set { SetPropertyValue<DateTime>("ApplyDate", ref fApplyDate, value); }
        }
        string fTrxStatus;
        [Size(10)]
        public string TrxStatus
        {
            get { return fTrxStatus; }
            set { SetPropertyValue<string>("TrxStatus", ref fTrxStatus, value); }
        }
        string fSalesOrder;
        [Size(255)]
        public string SalesOrder
        {
            get { return fSalesOrder; }
            set { SetPropertyValue<string>("SalesOrder", ref fSalesOrder, value); }
        }
        string fReference;
        [Size(255)]
        public string Reference
        {
            get { return fReference; }
            set { SetPropertyValue<string>("Reference", ref fReference, value); }
        }

        string fCurrencyCode;
        [Size(20)]
        public string CurrencyCode
        {
            get { return fCurrencyCode; }
            set { SetPropertyValue<string>("CurrencyCode", ref fCurrencyCode, value); }
        }

        double fOriginalAmount;
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public double OriginalAmount
        {
            get { return fOriginalAmount; }
            set { SetPropertyValue<double>("OriginalAmount", ref fOriginalAmount, value); }
        }
        double fBalanceDue;
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public double BalanceDue
        {
            get { return fBalanceDue; }
            set { SetPropertyValue<double>("BalanceDue", ref fBalanceDue, value); }
        }
        double fCreditedAmount;
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public double CreditedAmount
        {
            get { return fCreditedAmount; }
            set { SetPropertyValue<double>("CreditedAmount", ref fCreditedAmount, value); }
        }
        double fAdjustmentAmount;
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public double AdjustmentAmount
        {
            get { return fAdjustmentAmount; }
            set { SetPropertyValue<double>("AdjustmentAmount", ref fAdjustmentAmount, value); }
        }
        double fAppliedAmount;
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public double AppliedAmount
        {
            get { return fAppliedAmount; }
            set { SetPropertyValue<double>("AppliedAmount", ref fAppliedAmount, value); }
        }
        double fOriginalReceiptAmount;
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public double OriginalReceiptAmount
        {
            get { return fOriginalReceiptAmount; }
            set { SetPropertyValue<double>("OriginalReceiptAmount", ref fOriginalReceiptAmount, value); }
        }
        double fOverdueDays;
        public double OverdueDays
        {
            get { return fOverdueDays; }
            set { SetPropertyValue<double>("OverdueDays", ref fOverdueDays, value); }
        }
    }

}
