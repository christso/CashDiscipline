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
    public class ArtfGlJournalTask : ArtfTask
    { // You can use a different base persistent class based on your requirements (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public ArtfGlJournalTask(Session session)
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

        private decimal _GlNetDebitAmount;
        private ArtfGlCode _GlDebitGlCode;
        private ArtfGlCode _GlCreditGlCode;

        public ArtfGlCode GlCreditGlCode
        {
            get
            {
                return _GlCreditGlCode;
            }
            set
            {
                SetPropertyValue("GlCreditGlCode", ref _GlCreditGlCode, value);
            }
        }

        public ArtfGlCode GlDebitGlCode
        {
            get
            {
                return _GlDebitGlCode;
            }
            set
            {
                SetPropertyValue("GlDebitGlCode", ref _GlDebitGlCode, value);
            }
        }
        public decimal GlNetDebitAmount
        {
            get
            {
                return _GlNetDebitAmount;
            }
            set
            {
                SetPropertyValue("GlNetDebitAmount", ref _GlNetDebitAmount, value);
            }
        }

        [Association("ArtfTask-ArtfGlJournals")]
        public XPCollection<ArtfGlJournal> ArtfGlJournals
        {
            get
            {
                return GetCollection<ArtfGlJournal>("ArtfGlJournals");
            }
        }

        public override void AssignDefaultTaskOwner()
        {
            AssignDefaultTaskOwner(TaskTypeEnum.GenJournal);
        }

    }
}
