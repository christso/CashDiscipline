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
using CTMS.Module.BusinessObjects.Payments;

// With XPO, the data model is declared by classes (so-called Persistent Objects) that will define the database structure, and consequently, the user interface (http://documentation.devexpress.com/#Xaf/CustomDocument2600).
namespace CTMS.Module.BusinessObjects.Payments
{
    // Specify various UI options for your persistent class and its properties using a declarative approach via built-in attributes (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
    //[ImageName("BO_Contact")]
    //[DefaultProperty("PersistentProperty")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewAndDetailView, true, NewItemRowPosition.Top)]
    [VisibleInReports(true)]
    public class Payment : BaseObject
    { // You can use a different base persistent class based on your requirements (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public Payment(Session session)
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

        // Fields...
        private CreditAccount _CreditAccount;
        private PaymentBatch _PaymentBatch;
        private decimal _Amount;

        // Payment Total
        [Association("PaymentBatch-Payments")]
        [RuleRequiredField("Payment.PaymentBatch_RuleRequiredField", DefaultContexts.Save)]
        public PaymentBatch PaymentBatch
        {
            get
            {
                return _PaymentBatch;
            }
            set
            {
                PaymentBatch oldPaymentBatch = _PaymentBatch;
                SetPropertyValue("PaymentBatch", ref _PaymentBatch, value);
                if (!IsLoading && !IsSaving && oldPaymentBatch != _PaymentBatch)
                {
                    oldPaymentBatch = oldPaymentBatch ?? _PaymentBatch;
                    oldPaymentBatch.UpdatePaymentTotal(true);
                }
            }
        }

        [ModelDefault("LookupProperty", "BankAccountNumber")]
        public CreditAccount CreditAccount
        {
            get
            {
                return _CreditAccount;
            }
            set
            {
                SetPropertyValue("CreditAccount", ref _CreditAccount, value);
            }
        }

        [RuleRequiredField("RuleRequiredField for Payment.Amount", DefaultContexts.Save)]
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal Amount
        {
            get
            {
                return _Amount;
            }
            set
            {
                SetPropertyValue("Amount", ref _Amount, value);
            }
        }

        public new class Fields
        {
            public static OperandProperty CreditAccount
            {
                get { return new OperandProperty("CreditAccount"); }
            }
        }
    }
}
