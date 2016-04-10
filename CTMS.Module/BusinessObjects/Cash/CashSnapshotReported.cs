using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;

namespace CTMS.Module.BusinessObjects.Cash
{
    [ImageName("BO_List")]
    [ModelDefault("IsCloneable", "True")]
    [NavigationItem("Cash Setup")]
    [DefaultListViewOptions(allowEdit: true, newItemRowPosition: NewItemRowPosition.Top)]
    [ModelDefault("IsFooterVisible", "True")]
    public class CashSnapshotReported : BaseObject
    {
        public CashSnapshotReported(Session session)
            : base(session)
        {
        }

        private string _CompareName;
        [RuleUniqueValue("CashFlowSnapshot_CompareName_RuleUniqueValue", DefaultContexts.Save)]
        public string CompareName
        {
            get
            {
                return _CompareName;
            }
            set
            {
                SetPropertyValue("CompareName", ref _CompareName, value);
            }
        }

        private CashFlowSnapshot _CurrentSnapshot;

        public CashFlowSnapshot CurrentSnapshot
        {
            get
            {
                return _CurrentSnapshot;
            }
            set
            {
                SetPropertyValue("Snapshot", ref _CurrentSnapshot, value);
            }
        }

        private CashFlowSnapshot _PreviousSnapshot;

        public CashFlowSnapshot PreviousSnapshot
        {
            get
            {
                return _PreviousSnapshot;
            }
            set
            {
                SetPropertyValue("Snapshot", ref _PreviousSnapshot, value);
            }
        }

        private DateTime _FromDate;
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

        private DateTime _ToDate;
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

    }

}
