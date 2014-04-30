using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security.Strategy;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CTMS.Module.HelperClasses
{
    public class StaticHelpers
    {

        /// <summary>
        /// Get singleton instance. This will instatiate the object if no object exists.
        /// </summary>
        public static T GetInstance<T>(IObjectSpace objectSpace)
        {
            T result = objectSpace.FindObject<T>(null);
            if (result == null)
            {
                result = (T)Activator.CreateInstance(typeof(T), (((XPObjectSpace)objectSpace).Session));
                ((IXPObject)result).Session.Save(result);
            }
            return result;
        }

        public static SecuritySystemUser GetCurrentUser(Session session)
        {
            if (SecuritySystem.CurrentUser != null)
            {
                SecuritySystemUser currentUser = session.GetObjectByKey<SecuritySystemUser>(
                    session.GetKeyValue(SecuritySystem.CurrentUser));
                return currentUser;
            }
            return null;
        }

        public static SecuritySystemUser GetCurrentUser(IObjectSpace objSpace)
        {
            return GetCurrentUser(((XPObjectSpace)objSpace).Session);
        }


        public static DateTime DateObject(int Year, int Month, int Day)
        {
            //adjust Month that is greater than 12, e.g. if month = 13,
            //then reduce month by 12 and add 1 to Year
            var yearsInMonth = (int)(Month / 12);
            if (Month > 12)
            {
                Year += yearsInMonth;
                Month -= yearsInMonth * 12;
            }
            //get end of previous month month
            //by getting 1st day of current month less 1 day
            //TODO: if day or month <= 0 then adjust
            if (Day == 0)
                return new DateTime(Year, Month, 1).AddDays(-1);
            return new DateTime(Year, Month, Day);
        }

    }
}
