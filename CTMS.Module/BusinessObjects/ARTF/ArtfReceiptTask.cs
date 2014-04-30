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
    public class ArtfReceiptTask : ArtfTask
    { // You can use a different base persistent class based on your requirements (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public ArtfReceiptTask(Session session)
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
        //public string PersistentProperty {
        //    get { return _PersistentProperty; }
        //    set { SetPropertyValue("PersistentProperty", ref _PersistentProperty, value); }
        //}

        //[Action(Caption = "My Action Method", ConfirmationMessage = "Are you sure?", ImageName = "Attention", AutoCommit = True)]
        //public void ActionMethod() {
        //    // Define an Action in the UI that can execute your custom code or invoke a dialog for processing information gathered from end-users (http://documentation.devexpress.com/#Xaf/CustomDocument2619).
        //}

        decimal _Amount;
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal Amount
        {
            get { return _Amount; }
            set { SetPropertyValue<decimal>("Amount", ref _Amount, value); }
        }

        private bool _IsReversed;
        [ToolTip("Whether the entire receipt has been reversed")]
        public bool IsReversed
        {
            get
            {
                return _IsReversed;
            }
            set
            {
                SetPropertyValue("IsReversed", ref _IsReversed, value);
            }
        }
        public override void AssignDefaultTaskOwner()
        {
            AssignDefaultTaskOwner(TaskTypeEnum.Receipt);
        }

        
    }
}
