using CashDiscipline.Module.Attributes;
using DevExpress.ExpressApp.Model;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CashDiscipline.Module.BusinessObjects.AccountsPayable
{
    [DefaultProperty("Oid")]
    [ModelDefault("IsFooterVisible", "True")]
    [AutoColumnWidth(false)]
    [ModelDefault("ImageName", "BO_List")]
    public class ApVendorSitePaymentTerms : XPLiteObject
    {
        public ApVendorSitePaymentTerms(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        Guid fOid;
        [Key]
        public Guid Oid
        {
            get { return fOid; }
            set { SetPropertyValue<Guid>("Oid", ref fOid, value); }
        }

        DateTime fAsAtDate;
        [Persistent(@"As At Date")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime AsAtDate
        {
            get { return fAsAtDate; }
            set { SetPropertyValue<DateTime>("AsAtDate", ref fAsAtDate, value); }
        }

        string fVendorName;
        [Size(255)]
        [Persistent(@"Vendor Name")]
        public string VendorName
        {
            get { return fVendorName; }
            set { SetPropertyValue<string>("VendorName", ref fVendorName, value); }
        }
        string fVendorSiteCode;
        [Size(50)]
        [Persistent(@"Vendor Site Code")]
        public string VendorSiteCode
        {
            get { return fVendorSiteCode; }
            set { SetPropertyValue<string>("VendorSiteCode", ref fVendorSiteCode, value); }
        }
        string fVendortypelookupcode;
        [Size(50)]
        [Persistent(@"Vendor type lookup code")]
        public string Vendortypelookupcode
        {
            get { return fVendortypelookupcode; }
            set { SetPropertyValue<string>("Vendortypelookupcode", ref fVendortypelookupcode, value); }
        }
        string fOrg;
        [Size(50)]
        public string Org
        {
            get { return fOrg; }
            set { SetPropertyValue<string>("Org", ref fOrg, value); }
        }
        DateTime fCreationdate;
        [Persistent(@"Creation date")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime CreationDate
        {
            get { return fCreationdate; }
            set { SetPropertyValue<DateTime>("CreationDate", ref fCreationdate, value); }
        }
        string fVendorNumber;
        [Size(50)]
        [Persistent(@"Vendor Number")]
        public string VendorNumber
        {
            get { return fVendorNumber; }
            set { SetPropertyValue<string>("VendorNumber", ref fVendorNumber, value); }
        }
        string fVendorPaymentTerms;
        [Size(50)]
        [Persistent(@"Vendor Payment Terms")]
        public string VendorPaymentTerms
        {
            get { return fVendorPaymentTerms; }
            set { SetPropertyValue<string>("VendorPaymentTerms", ref fVendorPaymentTerms, value); }
        }
        string fVendorSitePaymentTerms;
        [Size(50)]
        [Persistent(@"Vendor Site Payment Terms")]
        public string VendorSitePaymentTerms
        {
            get { return fVendorSitePaymentTerms; }
            set { SetPropertyValue<string>("VendorSitePaymentTerms", ref fVendorSitePaymentTerms, value); }
        }
        DateTime fVendorEnddateactive;
        [Persistent(@"Vendor End date active")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime VendorEndDateActive
        {
            get { return fVendorEnddateactive; }
            set { SetPropertyValue<DateTime>("VendorEndDateActive", ref fVendorEnddateactive, value); }
        }
        DateTime fSiteInactivedate;
        [Persistent(@"Site Inactive date")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime SiteInactiveDate
        {
            get { return fSiteInactivedate; }
            set { SetPropertyValue<DateTime>("SiteInactiveDate", ref fSiteInactivedate, value); }
        }
        long fVendorid;
        [Persistent(@"Vendor id")]
        public long Vendorid
        {
            get { return fVendorid; }
            set { SetPropertyValue<long>("Vendorid", ref fVendorid, value); }
        }
        long fVendorsiteid;
        [Persistent(@"Vendor site id")]
        public long Vendorsiteid
        {
            get { return fVendorsiteid; }
            set { SetPropertyValue<long>("Vendorsiteid", ref fVendorsiteid, value); }
        }
        DateTime fLastupdatedate;
        [Persistent(@"Last update date")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime LastUpdateDate
        {
            get { return fLastupdatedate; }
            set { SetPropertyValue<DateTime>("LastUpdateDate", ref fLastupdatedate, value); }
        }
        DateTime fVendorCreationdate;
        [Persistent(@"Vendor Creation date")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
        public DateTime VendorCreationDate
        {
            get { return fVendorCreationdate; }
            set { SetPropertyValue<DateTime>("VendorCreationDate", ref fVendorCreationdate, value); }
        }
        string fVatregistrationnum;
        [Size(50)]
        [Persistent(@"Vat registration num")]
        public string Vatregistrationnum
        {
            get { return fVatregistrationnum; }
            set { SetPropertyValue<string>("Vatregistrationnum", ref fVatregistrationnum, value); }
        }
        string fState;
        [Size(255)]
        public string State
        {
            get { return fState; }
            set { SetPropertyValue<string>("State", ref fState, value); }
        }
        string fCountry;
        [Size(255)]
        public string Country
        {
            get { return fCountry; }
            set { SetPropertyValue<string>("Country", ref fCountry, value); }
        }
        string fAddressline1;
        [Size(255)]
        [Persistent(@"Address line1")]
        public string Addressline1
        {
            get { return fAddressline1; }
            set { SetPropertyValue<string>("Addressline1", ref fAddressline1, value); }
        }
        string fAddresslinesalt;
        [Size(255)]
        [Persistent(@"Address lines alt")]
        public string Addresslinesalt
        {
            get { return fAddresslinesalt; }
            set { SetPropertyValue<string>("Addresslinesalt", ref fAddresslinesalt, value); }
        }
        string fAddressline2;
        [Size(255)]
        [Persistent(@"Address line2")]
        public string Addressline2
        {
            get { return fAddressline2; }
            set { SetPropertyValue<string>("Addressline2", ref fAddressline2, value); }
        }
        string fAddressline3;
        [Size(255)]
        [Persistent(@"Address line3")]
        public string Addressline3
        {
            get { return fAddressline3; }
            set { SetPropertyValue<string>("Addressline3", ref fAddressline3, value); }
        }
        string fCity;
        [Size(255)]
        public string City
        {
            get { return fCity; }
            set { SetPropertyValue<string>("City", ref fCity, value); }
        }
        string fZip;
        [Size(50)]
        public string Zip
        {
            get { return fZip; }
            set { SetPropertyValue<string>("Zip", ref fZip, value); }
        }
        string fVatcode;
        [Size(50)]
        [Persistent(@"Vat code")]
        public string Vatcode
        {
            get { return fVatcode; }
            set { SetPropertyValue<string>("Vatcode", ref fVatcode, value); }
        }
    }
}
