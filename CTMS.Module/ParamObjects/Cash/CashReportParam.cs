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
using DevExpress.ExpressApp.Security.Strategy;
using CTMS.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp.Xpo;

using CTMS.Module.BusinessObjects;

namespace CTMS.Module.ParamObjects.Cash
{
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (http://documentation.devexpress.com/#Xaf/CustomDocument2701).
    public class CashReportParam : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public CashReportParam(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
            ReportUser = Xafology.ExpressApp.StaticHelpers.GetCurrentUser(Session);
            Snapshot1 = Session.GetObjectByKey<CashFlowSnapshot>(SetOfBooks.CachedInstance.CurrentCashFlowSnapshot.Oid);
        }
        // Fields...
        private SecuritySystemUser _ReportUser;
        private DateTime _ToDate;
        private DateTime _FromDate;
        private CashFlowSnapshot _Snapshot1;
        private CashFlowSnapshot _Snapshot2;

        public DateTime FromDate
        {
            get
            {
                return _FromDate;
            }
            set
            {
                SetPropertyValue("FromDate", ref _FromDate, value);
            }
        }


        public DateTime ToDate
        {
            get
            {
                return _ToDate;
            }
            set
            {
                SetPropertyValue("ToDate", ref _ToDate, value);
            }
        }

        public CashFlowSnapshot Snapshot1
        {
            get
            {
                return _Snapshot1;
            }
            set
            {
                SetPropertyValue("Snapshot1", ref _Snapshot1, value);
            }
        }

        public CashFlowSnapshot Snapshot2
        {
            get
            {
                return _Snapshot2;
            }
            set
            {
                SetPropertyValue("Snapshot2", ref _Snapshot2, value);
            }
        }

        [MemberDesignTimeVisibility(false), NonPersistent]
        public bool IsVarianceMode
        {
            get
            {
                return Snapshot1 != null && Snapshot2 != null;
            }
        }

        [ModelDefault("AllowEdit", "False")]
        public SecuritySystemUser ReportUser
        {
            get
            {
                return _ReportUser;
            }
            set
            {
                SetPropertyValue("ReportUser", ref _ReportUser, value);
            }
        }

        
        public static CashReportParam GetInstance(XPObjectSpace objSpace)
        {
            var currentUser = Xafology.ExpressApp.StaticHelpers.GetCurrentUser(objSpace.Session);
            CashReportParam result = objSpace.FindObject<CashReportParam>(
                CriteriaOperator.Parse("ReportUser = ?", currentUser));
            if (result == null)
            {
                result = new CashReportParam(objSpace.Session);
                result.ReportUser = currentUser;
                result.Save();
            }
            return result;
        }

        /// <summary>
        /// set default parameters if not value defined for the parameteres
        /// </summary>
        /// <returns>True if parameter values were blank and substited with default values,
        /// otherwise False.</returns>
        public bool SetDefaultParams()
        {
            var reportParam = this;
            var session = reportParam.Session;
            var result = false;
            // create default parameters if no parameters exist
            var cashFlowCount = Convert.ToInt32(session.Evaluate<CashFlow>(
                CriteriaOperator.Parse("Count()"), null));
            if (cashFlowCount == 0) return false;
            
            if (reportParam.FromDate == default(DateTime))
            {
                reportParam.FromDate = (DateTime)session.Evaluate<CashFlow>(
                    CriteriaOperator.Parse("Min(TranDate)"), null);
                result = true;
            }
            if (reportParam.ToDate == default(DateTime))
            {
                reportParam.ToDate = (DateTime)session.Evaluate<CashFlow>(
                    CriteriaOperator.Parse("Max(TranDate)"), null);
                result = true;
            }
            return result;
        }

    }

}
