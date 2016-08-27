using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
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


        public static BaseObject PullCachedInstance(DevExpress.Xpo.Session session, BaseObject CachedInstance,
            Type objType)
        {
            // return previous instance if session matches
            try
            {
 
                if (CachedInstance != null
                    && ((BaseObject)CachedInstance).Session == session
                    && !CachedInstance.IsDeleted)
                {
                    return CachedInstance;
                }
            }
            catch (ObjectDisposedException)
            {
            }

            // return instance from new session
            var instance = session.FindObject(
                PersistentCriteriaEvaluationBehavior.InTransaction, objType, null) as BaseObject;
            if (instance == null)
            {
                // create SetOfBooks if it doesn't exist
                instance = Activator.CreateInstance(objType, session) as BaseObject;
                instance.Save();
            }
            return instance;
        }
    }
}
