using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.ParamObjects.Forex
{
    public class ForexSettleFifoParam : BaseObject
    {
        public ForexSettleFifoParam(Session session) : base(session) 
        {
            session.LockingOption = LockingOption.None;
        }

        public static ForexSettleFifoParam GetInstance(IObjectSpace objectSpace)
        {
            ForexSettleFifoParam result = objectSpace.FindObject<ForexSettleFifoParam>(null);
            if (result == null)
            {
                result = new ForexSettleFifoParam(((XPObjectSpace)objectSpace).Session);
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
