using System;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp.Model;

namespace CashDiscipline.Module.ParamObjects.Cash
{
    public class DailyCashUpdateParam : BaseObject
    {
        public DailyCashUpdateParam(Session session) : base(session) 
        {
            session.LockingOption = LockingOption.None;
        }

        public static DailyCashUpdateParam GetInstance(IObjectSpace objectSpace)
        {
            DailyCashUpdateParam result = objectSpace.FindObject<DailyCashUpdateParam>(null);
            if (result == null)
            {
                result = new DailyCashUpdateParam(((XPObjectSpace)objectSpace).Session);
                result.Save();
            }
            return result;
        }


        private DateTime _FromDate;
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
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
        [ModelDefault("EditMask", "dd-MMM-yy")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
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

        private Activity _ApReclassActivity;
        public Activity ApReclassActivity
        {
            get
            {
                return _ApReclassActivity;
            }
            set
            {
                SetPropertyValue("ApReclassActivity", ref _ApReclassActivity, value);
            }
        }

        private CashFlowSource _ApReclassSource;
        public CashFlowSource ApReclassSource
        {
            get
            {
                return _ApReclassSource;
            }
            set
            {
                SetPropertyValue("ApReclassSource", ref _ApReclassSource, value);
            }
        }


        private Counterparty _Counterparty;
        public Counterparty Counterparty
        {
            get
            {
                return _Counterparty;
            }
            set
            {
                SetPropertyValue("Counterparty", ref _Counterparty, value);
            }
        }
    }
}
