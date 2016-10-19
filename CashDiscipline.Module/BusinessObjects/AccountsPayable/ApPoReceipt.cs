using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.ExpressApp.Model;
using CashDiscipline.Module.Attributes;

namespace CashDiscipline.Module.BusinessObjects.AccountsPayable
{
    [DefaultProperty("Oid")]
    [ModelDefault("IsFooterVisible", "True")]
    [AutoColumnWidth(false)]
    [ModelDefault("ImageName", "BO_List")]
    public class ApPoReceipt : XPLiteObject
    {
        public ApPoReceipt(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        double _PoNum;
        [Persistent(@"Po Num")]
        public double PoNum
        {
            get { return _PoNum; }
            set { SetPropertyValue<double>("PoNum", ref _PoNum, value); }
        }
        double _ReceiptNum;
        [Persistent(@"Receipt Num")]
        public double ReceiptNum
        {
            get { return _ReceiptNum; }
            set { SetPropertyValue<double>("ReceiptNum", ref _ReceiptNum, value); }
        }
        double _QuantityReceived;
        [Persistent(@"Quantity Received")]
        public double QuantityReceived
        {
            get { return _QuantityReceived; }
            set { SetPropertyValue<double>("QuantityReceived", ref _QuantityReceived, value); }
        }
        string _ReceivedBy;
        [Size(255)]
        [Persistent(@"Received By")]
        public string ReceivedBy
        {
            get { return _ReceivedBy; }
            set { SetPropertyValue<string>("ReceivedBy", ref _ReceivedBy, value); }
        }
        double _ReqNum;
        [Persistent(@"Req Num")]
        public double ReqNum
        {
            get { return _ReqNum; }
            set { SetPropertyValue<double>("ReqNum", ref _ReqNum, value); }
        }
        string _RequesterName;
        [Size(255)]
        [Persistent(@"Requester Name")]
        public string RequesterName
        {
            get { return _RequesterName; }
            set { SetPropertyValue<string>("RequesterName", ref _RequesterName, value); }
        }
        string _VendorName;
        [Size(255)]
        [Persistent(@"Vendor Name")]
        public string VendorName
        {
            get { return _VendorName; }
            set { SetPropertyValue<string>("VendorName", ref _VendorName, value); }
        }
        string _VendorSiteCode;
        [Size(255)]
        [Persistent(@"Vendor Site Code")]
        public string VendorSiteCode
        {
            get { return _VendorSiteCode; }
            set { SetPropertyValue<string>("VendorSiteCode", ref _VendorSiteCode, value); }
        }
        string _TransactionType;
        [Size(255)]
        [Persistent(@"Transaction Type")]
        public string TransactionType
        {
            get { return _TransactionType; }
            set { SetPropertyValue<string>("TransactionType", ref _TransactionType, value); }
        }
        string _ItemDescription;
        [Size(255)]
        [Persistent(@"Item Description")]
        public string ItemDescription
        {
            get { return _ItemDescription; }
            set { SetPropertyValue<string>("ItemDescription", ref _ItemDescription, value); }
        }
        double _LineUnitPriceORIGCUR;
        [Persistent(@"Line Unit Price ORIG CUR")]
        public double LineUnitPriceORIGCUR
        {
            get { return _LineUnitPriceORIGCUR; }
            set { SetPropertyValue<double>("LineUnitPriceORIGCUR", ref _LineUnitPriceORIGCUR, value); }
        }
        double _UnitPriceAUD;
        [Persistent(@"Unit Price AUD")]
        public double UnitPriceAUD
        {
            get { return _UnitPriceAUD; }
            set { SetPropertyValue<double>("UnitPriceAUD", ref _UnitPriceAUD, value); }
        }
        string _MajorCategory;
        [Size(255)]
        [Persistent(@"Major Category")]
        public string MajorCategory
        {
            get { return _MajorCategory; }
            set { SetPropertyValue<string>("MajorCategory", ref _MajorCategory, value); }
        }
        string _MinorCategory;
        [Size(255)]
        [Persistent(@"Minor Category")]
        public string MinorCategory
        {
            get { return _MinorCategory; }
            set { SetPropertyValue<string>("MinorCategory", ref _MinorCategory, value); }
        }
        double _QuantityOrdered;
        [Persistent(@"Quantity Ordered")]
        public double QuantityOrdered
        {
            get { return _QuantityOrdered; }
            set { SetPropertyValue<double>("QuantityOrdered", ref _QuantityOrdered, value); }
        }
        double _DistrAmtOrderedOrigCur;
        [Persistent(@"Distr Amt Ordered Orig Cur")]
        public double DistrAmtOrderedOrigCur
        {
            get { return _DistrAmtOrderedOrigCur; }
            set { SetPropertyValue<double>("DistrAmtOrderedOrigCur", ref _DistrAmtOrderedOrigCur, value); }
        }
        string _Company;
        [Size(10)]
        public string Company
        {
            get { return _Company; }
            set { SetPropertyValue<string>("Company", ref _Company, value); }
        }
        string _Account;
        [Size(10)]
        public string Account
        {
            get { return _Account; }
            set { SetPropertyValue<string>("Account", ref _Account, value); }
        }
        string _CostCentre;
        [Size(10)]
        [Persistent(@"Cost Centre")]
        public string CostCentre
        {
            get { return _CostCentre; }
            set { SetPropertyValue<string>("CostCentre", ref _CostCentre, value); }
        }
        DateTime _ReceiptDate;
        [Persistent(@"Receipt Date")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime ReceiptDate
        {
            get { return _ReceiptDate; }
            set { SetPropertyValue<DateTime>("ReceiptDate", ref _ReceiptDate, value); }
        }
        string _InvoiceFromReceipt;
        [Size(255)]
        [Persistent(@"Invoice From Receipt")]
        public string InvoiceFromReceipt
        {
            get { return _InvoiceFromReceipt; }
            set { SetPropertyValue<string>("InvoiceFromReceipt", ref _InvoiceFromReceipt, value); }
        }
        string _CommentsFromReceipt;
        [Size(255)]
        [Persistent(@"Comments From Receipt")]
        public string CommentsFromReceipt
        {
            get { return _CommentsFromReceipt; }
            set { SetPropertyValue<string>("CommentsFromReceipt", ref _CommentsFromReceipt, value); }
        }
        string _ProjectNumber;
        [Size(255)]
        [Persistent(@"Project Number")]
        public string ProjectNumber
        {
            get { return _ProjectNumber; }
            set { SetPropertyValue<string>("ProjectNumber", ref _ProjectNumber, value); }
        }
        string _ProjectType;
        [Size(255)]
        [Persistent(@"Project Type")]
        public string ProjectType
        {
            get { return _ProjectType; }
            set { SetPropertyValue<string>("ProjectType", ref _ProjectType, value); }
        }
        string _ProjectStatusCode;
        [Size(255)]
        [Persistent(@"Project Status Code")]
        public string ProjectStatusCode
        {
            get { return _ProjectStatusCode; }
            set { SetPropertyValue<string>("ProjectStatusCode", ref _ProjectStatusCode, value); }
        }
        double _Rate;
        public double Rate
        {
            get { return _Rate; }
            set { SetPropertyValue<double>("Rate", ref _Rate, value); }
        }
        DateTime _PoCreationDate;
        [Persistent(@"Po Creation Date")]
        [ModelDefault("EditMask", "dd-MMM-yy"), ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime PoCreationDate
        {
            get { return _PoCreationDate; }
            set { SetPropertyValue<DateTime>("PoCreationDate", ref _PoCreationDate, value); }
        }
        string _ProjectManager;
        [Size(255)]
        [Persistent(@"Project Manager")]
        public string ProjectManager
        {
            get { return _ProjectManager; }
            set { SetPropertyValue<string>("ProjectManager", ref _ProjectManager, value); }
        }
        double _LineNum;
        [Persistent(@"Line Num")]
        public double LineNum
        {
            get { return _LineNum; }
            set { SetPropertyValue<double>("LineNum", ref _LineNum, value); }
        }
        double _DistributionNum;
        [Persistent(@"Distribution Num")]
        public double DistributionNum
        {
            get { return _DistributionNum; }
            set { SetPropertyValue<double>("DistributionNum", ref _DistributionNum, value); }
        }
        string _PoHeaderClosedCode;
        [Size(255)]
        [Persistent(@"Po Header Closed Code")]
        public string PoHeaderClosedCode
        {
            get { return _PoHeaderClosedCode; }
            set { SetPropertyValue<string>("PoHeaderClosedCode", ref _PoHeaderClosedCode, value); }
        }
        string _LineClosedCode;
        [Size(255)]
        [Persistent(@"Line Closed Code")]
        public string LineClosedCode
        {
            get { return _LineClosedCode; }
            set { SetPropertyValue<string>("LineClosedCode", ref _LineClosedCode, value); }
        }
        string _ShipmentClosedCode;
        [Size(255)]
        [Persistent(@"Shipment Closed Code")]
        public string ShipmentClosedCode
        {
            get { return _ShipmentClosedCode; }
            set { SetPropertyValue<string>("ShipmentClosedCode", ref _ShipmentClosedCode, value); }
        }
        double _QuantityDelivered;
        [Persistent(@"Quantity Delivered")]
        public double QuantityDelivered
        {
            get { return _QuantityDelivered; }
            set { SetPropertyValue<double>("QuantityDelivered", ref _QuantityDelivered, value); }
        }
        double _QuantityBilled;
        [Persistent(@"Quantity Billed")]
        public double QuantityBilled
        {
            get { return _QuantityBilled; }
            set { SetPropertyValue<double>("QuantityBilled", ref _QuantityBilled, value); }
        }
        double _DistrAmtDeliveredORIGCUR;
        [Persistent(@"Distr Amt Delivered ORIG CUR")]
        public double DistrAmtDeliveredORIGCUR
        {
            get { return _DistrAmtDeliveredORIGCUR; }
            set { SetPropertyValue<double>("DistrAmtDeliveredORIGCUR", ref _DistrAmtDeliveredORIGCUR, value); }
        }
        double _DistrAmtBilledORIGCUR;
        [Persistent(@"Distr Amt Billed ORIG CUR")]
        public double DistrAmtBilledORIGCUR
        {
            get { return _DistrAmtBilledORIGCUR; }
            set { SetPropertyValue<double>("DistrAmtBilledORIGCUR", ref _DistrAmtBilledORIGCUR, value); }
        }

        string _AccountDescription;
        [Size(255)]
        [Persistent(@"Account Description")]
        public string AccountDescription
        {
            get { return _AccountDescription; }
            set { SetPropertyValue<string>("AccountDescription", ref _AccountDescription, value); }
        }

        string _CostCentreDescription;
        [Size(255)]
        [Persistent(@"Cost Centre Description")]
        public string CostCentreDescription
        {
            get { return _CostCentreDescription; }
            set { SetPropertyValue<string>("CostCentreDescription", ref _CostCentreDescription, value); }
        }

        string _AccountActivity;
        [Size(255)]
        public string AccountActivity
        {
            get { return _AccountActivity; }
            set { SetPropertyValue<string>("AccountActivity", ref _AccountActivity, value); }
        }
        string _CostCentreActivity;
        [Size(255)]
        public string CostCentreActivity
        {
            get { return _CostCentreActivity; }
            set { SetPropertyValue<string>("CostCentreActivity", ref _CostCentreActivity, value); }
        }
        string _Activity;
        [Size(255)]
        public string Activity
        {
            get { return _Activity; }
            set { SetPropertyValue<string>("Activity", ref _Activity, value); }
        }
        string _VendorNum;
        [Size(50)]
        [Persistent(@"Vendor Num")]
        public string VendorNum
        {
            get { return _VendorNum; }
            set { SetPropertyValue<string>("VendorNum", ref _VendorNum, value); }
        }
        DateTime _DbTimestamp;
        public DateTime DbTimestamp
        {
            get { return _DbTimestamp; }
            set { SetPropertyValue<DateTime>("DbTimestamp", ref _DbTimestamp, value); }
        }
        string _PaymentTerms;
        [Size(50)]
        [Persistent(@"Payment Terms")]
        public string PaymentTerms
        {
            get { return _PaymentTerms; }
            set { SetPropertyValue<string>("PaymentTerms", ref _PaymentTerms, value); }
        }
        double _OrigCurReceived;
        [Persistent(@"Orig Cur Received")]
        public double OrigCurReceived
        {
            get { return _OrigCurReceived; }
            set { SetPropertyValue<double>("OrigCurReceived", ref _OrigCurReceived, value); }
        }
        double _AudCurReceived;
        [Persistent(@"Aud Cur Received")]
        public double AudCurReceived
        {
            get { return _AudCurReceived; }
            set { SetPropertyValue<double>("AudCurReceived", ref _AudCurReceived, value); }
        }
        Guid _Oid;
        [Key]
        public Guid Oid
        {
            get { return _Oid; }
            set { SetPropertyValue<Guid>("Oid", ref _Oid, value); }
        }
        int _IsConfirmed;
        public int IsConfirmed
        {
            get { return _IsConfirmed; }
            set { SetPropertyValue<int>("IsConfirmed", ref _IsConfirmed, value); }
        }
    }

}
