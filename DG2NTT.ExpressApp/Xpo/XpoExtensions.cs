﻿using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.HelperClasses.Xpo
{
    public class XpoExtensions
    {
        public static System.Collections.Generic.IList<T> GetObjects<T>(Session session, CriteriaOperator criteria, SortingCollection sorting)
        {
            return session.GetObjects(session.GetClassInfo(typeof(T)),
                            criteria,
                            sorting, 0, false, true).Cast<T>().ToList();
        }

        public static System.Collections.Generic.IList<T> GetObjects<T>(Session session, CriteriaOperator criteria)
        {
            return session.GetObjects(session.GetClassInfo(typeof(T)),
                            criteria,
                            new SortingCollection(null), 0, false, true).Cast<T>().ToList();
        }
    }
}
