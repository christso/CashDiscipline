using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.BusinessObjects.Setup
{
    [DefaultClassOptions]
    [ModelDefault("ImageName", "BO_List")]
    public class ActionPortal : BaseObject
    {
        public ActionPortal()
        {

        }
        public ActionPortal(Session session)
            : base(session)
        {

        }

        private string _ActionName;
        public string ActionName
        {
            get
            {
                return _ActionName;
            }
            set
            {
                SetPropertyValue("ActionName", ref _ActionName, value);
            }
        }
        
        private string _ActionDescription;
        public string ActionDescription
        {
            get
            {
                return _ActionDescription;
            }
            set
            {
                SetPropertyValue("ActionDescription", ref _ActionDescription, value);
            }
        }
    }
}
