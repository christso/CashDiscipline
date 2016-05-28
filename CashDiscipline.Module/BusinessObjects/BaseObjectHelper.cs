using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.BusinessObjects
{
    public class BaseObjectHelper
    {
        public static T GetInstance<T>(IObjectSpace objectSpace) where T : BaseObject
        {
            T result = objectSpace.FindObject<T>(null);
            if (result == null)
            {
                result = objectSpace.CreateObject<T>();
                result.Save();
            }
            return result;
        }
    }
}
