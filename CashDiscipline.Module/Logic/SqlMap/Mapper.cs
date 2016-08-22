using CashDiscipline.Module.Interfaces;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.Xpo;
using SmartFormat;
using DevExpress.Xpo;

namespace CashDiscipline.Module.Logic.SqlMap
{
    public class Mapper<TMap>
    {
        private string objectTable;
        private IList<TMap> maps;
        private readonly XPObjectSpace objSpace;
        
        public Mapper(XPObjectSpace objSpace, 
            string objectTable,
             Func<int, List<string>> getMapSetCommandTextList)
        {
            if (string.IsNullOrEmpty(objectTable))
                throw new ArgumentNullException("objectTable");

            this.objectTable = objectTable;
            this.objSpace = objSpace;
            this.GetMapSetCommandTextList = getMapSetCommandTextList;
        }
        public IList<TMap> RefreshMaps()
        {
            this.maps = objSpace.GetObjects<TMap>();
            return maps;
        }

        // Mapping using SQL parameters
        public string MapCommandTextListSqlTemplate
        {
            get; set;
        }

        // Mapping using object OIDs
        public string MapCommandTextListByObjectSqlTemplate
        {
            get; set;
        }

        public Func<int, List<string>> GetMapSetCommandTextList { get; set; }

        public List<SqlParameter> SqlParameters { get; set; }

        public void Process(string sqlUpdate, CriteriaOperator criteria)
        {
            var sqlWhere = CriteriaToWhereClauseHelper.GetMsSqlWhere(XpoCriteriaFixer.Fix(criteria));
            var sqlTemplate = sqlUpdate + " AND " + sqlWhere;

            RefreshMaps();

            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            var commandTextList = GetMapCommandTextList(sqlTemplate);

            foreach (string commandText in commandTextList)
            {
                command.CommandText = commandText;
                command.Parameters.Clear();
                ProcessCommand(command);
            }
        }

        public void Process(IEnumerable objs)
        {
            RefreshMaps();

            var oidStrings = new List<string>();
            foreach (BaseObject obj in objs)
            {
                oidStrings.Add(string.Format("'{0}'", obj.Oid));
            }

            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            var commandTextList = GetMapCommandTextListByItem(objs);

            foreach (string commandText in commandTextList)
            {
                command.CommandText = commandText;
                command.Parameters.Clear();
                ProcessCommand(command);
            }
        }

        public void Process(IXPObject obj)
        {
            var objs = new object[1];
            objs[0] = obj;
            Process(objs);
        }

        private void ProcessCommand(SqlCommand command)
        {
            if (SqlParameters != null)
                command.Parameters.AddRange(SqlParameters.ToArray());
            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ex.Message + "\r\nLine Number " + ex.LineNumber
                    + ". \r\nSQL ---------\r\n" + command.CommandText, ex);
            }
        }

        // used for mapping the object view
        // e.g. mapPropertyName = "FixActivity"; mapPropertyValue = "Handset Pchse"
        public List<string> GetMapCommandTextList()
        {
            return GetMapCommandTextList(MapCommandTextListSqlTemplate);
        }

        private List<string> GetMapCommandTextList(string sqlTemplate)
        {
            var steps = maps.GroupBy(m => new { ((IMapping)m).MapStep })
                   .OrderBy(g => g.Key.MapStep)
                   .Select(g => g.Key.MapStep)
                   .ToList<int>();

            var mapTextList = new List<string>();

            foreach (var step in steps)
            {
                var setTextList = GetMapSetCommandTextList(step);

                string setText = string.Join(",\n", setTextList);

                if (setTextList.Count > 0)
                {
                    mapTextList.Add(string.Format(sqlTemplate,
                        setText));
                }
            }
            return mapTextList;
        }

        // used for mapping individual objects
        public List<string> GetMapCommandTextListByItem(IEnumerable objs)
        {
            var oids = new List<string>();
            foreach (BaseObject obj in objs)
            {
                oids.Add(string.Format("'{0}'", obj.Oid));
            }

            var steps = maps.GroupBy(m => new { ((IMapping)m).MapStep })
               .OrderBy(g => g.Key.MapStep)
               .Select(g => g.Key.MapStep)
               .ToList<int>();

            var mapTextList = new List<string>();

            foreach (var step in steps)
            {
                var setTextList = GetMapSetCommandTextList(step);

                string setText = string.Join(",\n", setTextList);

                if (setTextList.Count > 0)
                {
                    mapTextList.Add(string.Format(MapCommandTextListByObjectSqlTemplate,
                        setText, string.Join(",", oids)));
                }
            }
            return mapTextList;
        }

        public string GetMapSetCommandText(string mapPropertyName,
            Func<TMap, string> mapPropertyValue,
            Predicate<TMap> predicate, int step,
            string defaultValueSql)
        {
            ValidateMapExists();
            if (mapPropertyValue == null)
                throw new ArgumentException("mapPropertyValue");
            if (predicate == null)
                throw new ArgumentException("predicate");

            string elseValue = string.Empty;
            var mapsCmdList = new List<string>();

            foreach (IMapping map in maps.Where(m =>
                ((IMapping)m).MapStep == step && predicate(m)))
            {
                if (map.CriteriaExpression.ToLower().Trim() == "else")
                {
                    elseValue = mapPropertyValue((TMap)map);
                }
                else
                {
                    mapsCmdList.Add(string.Format(
                        @"WHEN {0} THEN {1}",
                        map.CriteriaExpression, mapPropertyValue((TMap)map)));
                }
            }

            // add ELSE clause if value is specified in the mapping tables
            if (!string.IsNullOrEmpty(elseValue))
                mapsCmdList.Add("ELSE " + elseValue);

            // join list elements into single string
            string setText = string.Empty;

            if (mapsCmdList.Count > 0)
            {
                if (string.IsNullOrEmpty(defaultValueSql))
                {
                    defaultValueSql = "WHEN {objectTable}.{0} IS NOT NULL THEN {objectTable}.{0}"
                        .Replace("{objectTable}", this.objectTable);
                }
                var mapsCmdText = string.Join("\n", mapsCmdList);
                mapsCmdText = string.Format(defaultValueSql, mapPropertyName)
                    + "\n" + mapsCmdText;

                setText = string.Format(@"{1} = CASE
{0}
END",
mapsCmdText,
mapPropertyName);
            }
            return setText;
        }

        private void ValidateMapExists()
        {
            if (this.maps == null)
                throw new InvalidOperationException("Maps cannot be null");
        }
    }
}
