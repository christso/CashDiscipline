using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.BusinessObjects
{
    [NavigationItem("Administration")]
    [ModelDefault("ImageName", "BO_List")]
    public class ReportConfig : BaseObject
    {
        public ReportConfig(Session session)
            : base(session)
        {
        }

        private DateTime _CurrentCashFlowStartDate;
        [ModelDefault("DisplayFormat", "dd-MMM-yy")]
        public DateTime CurrentCashFlowStartDate
        {
            get
            {
                return _CurrentCashFlowStartDate;
            }
            set
            {
                SetPropertyValue("CurrentCashFlowStartDate", ref _CurrentCashFlowStartDate, value);
            }
        }
    }
}
