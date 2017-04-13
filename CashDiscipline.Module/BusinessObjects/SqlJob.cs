using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Model;
using System.ComponentModel;
using DevExpress.ExpressApp;
using CashDiscipline.Module.Attributes;

namespace CashDiscipline.Module.BusinessObjects
{
    [NavigationItem("Administration")]
    [ModelDefault("ImageName", "BO_List")]
    [ModelDefault("IsFooterVisible", "True")]
    [AutoColumnWidth(false)]
    public class SqlJob : BaseObject
    {
        public SqlJob()
        {

        }
        public SqlJob(Session session)
            : base(session)
        {

        }

        private string _JobName;
        public string JobName
        {
            get
            {
                return _JobName;
            }
            set
            {
                SetPropertyValue("JobName", ref _JobName, value);
            }
        }

        private string _LogMessage;
        [Size(SizeAttribute.Unlimited)]
        public string LogMessage
        {
            get
            {
                return _LogMessage;
            }
            set
            {
                SetPropertyValue("LogMessage", ref _LogMessage, value);
            }
        }

        private DateTime _LogDateTime;
        [ModelDefault("DisplayFormat", "dd-MMM-yy HH:mm:ss")]
        public DateTime LogDateTime
        {
            get
            {
                return _LogDateTime;
            }
            set
            {
                SetPropertyValue("LogDateTime", ref _LogDateTime, value);
            }
        }

    }
}
