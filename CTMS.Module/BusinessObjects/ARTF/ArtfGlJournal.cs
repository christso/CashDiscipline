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

// With XPO, the data model is declared by classes (so-called Persistent Objects) that will define the database structure, and consequently, the user interface (http://documentation.devexpress.com/#Xaf/CustomDocument2600).
namespace CTMS.Module.BusinessObjects.Artf
{
    // Specify various UI options for your persistent class and its properties using a declarative approach via built-in attributes (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
    //[ImageName("BO_Contact")]
    //[DefaultProperty("PersistentProperty")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewAndDetailView, true, NewItemRowPosition.Top)]
    public class ArtfGlJournal : BaseObject
    { // You can use a different base persistent class based on your requirements (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public ArtfGlJournal(Session session)
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

        //private string _PersistentProperty;
        //[XafDisplayName("My Persistent Property")]
        //[ToolTip("Specify a hint message for a property in the UI.")]
        //[ModelDefault("EditMask", "(000)-00")]
        //[Index(0), VisibleInListView(true), VisibleInDetailView(false), VisibleInLookupListView(true)]
        //[RuleRequiredField(DefaultContexts.Save)]
        private DateTime _GlDate;
        public DateTime GlDate
        {
            get { return _GlDate; }
            set { SetPropertyValue("GlDate", ref _GlDate, value); }
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
        private decimal _NetAmount;
        public decimal NetAmount
        {
            get { return _NetAmount; }
            set { SetPropertyValue("NetAmount", ref _NetAmount, value); }
        }

        private ArtfGlJournalTask _ArtfTask;
        [Association("ArtfTask-ArtfGlJournals")]
        public ArtfGlJournalTask ArtfTask
        {
            get { return _ArtfTask; }
            set { SetPropertyValue("ArtfTask", ref _ArtfTask, value); }
        }

        private string _GlDescription;
        [Size(SizeAttribute.Unlimited)]
        public string GlDescription
        {
            get { return _GlDescription; }
            set { SetPropertyValue("GlDescription", ref _GlDescription, value); }
        }
    }
}
