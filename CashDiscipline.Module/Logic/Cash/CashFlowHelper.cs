using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Cash
{
    public class CashFlowHelper
    {
        public static CashFlowSnapshot GetCurrentSnapshot(Session session)
        {
            var setOfBooks = SetOfBooks.GetInstance(session);
            return session.GetObjectByKey<CashFlowSnapshot>(setOfBooks.CurrentCashFlowSnapshot.Oid);
        }

        public static DateTime GetMaxActualTranDate(Session session)
        {
            var currentSnapshot = SetOfBooks.CachedInstance.CurrentCashFlowSnapshot;

            DateTime? res = (DateTime?)session.Evaluate<CashFlow>(CriteriaOperator.Parse("Max(TranDate)"),
                    CriteriaOperator.Parse("Status = ? And Snapshot = ?", 
                    CashFlowStatus.Actual, currentSnapshot));
            if (res == null)
                return default(DateTime);
            return (DateTime)res;
        }

        #region Date Helpers

        // this returns the date at the start of the week
        // e.g. the Thursday that occurs before sourceDate
        public static DateTime StartDateOfWeek(DateTime sourceDate, int dayOfWeek, int firstDayOfWeek = 2)
        {
            var ldw = EndDateOfWeek(sourceDate, dayOfWeek, firstDayOfWeek);
            if (XlWeekDay(sourceDate) >= XlWeekDay(ldw))
                return ldw;
            else
                return ldw.AddDays(7);
        }

        // this returns the date at the end of the current week
        // e.g. the Friday in the current week if the sourceDate betwen Monday and Friday
        public static DateTime EndDateOfWeek(DateTime sourceDate, int dayOfWeek, int firstDayOfWeek = 2)
        {
            return sourceDate
                .AddDays(dayOfWeek)
                .AddDays(-1 * XlWeekDay(sourceDate, firstDayOfWeek));
        }

        // this returns the date at the end of next week
        // e.g. the Thursday that occurs after sourceDate
        public static DateTime NextDateOfWeek(DateTime sourceDate, int dayOfWeek, int firstDayOfWeek = 2)
        {
            var ldw = EndDateOfWeek(sourceDate, dayOfWeek, firstDayOfWeek);
            if (XlWeekDay(sourceDate) <= XlWeekDay(ldw))
                return ldw;
            else
                return ldw.AddDays(7);
        }

        // This returns the day of the week in integer type
        // it behaves the same as the Excel WEEKDAY function.
        public static int XlWeekDay(DateTime dateTime, int firstDayOfWeek = 2)
        {
            DayOfWeek wd = dateTime.DayOfWeek;
            int result = 0;
            switch (wd)
            {
                case DayOfWeek.Monday:
                    result = 1 + 2 - firstDayOfWeek;
                    break;
                case DayOfWeek.Tuesday:
                    result = 2 + 2 - firstDayOfWeek;
                    break;
                case DayOfWeek.Wednesday:
                    result = 3 + 2 - firstDayOfWeek;
                    break;
                case DayOfWeek.Thursday:
                    result = 4 + 2 - firstDayOfWeek;
                    break;
                case DayOfWeek.Friday:
                    result = 5 + 2 - firstDayOfWeek;
                    break;
                case DayOfWeek.Saturday:
                    result = 6 + 2 - firstDayOfWeek;
                    break;
                case DayOfWeek.Sunday:
                    result = 7 + 2 - firstDayOfWeek;
                    break;
            }
            return result;
        }

        #endregion
    }
}
