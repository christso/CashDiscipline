using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.Xpo;

namespace CashDiscipline.Module.ControllerHelpers
{
    public class BatchDeleter
    {
        private IObjectSpace objectSpace;

        public BatchDeleter(IObjectSpace objectSpace)
        {
            this.objectSpace = objectSpace;
        }

        public void Delete(BaseObject parent, string foreignKeyName)
        {
            var session = ((XPObjectSpace)objectSpace).Session;

            if (session.Connection == null)
            {
                // in memory connection
                parent.Delete();
            }
            else if (session.Connection != null)
            {
                // mssql connection
                DeleteObject(parent);
                DeleteChildren(parent, foreignKeyName);
            }
            objectSpace.CommitChanges();
        }

        public void Delete(IEnumerable parents, string foreignKeyName)
        {
            DeleteObjects(parents);

        }


        private void DeleteObject(BaseObject parent)
        {
            var session = ((XPObjectSpace)objectSpace).Session;

            var gcRecordIDGenerator = new Random();
            var randomNumber = gcRecordIDGenerator.Next(1, 2147483647);

            // get property names of parent objects
            List<string> cfParents = parent.ClassInfo.Members
                                .Where(m => m.IsPersistent && m.ReferenceType != null)
                                .Select(s => s.Name).ToList<string>();

            // get names of foreign keys to set to NULL
            string nullParentsSql = "";
            foreach (string mName in cfParents)
            {
                nullParentsSql += string.Format(", {0} = NULL", mName);
            }

            // update GCRecord and set foreign keys to NULL
            var cfTable = parent.ClassInfo.TableName;
            session.ExecuteNonQuery(string.Format(
                "UPDATE [{0}] SET GCRecord = {1}{4}"
                + " WHERE Oid IN ({2})",
                cfTable, // {0}
                randomNumber, // {1}
                "'" + parent.Oid + "'", //{2}
                nullParentsSql // {3}
                ));
        }

        private void DeleteObjects(IEnumerable objs)
        {
            var session = ((XPObjectSpace)objectSpace).Session;

            // get OID strings
            List<string> oidStrings = new List<string>();
            foreach (BaseObject obj in objs)
            {
                oidStrings.Add(string.Format("'{0}'", obj.Oid));
            }

            // get classinfo
            XPClassInfo classInfo = null;
            foreach (BaseObject obj in objs)
            {
                classInfo = obj.ClassInfo;
                if (classInfo == null)
                    throw new InvalidOperationException("XPClassInfo cannot be null");
                break;
            }

            // get property names of parent objects
            List<string> cfParents = classInfo.Members
                                .Where(m => m.IsPersistent && m.ReferenceType != null)
                                .Select(s => s.Name).ToList<string>();

            // get names of foreign keys to set to NULL
            List<string> nullParentSqls = new List<string>();
            foreach (string mName in cfParents)
            {
                nullParentSqls.Add(string.Format("{0} = NULL", mName));
            }

            var gcRecordIDGenerator = new Random();
            var randomNumber = gcRecordIDGenerator.Next(1, 2147483647);

            var sqlNonQuery = string.Format(
                "UPDATE [{0}] SET GCRecord = {1}{3} WHERE Oid IN ({2})",
                classInfo.TableName,
                randomNumber,
                string.Join(",", oidStrings),
                nullParentSqls.Count > 0 ? string.Join(",", nullParentSqls) : "");
            session.ExecuteNonQuery(sqlNonQuery);
        }

        public void Delete(XPClassInfo classInfo, CriteriaOperator criteria)
        {
            var session = ((XPObjectSpace)objectSpace).Session;

            var gcRecordIDGenerator = new Random();
            var randomNumber = gcRecordIDGenerator.Next(1, 2147483647);
            var sqlWhere = CriteriaToWhereClauseHelper.GetMsSqlWhere(XpoCriteriaFixer.Fix(criteria));
            sqlWhere = string.IsNullOrEmpty(sqlWhere) ? "" : " WHERE " + sqlWhere;
            var sqlNonQuery = "UPDATE " + classInfo.TableName + " SET GCRecord = "
                + randomNumber
                + sqlWhere;
            session.ExecuteNonQuery(sqlNonQuery);
        }
        private void DeleteChildren(BaseObject parent, string foreignKeyName)
        {
            var session = ((XPObjectSpace)objectSpace).Session;

            var gcRecordIDGenerator = new Random();
            var randomNumber = gcRecordIDGenerator.Next(1, 2147483647);

            // get members which are aggregated collections
            var children = parent.ClassInfo.Members.Where(m =>
                m.IsCollection && m.IsAggregated);

            foreach (var child in children)
            {
                // get property names of parent objects
                List<string> cfParents = child.CollectionElementType.Members
                                    .Where(m => m.IsPersistent && m.ReferenceType != null)
                                    .Select(s => s.Name).ToList<string>();

                // get names of foreign keys to set to NULL
                string nullParentsSql = "";
                foreach (string mName in cfParents)
                {
                    nullParentsSql += string.Format(", {0} = NULL", mName);
                }

                // update GCRecord and set foreign keys to NULL
                var cfTable = child.CollectionElementType.TableName;
                session.ExecuteNonQuery(string.Format(
                    "UPDATE [{0}] SET GCRecord = {1}{4}"
                    + " WHERE {3} IN ({2})",
                    cfTable, // {0}
                    randomNumber, // {1}
                    "'" + parent.Oid + "'", //{2}
                    foreignKeyName, // {3}
                    nullParentsSql
                    ));
            }
        }
    }
}
