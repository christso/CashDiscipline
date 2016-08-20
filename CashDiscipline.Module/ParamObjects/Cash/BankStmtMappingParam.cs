using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.ParamObjects.Cash
{
    public class BankStmtMappingParam : BaseObject
    {
        public BankStmtMappingParam(Session session) : base(session) 
        {
            session.LockingOption = LockingOption.None;
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

        private bool _ActivityFlag;
        public bool ActivityFlag
        {
            get
            {
                return _ActivityFlag;
            }
            set
            {
                SetPropertyValue("ActivityFlag", ref _ActivityFlag, value);
            }
        }

        private bool _CounterpartyFlag;
        public bool CounterpartyFlag
        {
            get
            {
                return _CounterpartyFlag;
            }
            set
            {
                SetPropertyValue("CounterpartyFlag", ref _CounterpartyFlag, value);
            }
        }

        private bool _OraTrxNumFlag;
        public bool OraTrxNumFlag
        {
            get
            {
                return _OraTrxNumFlag;
            }
            set
            {
                SetPropertyValue("OraTrxNumFlag", ref _OraTrxNumFlag, value);
            }
        }
    }
}
