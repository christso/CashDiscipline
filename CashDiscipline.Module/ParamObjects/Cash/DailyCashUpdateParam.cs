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
