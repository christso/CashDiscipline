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
using CTMS.Module.BusinessObjects.Cash;

// With XPO, the data model is declared by classes (so-called Persistent Objects) that will define the database structure, and consequently, the user interface (http://documentation.devexpress.com/#Xaf/CustomDocument2600).
namespace CTMS.Module.BusinessObjects.Payments
{
    // Specify various UI options for your persistent class and its properties using a declarative approach via built-in attributes (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
    //[ImageName("BO_Contact")]
    //[DefaultProperty("PersistentProperty")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewAndDetailView, true, NewItemRowPosition.Top)]
    public class CreditAccount : BaseObject
    { // You can use a different base persistent class based on your requirements (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public CreditAccount(Session session)
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

        // Fields...

        private string _BankAccountNumber;
        private Account _LinkedAccount;
        private string _ShortName;
        private string _EntityName;

        [RuleRequiredField("RuleRequiredField for CreditAccount.ShortName", DefaultContexts.Save)]
        public string ShortName
        {
            get
            {
                return _ShortName;
            }
            set
            {
                SetPropertyValue("ShortName", ref _ShortName, value);
            }
        }

        public string EntityName
        {
            get
            {
                return _EntityName;
            }
            set
            {
                SetPropertyValue("BeneficiaryName", ref _EntityName, value);
            }
        }

        [RuleRequiredField("RuleRequiredField for CreditAccount.BankAccountNumber", DefaultContexts.Save)]
        public string BankAccountNumber
        {
            get
            {
                return _BankAccountNumber;
            }
            set
            {
                SetPropertyValue("AccountNumber", ref _BankAccountNumber, value);
            }
        }

        // TODO: create new account where the user maks is bank account

        public Account LinkedAccount
        {
            get
            {
                return _LinkedAccount;
            }
            set
            {
                SetPropertyValue("LinkedAccount", ref _LinkedAccount, value);
            }
        }

        public new class Fields
        {
            public static OperandProperty LinkedAccount
            {
                get { return new OperandProperty("LinkedAccount"); }
            }
        }
    }
}
