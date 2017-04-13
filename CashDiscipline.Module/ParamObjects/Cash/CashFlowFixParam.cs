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
using DevExpress.ExpressApp.Model;

namespace CashDiscipline.Module.ParamObjects.Cash
{
    [ModelDefault("ImageName", "BO_List")]
    public class CashFlowFixParam : BaseObject
    {
        public CashFlowFixParam(Session session) : base(session) 
        {
            session.LockingOption = LockingOption.None;
        }

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
            this.DlrComDayOfWeek = 3;
        }

        private DateTime _FromDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
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
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
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

        private DateTime _ApayableLockdownDate;
        [DisplayName("AP Lockdown Date")]
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
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

        private DateTime _ApayableNextLockdownDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
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

        private DateTime _PayrollLockdownDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
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

        private DateTime _PayrollNextLockdownDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        [ModelDefault("EditMask", "dd-MMM-yy")]
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

        private CashFlowSnapshot _Snapshot;
        public CashFlowSnapshot Snapshot
        {
            get
            {
                return _Snapshot;
            }
            set
            {
                SetPropertyValue("Snapshot", ref _Snapshot, value);
            }
        }

        private int _DlrComDayOfWeek;
        public int DlrComDayOfWeek
        {
            get
            {
                return _DlrComDayOfWeek;
            }
            set
            {
                SetPropertyValue("DlrComDayOfWeek", ref _DlrComDayOfWeek, value);
            }
        }



    }
}
