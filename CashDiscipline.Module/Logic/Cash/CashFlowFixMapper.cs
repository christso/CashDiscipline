using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.Controllers.Forex;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace CashDiscipline.Module.Logic.Cash
{
    public class CashFlowFixMapper
    {
        private readonly XPObjectSpace objSpace;
        private IList<CashFlowFixMapping> maps;

        public CashFlowFixMapper(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
 
        }

        public void Process(IEnumerable cashFlows)
        {
            RefreshMaps();

            var oidStrings = new List<string>();
            foreach (CashFlow cashFlow in cashFlows)
            {
                oidStrings.Add(string.Format("'{0}'", cashFlow.Oid));
            }

            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            var commandTextList = GetMapCommandTextListByCashFlow(cashFlows);

            foreach (string commandText in commandTextList)
            {
                command.CommandText = commandText;
                command.Parameters.Clear();
                command.ExecuteNonQuery();
            }
        }

        public void Process(List<SqlParameter> parameters)
        {
            RefreshMaps();

            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            var commandTextList = GetMapCommandTextList();

            foreach (string commandText in commandTextList)
            {
                command.CommandText = commandText;
                command.Parameters.Clear();
                command.Parameters.AddRange(parameters.ToArray());
                command.ExecuteNonQuery();
            }
        }

        public List<string> GetMapCommandTextListByCashFlow(IEnumerable cashFlows)
        {
            var oids = new List<string>();
            foreach (CashFlow cf in cashFlows)
            {
                oids.Add(string.Format("'{0}'", cf.Oid));
            }

            var steps = maps.GroupBy(m => new { m.MapStep })
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
                    mapTextList.Add(string.Format(@"UPDATE CashFlow SET
{0}
FROM CashFlow
LEFT JOIN CashFlowSource Source ON Source.Oid = CashFlow.Source
LEFT JOIN Activity ON Activity.Oid = CashFlow.Activity
WHERE CashFLow.GCRecord IS NULL
AND CashFlow.[Oid] IN ({1})",
                        setText, string.Join(",", oids)));
                }
            }
            return mapTextList;
        }

        public List<string> GetMapSetCommandTextList(int step)
        {
            var setTextList = new List<string>();

            var commandText = GetMapSetCommandText("FixActivity",
                m => string.Format("'{0}'", m.FixActivity.Oid),
                m => m.FixActivity != null, step);
            if (!string.IsNullOrWhiteSpace(commandText))
                setTextList.Add(commandText);

            commandText = GetMapSetCommandText("FixFromDate",
                m => string.Format("{0}", m.FixFromDateExpr),
                m => !string.IsNullOrEmpty(m.FixFromDateExpr), step);
            if (!string.IsNullOrWhiteSpace(commandText))
                setTextList.Add(commandText);

            commandText = GetMapSetCommandText("FixToDate",
                m => string.Format("{0}", m.FixToDateExpr),
                m => !string.IsNullOrEmpty(m.FixToDateExpr), step);
            if (!string.IsNullOrWhiteSpace(commandText))
                setTextList.Add(commandText);

            return setTextList;
        }

        // e.g. mapPropertyName = "FixActivity"; mapPropertyValue = "Handset Pchse"
        public List<string> GetMapCommandTextList()
        {
            var steps = maps.GroupBy(m => new { m.MapStep })
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
                    mapTextList.Add(string.Format(@"UPDATE CashFlow SET
{0}
FROM CashFlow
LEFT JOIN CashFlowSource Source ON Source.Oid = CashFlow.Source
LEFT JOIN Activity ON Activity.Oid = CashFlow.Activity
WHERE CashFLow.GCRecord IS NULL
AND CashFlow.[Snapshot] = @Snapshot
AND CashFlow.TranDate BETWEEN @FromDate AND @ToDate",
                        setText));
                }
            }
            return mapTextList;
        }

        public IList<CashFlowFixMapping> RefreshMaps()
        {
            this.maps = objSpace.GetObjects<CashFlowFixMapping>();
            return maps;
        }

        public string GetMapSetCommandText(string mapPropertyName,
            Func<CashFlowFixMapping, string> mapPropertyValue, 
            Predicate<CashFlowFixMapping> predicate, int step)
        {
            ValidateMapExists();
            if (mapPropertyValue == null)
                throw new ArgumentException("mapPropertyValue");
            if (predicate == null)
                throw new ArgumentException("predicate");

            string elseValue = string.Empty;
            var mapsCmdList = new List<string>();

            foreach (var map in maps.Where(m =>
                m.MapStep == step && predicate(m)))
            {
                if (map.CriteriaExpression.ToLower().Trim() == "else")
                {
                    elseValue = mapPropertyValue(map);
                }
                else
                {
                    mapsCmdList.Add(string.Format(
                        @"WHEN {0} THEN {1}",
                        map.CriteriaExpression, mapPropertyValue(map)));
                }
            }

            // add ELSE clause if value is specified in the mapping tables
            if (!string.IsNullOrEmpty(elseValue))
                mapsCmdList.Add("ELSE " + elseValue);

            // join list elements into single string
            string setText = string.Empty;

            if (mapsCmdList.Count > 0)
            {
                var mapsCmdText = string.Join("\n", mapsCmdList);
                mapsCmdText = string.Format("WHEN CashFlow.{0} IS NOT NULL THEN CashFlow.{0}", mapPropertyName)
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
