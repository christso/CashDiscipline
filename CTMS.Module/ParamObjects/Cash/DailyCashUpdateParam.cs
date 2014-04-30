using System;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Reports;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using CTMS.Module.BusinessObjects.Cash;

namespace CTMS.Module.ParamObjects.Cash
{
    [NonPersistent]
    public class DailyCashUpdateParam
    {
        private DateTime _TranDate;
        public DateTime TranDate
        {
            get
            {
                return _TranDate;
            }
            set
            {
                _TranDate = value;
            }
        }
    }
}
