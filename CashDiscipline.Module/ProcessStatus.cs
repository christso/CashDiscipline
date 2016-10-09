using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module
{
    public class ProcessStatus : BaseObject
    {
        public ProcessStatus(Session session) : base(session) 
        {
            session.LockingOption = LockingOption.None;
        }

        public static ProcessStatus GetInstance(IObjectSpace objectSpace)
        {
            ProcessStatus result = objectSpace.FindObject<ProcessStatus>(null);
            if (result == null)
            {
                result = new ProcessStatus(((XPObjectSpace)objectSpace).Session);
                result.Save();
            }
            return result;
        }

        private bool _IsUnfixRequired;
        public bool IsUnfixRequired
        {
            get
            {
                return _IsUnfixRequired;
            }
            set
            {
                SetPropertyValue("IsUnfixRequired", ref _IsUnfixRequired, value);
            }
        }
    }
}
