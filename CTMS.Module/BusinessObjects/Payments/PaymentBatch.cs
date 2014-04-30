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
using System.Data;
using CTMS.Module.BusinessObjects.Cash;

// With XPO, the data model is declared by classes (so-called Persistent Objects) that will define the database structure, and consequently, the user interface (http://documentation.devexpress.com/#Xaf/CustomDocument2600).
namespace CTMS.Module.BusinessObjects.Payments
{
    // Specify various UI options for your persistent class and its properties using a declarative approach via built-in attributes (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
    //[ImageName("BO_Contact")]
    //[DefaultProperty("PersistentProperty")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewAndDetailView, true, NewItemRowPosition.Top)]
    [ImageName("BO_List")]
    [VisibleInReports(true)]
    public class PaymentBatch : BaseObject
    { // You can use a different base persistent class based on your requirements (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public PaymentBatch(Session session)
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
        private decimal? _PaymentTotal;
        private string _BatchName;
        private Account _DebitAccount;
        private DateTime _ValueDate;

        [Size(30)]
        public string BatchName
        {
            get
            {
                return _BatchName;
            }
            set
            {
                SetPropertyValue("BatchName", ref _BatchName, value);
            }
        }

        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime ValueDate
        {
            get
            {
                return _ValueDate;
            }
            set
            {
                SetPropertyValue("ValueDate", ref _ValueDate, value);
            }
        }

        [ModelDefault("LookupProperty", "BankAccountNumber")]
        public Account DebitAccount
        {
            get
            {
                return _DebitAccount;
            }
            set
            {
                SetPropertyValue("DebitAccount", ref _DebitAccount, value);
            }
        }

        [Association("PaymentBatch-Payments"), DevExpress.Xpo.Aggregated]
        public XPCollection<Payment> Payments
        {
            get
            {
                return GetCollection<Payment>("Payments");
            }
        }

        #region Payment Master
        [Persistent("PaymentTotal")]
        private decimal? fPaymentTotal = null;
        [VisibleInLookupListView(false)]
        [PersistentAlias("fPaymentTotal")]
        [ModelDefault("AllowEdit", "False")]
        public decimal? PaymentTotal
        {
            get
            {
                if (!IsLoading && !IsSaving && fPaymentTotal == null)
                    UpdatePaymentTotal(false);
                return fPaymentTotal;
            }
            set
            {
                SetPropertyValue("PaymentTotal", ref _PaymentTotal, value);
            }
        }

        protected override void OnLoaded()
        {
            //When using "lazy" calculations it's necessary to reset cached values.
            Reset();
            base.OnLoaded();
        }
        private void Reset()
        {
            fPaymentTotal = null;
            Payments.Reload();
        }

        //Define a way to calculate and update the OrdersTotal;
        public void UpdatePaymentTotal(bool forceChangeEvents)
        {
            //Put your complex business logic here. Just for demo purposes, we calculate a sum here.
            decimal? oldPaymentTotal = fPaymentTotal;
            decimal tempTotal = 0;
            //Manually iterate through the Orders collection if your calculated property requires a complex business logic which cannot be expressed via criteria language.
            foreach (Payment detail in Payments)
                tempTotal += detail.Amount;
            fPaymentTotal = tempTotal;
            if (forceChangeEvents)
                OnChanged("PaymentTotal", oldPaymentTotal, fPaymentTotal);
        }
        #endregion

    }

}
