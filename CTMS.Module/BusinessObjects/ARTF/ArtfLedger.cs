using System;
using System.ComponentModel;

using DevExpress.Xpo;
using DevExpress.Data.Filtering;

using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;

using GenerateUserFriendlyId.Module.BusinessObjects;
using DevExpress.ExpressApp.Security.Strategy;
using CTMS.Module.HelperClasses;
using DevExpress.ExpressApp.Model;

namespace CTMS.Module.BusinessObjects.Artf
{
    public class ArtfLedger : UserFriendlyIdPersistentObject, IReconMaster
    {
        public ArtfLedger(Session session)
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
            LedgerDate = DateTime.Now;
            System = Session.FindObject<ArtfSystem>(CriteriaOperator.Parse("Name = 'Payment Central'"));
            GlCode = Session.FindObject<ArtfGlCode>(CriteriaOperator.Parse("Name = 'ANZ Operating'"));

            SecuritySystemUser currentUser = StaticHelpers.GetCurrentUser(Session);
            if (currentUser != null)
                this.CreatedBy = currentUser;
        }
        protected override void OnDeleting()
        {
            UnlinkRecons("ArtfToRecons");
            UnlinkRecons("ArtfFromRecons");

            base.OnDeleting();
        }
        private void UnlinkRecons(string name)
        {
            XPCollection<ArtfRecon> recons = GetCollection<ArtfRecon>(name);
            while (recons.Count > 0)
            {
                recons[0].FromLedger = null;
            }
        }

        [PersistentAlias("concat('L', ToStr(SequentialNumber))")]
        public string LedgerId
        {
            get
            {
                return Convert.ToString(EvaluateAlias("LedgerId"));
            }
        }
        DateTime _LedgerDate;
        public DateTime LedgerDate
        {
            get { return _LedgerDate; }
            set { SetPropertyValue("LedgerDate", ref _LedgerDate, value); }
        }
        decimal _Amount;
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal Amount
        {
            get { return _Amount; }
            set { SetPropertyValue("Amount", ref _Amount, value); }
        }
        string _Description;
        [Size(SizeAttribute.Unlimited)]
        public string Description
        {
            get { return _Description; }
            set { SetPropertyValue("Description", ref _Description, value); }
        }
        ArtfGlCode _GlCode;
        [Association(@"ArtfGlCode-ArtfLedgers")]
        public ArtfGlCode GlCode
        {
            get { return _GlCode; }
            set { SetPropertyValue("GlCode", ref _GlCode, value); }
        }
        ArtfSystem _System;
        [Association(@"ArtfSystem-ArtfLedgers")]
        public ArtfSystem System
        {
            get { return _System; }
            set { SetPropertyValue("System", ref _System, value); }
        }
        [Association(@"ArtfFromLedger-ArtfRecons")]
        public XPCollection<ArtfRecon> ArtfFromRecons 
        {
            get { return GetCollection<ArtfRecon>("ArtfFromRecons"); } 
        }
        [Association(@"ArtfToLedger-ArtfRecons")]
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
            _ToReconTotal = null;
            ArtfFromRecons.Reload();
            ArtfToRecons.Reload();
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

        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal? FromUnreconTotal
        {
            get
            {
                return Amount - FromReconTotal;
            }
        }

        //Define a way to calculate and update the OrdersTotal;
        public void UpdateFromReconTotal(bool forceChangeEvents)
        {
            //Put your complex business logic here. Just for demo purposes, we calculate a sum here.
            decimal? oldReconTotal = _FromReconTotal;
            decimal tempTotal = 0;
            //Manually iterate through the Orders collection if your calculated property requires a complex business logic which cannot be expressed via criteria language.
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
        [ModelDefault("EditMask", "n2")]
        [ModelDefault("DisplayFormat", "n2")]
        public decimal? ToUnreconTotal
        {
            get
            {
                return Amount - ToReconTotal;
            }
        }

        //Define a way to calculate and update the OrdersTotal;
        public void UpdateToReconTotal(bool forceChangeEvents)
        {
            //Put your complex business logic here. Just for demo purposes, we calculate a sum here.
            decimal? oldReconTotal = _ToReconTotal;
            decimal tempTotal = 0;
            //Manually iterate through the Orders collection if your calculated property requires a complex business logic which cannot be expressed via criteria language.
            foreach (ArtfRecon detail in ArtfToRecons)
                tempTotal += detail.Amount;
            _ToReconTotal = tempTotal;
            if (forceChangeEvents)
                OnChanged("ToReconTotal", oldReconTotal, _ToReconTotal);
        }

        #endregion
        #endregion

        #region Methods


        #endregion

    }

}
