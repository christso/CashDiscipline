using CashDiscipline.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashDiscipline.Module.BusinessObjects.Cash;

namespace CashDiscipline.Module.ParamObjects.Cash
{
    public class CashFlowFixParam : BaseObject
    {
        public CashFlowFixParam(Session session) : base(session) 
        {
            session.LockingOption = LockingOption.None;
        }

        public CashFlowFixParam() { }
             
        // TODO: create generic method
        public static CashFlowFixParam GetInstance(IObjectSpace objectSpace)
        {
            CashFlowFixParam result = objectSpace.FindObject<CashFlowFixParam>(null);
            if (result == null) {
                result = new CashFlowFixParam(((XPObjectSpace)objectSpace).Session);
                result.Save();
            }
            return result;
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        // Fields...
        private Activity _ApReclassActivity;
        private DateTime _PayrollNextLockdownDate;
        private DateTime _PayrollLockdownDate;
        private DateTime _ApayableNextLockdownDate;
        private DateTime _ApayableLockdownDate;
        private DateTime _ToDate;
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

        [DisplayName("AP Lockdown Date")]
        public DateTime ApayableLockdownDate
        {
            get
            {
                return _ApayableLockdownDate;
            }
            set
            {
                SetPropertyValue("ApayableLockdownDate", ref _ApayableLockdownDate, value);
            }
        }


        public DateTime ApayableNextLockdownDate
        {
            get
            {
                return _ApayableNextLockdownDate;
            }
            set
            {
                SetPropertyValue("ApayableNextLockdownDate", ref _ApayableNextLockdownDate, value);
            }
        }


        public DateTime PayrollLockdownDate
        {
            get
            {
                return _PayrollLockdownDate;
            }
            set
            {
                SetPropertyValue("PayrollLockdownDate", ref _PayrollLockdownDate, value);
            }
        }


        public DateTime PayrollNextLockdownDate
        {
            get
            {
                return _PayrollNextLockdownDate;
            }
            set
            {
                SetPropertyValue("PayrollNextLockdownDate", ref _PayrollNextLockdownDate, value);
            }
        }

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
    }
}
