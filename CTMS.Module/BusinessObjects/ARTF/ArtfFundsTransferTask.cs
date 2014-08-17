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
using CTMS.Module.BusinessObjects.Payments;

// With XPO, the data model is declared by classes (so-called Persistent Objects) that will define the database structure, and consequently, the user interface (http://documentation.devexpress.com/#Xaf/CustomDocument2600).
namespace CTMS.Module.BusinessObjects.Artf
{
    // Specify various UI options for your persistent class and its properties using a declarative approach via built-in attributes (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
    //[ImageName("BO_Contact")]
    //[DefaultProperty("PersistentProperty")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewAndDetailView, true, NewItemRowPosition.Top)]
    [VisibleInReports]
    public class ArtfFundsTransferTask : ArtfTask
    { // You can use a different base persistent class based on your requirements (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public ArtfFundsTransferTask(Session session)
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

        private Activity _TfrFromActivity;
        private Account _TfrToBankAccount;
        private Account _TfrFromBankAccount;

        // TODO: this does not work. It does not return XP collection.
        [DataSourceProperty("ArtfRecon.BankStmt.Account.ArtfCustomerType.Cash_Accounts")]
        public Account TfrFromBankAccount
        {
            get
            {
                return _TfrFromBankAccount;
            }
            set
            {
                SetPropertyValue("TfrFromBankAccount", ref _TfrFromBankAccount, value);
            }
        }

        [DataSourceProperty("ArtfRecon.CustomerType.Cash_Accounts")]
        public Account TfrToBankAccount
        {
            get
            {
                return _TfrToBankAccount;
            }
            set
            {
                SetPropertyValue("TfrToBankAccount", ref _TfrToBankAccount, value);
            }
        }


        public Activity TfrFromActivity
        {
            get
            {
                return _TfrFromActivity;
            }
            set
            {
                SetPropertyValue("TfrFromActivity", ref _TfrFromActivity, value);
            }
        }

        public override void AssignDefaultTaskOwner()
        {
            AssignDefaultTaskOwner(TaskTypeEnum.Transfer);
        }

        [Association("ArtfFundsTransferTask-ArtfTaskPayments")]
        public XPCollection<ArtfTaskPayment> ArtfTaskPayments
        {
            get
            {
                return GetCollection<ArtfTaskPayment>("ArtfTaskPayments");
            }
        }

        public new class Fields
        {
            public static OperandProperty Oid
            {
                get { return new OperandProperty("Oid"); }
            }
            public static OperandProperty TfrFromBankAccount
            {
                get { return new OperandProperty("TfrFromBankAccount"); }
            }
            public static OperandProperty TfrToBankAccount
            {
                get { return new OperandProperty("TfrToBankAccount"); }
            }
        }
    }
}
