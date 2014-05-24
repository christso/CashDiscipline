using System;
using System.ComponentModel;

using DevExpress.Xpo;
using DevExpress.Data.Filtering;

using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using CTMS.Module.BusinessObjects;
using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp.Security.Strategy;
using CTMS.Module.HelperClasses;
using DevExpress.ExpressApp.Model;

namespace CTMS.Module.BusinessObjects.Artf
{
    [DefaultProperty("Oid")]
    public class ArtfReceipt : BaseObject, IReconMaster
    {
        public ArtfReceipt(Session session)
            : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here or place it only when the IsLoading property is false:
            // if (!IsLoading){
            //    It is now OK to place your initialization code here.
            // }
            // or as an alternative, move your initialization code into the AfterConstruction method.
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
            ReceiptDate = DateTime.Now;

            SecuritySystemUser currentUser = D2NXAF.ExpressApp.StaticHelpers.GetCurrentUser(Session);
            if (currentUser != null)
                this.CreatedBy = currentUser;
        }

        //TODO: do not include underscore if only 1 receipt number entered
        [PersistentAlias("MultiConcat('_', ReceiptBatchNumber, ReceiptNumber)")]
        [VisibleInDetailView(false)]
        public string ReceiptAlias
        {
            get
            {
                return Convert.ToString(EvaluateAlias("ReceiptAlias"));
            }
        }

        DateTime _ReceiptDate;
        public DateTime ReceiptDate
        {
            get { return _ReceiptDate; }
            set { SetPropertyValue<DateTime>("ReceiptDate", ref _ReceiptDate, value); }
        }
        string _ReceiptBatchNumber;
        [RuleRequiredField("RuleRequiredField for ArtfReceipt.ReceiptBatchNumber", DefaultContexts.Save,
            SkipNullOrEmptyValues=false, TargetCriteria = FieldName.ReceiptNumber + " = null")]
        public string ReceiptBatchNumber
        {
            get { return _ReceiptBatchNumber; }
            set { SetPropertyValue<string>("ReceiptBatchNumber", ref _ReceiptBatchNumber, value); }
        }
        string _ReceiptNumber;
        [RuleRequiredField("RuleRequiredField for ArtfReceipt.ReceiptNumber", DefaultContexts.Save,
            SkipNullOrEmptyValues = false, TargetCriteria = FieldName.ReceiptBatchNumber + " = null")]
        public string ReceiptNumber
        {
            get { return _ReceiptNumber; }
            set { SetPropertyValue<string>("ReceiptNumber", ref _ReceiptNumber, value); }
        }
        decimal _Amount;
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal Amount
        {
            get { return _Amount; }
            set { SetPropertyValue<decimal>("Amount", ref _Amount, value); }
        }
        ArtfSystem _System;
        [Association(@"ArtfSystem-ArtfReceipts")]
        public ArtfSystem System
        {
            get { return _System; }
            set { SetPropertyValue("ArtfSystem", ref _System, value); }
        }
        

        [Association(@"ArtfFromReceipt-ArtfRecons")]
        public XPCollection<ArtfRecon> ArtfFromRecons 
        { 
            get { return GetCollection<ArtfRecon>("ArtfFromRecons"); } 
        }
        [Association(@"ArtfToReceipt-ArtfRecons")]
        public XPCollection<ArtfRecon> ArtfToRecons
        {
            get { return GetCollection<ArtfRecon>("ArtfToRecons"); }
        }

        SecuritySystemUser _CreatedBy;
        [VisibleInListView(false)]
        [ModelDefault("AllowEdit", "False")]
        public SecuritySystemUser CreatedBy
        {
            get
            {
                return _CreatedBy;
            }
            set
            {
                SetPropertyValue("CreatedBy", ref _CreatedBy, value);
            }
        }
        private Account _RemittanceAccount;
        [ToolTip("Bank account that receives the payment")]
        public Account RemittanceAccount
        {
            get
            {
                return _RemittanceAccount;
            }
            set
            {
                SetPropertyValue("RemittanceAccount", ref _RemittanceAccount, value);
            }
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


        #region Recon Master

        protected override void OnLoaded()
        {
            //When using "lazy" calculations it's necessary to reset cached values.
            Reset();
            base.OnLoaded();
        }
        private void Reset()
        {
            _FromReconTotal = null;
            ArtfFromRecons.Reload();

            _ToReconTotal = null;
            ArtfToRecons.Reload();
        }
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal? ToUnreconTotal
        {
            get
            {
                return Amount - ToReconTotal ?? 0.00M;
            }
        }
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal? FromUnreconTotal
        {
            get
            {
                return Amount - FromReconTotal ?? 0.00M;
            }
        }
        #region From Recon
        [Persistent("FromReconTotal")]
        private decimal? _FromReconTotal = null;
        [PersistentAlias("_FromReconTotal")]
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal? FromReconTotal
        {
            get
            {
                if (!IsLoading && !IsSaving && _FromReconTotal == null)
                    UpdateFromReconTotal(false);
                return _FromReconTotal;
            }
        }

        //Define a way to calculate and update the Total;
        public void UpdateFromReconTotal(bool forceChangeEvents)
        {
            //Put your complex business logic here.
            decimal? oldReconTotal = _FromReconTotal;
            decimal tempTotal = 0;
            //Manually iterate through the collection if your calculated property requires a complex business logic which cannot be expressed via criteria language.
            foreach (ArtfRecon detail in ArtfFromRecons)
                tempTotal += detail.Amount;
            _FromReconTotal = tempTotal;
            if (forceChangeEvents)
                OnChanged("FromReconTotal", oldReconTotal, _FromReconTotal);
        }
        #endregion
        #region To Recon

        [Persistent("ToReconTotal")]
        private decimal? _ToReconTotal = null;
        [PersistentAlias("_ToReconTotal")]
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal? ToReconTotal
        {
            get
            {
                if (!IsLoading && !IsSaving && _ToReconTotal == null)
                    UpdateToReconTotal(false);
                return _ToReconTotal;
            }
        }

        //Define a way to calculate and update the Total;
        public void UpdateToReconTotal(bool forceChangeEvents)
        {
            //Put your complex business logic here.
            decimal? oldReconTotal = _ToReconTotal;
            decimal tempTotal = 0;
            //Manually iterate through the collection if your calculated property requires a complex business logic which cannot be expressed via criteria language.
            foreach (ArtfRecon detail in ArtfToRecons)
                tempTotal += detail.Amount;
            _ToReconTotal = tempTotal;
            if (forceChangeEvents)
                OnChanged("ToReconTotal", oldReconTotal, _ToReconTotal);
        }
        #endregion

        #endregion

        public class FieldName
        {
            public const string ReceiptBatchNumber = "ReceiptBatchNumber";
            public const string ReceiptNumber = "ReceiptNumber";
        }
    }

}
