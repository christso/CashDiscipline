using System;
using DevExpress.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Model;

namespace CashDiscipline.Module.BusinessObjects
{
    [NavigationItem("Administration")]
    public class SystemMaintenance : BaseObject
    {
        public SystemMaintenance(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }

        private DateTime _LastRunDtTm;
        [ModelDefault("DisplayFormat", "dd-MMM-yy HH:mm:ss")]
        public DateTime LastRunDtTm
        {
            get
            {
                return _LastRunDtTm;
            }
            set
            {
                SetPropertyValue("LastRunDtTm", ref _LastRunDtTm, value);
            }
        }

        private string _CommandName;

        public string CommandName
        {
            get
            {
                return _CommandName;
            }
            set
            {
                SetPropertyValue("CommandName", ref _CommandName, value);
            }
        }
    }

}