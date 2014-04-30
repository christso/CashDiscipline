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
using CTMS.Module.ParamObjects.Import;

namespace CTMS.Module.BusinessObjects.User
{
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (http://documentation.devexpress.com/#Xaf/CustomDocument2701).
    [FileAttachmentAttribute("LayoutFile")]
    public class UserViewLayout : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (http://documentation.devexpress.com/#Xaf/CustomDocument3146).
        public UserViewLayout(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (http://documentation.devexpress.com/#Xaf/CustomDocument2834).
        }

        // Fields...
        private FileData _LayoutFile;
        private string _LayoutName;
        private UserViewLayoutType _LayoutType;
        private SecuritySystemUser _CreatedBy;

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

        public UserViewLayoutType LayoutType
        {
            get
            {
                return _LayoutType;
            }
            set
            {
                SetPropertyValue("LayoutType", ref _LayoutType, value);
            }
        }

        [RuleUniqueValue("UserViewLayout_RuleUniqueValue", DefaultContexts.Save)]
        public string LayoutName
        {
            get
            {
                return _LayoutName;
            }
            set
            {
                SetPropertyValue("LayoutName", ref _LayoutName, value);
            }
        }

        public FileData LayoutFile
        {
            get
            {
                return _LayoutFile;
            }
            set
            {
                SetPropertyValue("LayoutFile", ref _LayoutFile, value);
            }
        }

        public static bool IsCashFlowPivotLayout(string name)
        {
            return name == Constants.CashFlowPivotLayoutDaily
                || name == Constants.CashFlowPivotLayoutDaily
                || name == Constants.CashFlowPivotLayoutWeekly
                || name == Constants.CashFlowPivotLayoutMonthly
                || name == Constants.CashFlowPivotLayoutMonthlyVariance
                || name == Constants.CashFlowPivotLayoutFixForecast;
        }

    }

    public enum UserViewLayoutType
    {
        [DevExpress.ExpressApp.DC.XafDisplayName("Cash Flow Report Pivot Grid")]
        CashFlowReportPivotGrid
    }
}
