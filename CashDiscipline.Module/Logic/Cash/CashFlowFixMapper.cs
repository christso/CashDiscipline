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
using SmartFormat;

namespace CashDiscipline.Module.Logic.Cash
{
    public class CashFlowFixMapper
    {
        private readonly XPObjectSpace objSpace;
        private IList<CashFlowFixMapping> maps;

        #region SQL Templates

        private const string MapCommandTextListSqlTemplate =
            MapCommandTextListSqlTemplateCommon + @"
AND CashFlow.TranDate BETWEEN @FromDate AND @ToDate
AND CashFlow.[Snapshot] = @Snapshot
AND (
    CashFlow.Fix IS NULL OR Fix.Name LIKE 'Auto' 
    OR CashFlow.FixActivity IS NULL
    OR CashFlow.ForexSettleType IS NULL
    OR CashFlow.ForexSettleType = @AutoForexSettleType
)";

        private const string MapCommandTextListByCashFlowSqlTemplate =
            MapCommandTextListSqlTemplateCommon + @"
AND CashFlow.[Oid] IN ({1})";

        private const string MapCommandTextListSqlTemplateCommon = @"UPDATE CashFlow SET
{0}
FROM CashFlow
LEFT JOIN CashFlowSource Source ON Source.Oid = CashFlow.Source
LEFT JOIN Activity ON Activity.Oid = CashFlow.Activity
LEFT JOIN Account ON Account.Oid = CashFlow.Account
LEFT JOIN CashForecastFixTag Fix ON Fix.Oid = CashFlow.Fix
WHERE CashFLow.GCRecord IS NULL";

        #endregion

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
            var commandTextList = GetMapCommandTextListByItem(cashFlows);

            foreach (string commandText in commandTextList)
            {
                command.CommandText = FixCashFlowsAlgorithm.ParameterCommandText + "\n" + commandText;
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
                command.CommandText = FixCashFlowsAlgorithm.ParameterCommandText + "\n" + commandText;
                command.Parameters.Clear();
                //command.Parameters.AddRange(parameters.ToArray());
                command.ExecuteNonQuery();
            }
        }

        public List<string> GetMapCommandTextListByItem(IEnumerable cashFlows)
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
                    mapTextList.Add(string.Format(MapCommandTextListByCashFlowSqlTemplate,
                        setText, string.Join(",", oids)));
                }
            }
            return mapTextList;
        }

        public List<string> GetMapSetCommandTextList(int step)
        {
            var setTextList = new List<string>();

            var commandText = GetMapSetCommandText("FixActivity", m => string.Format("'{0}'", m.FixActivity.Oid), m => m.FixActivity != null, step,
                "WHEN CashFlow.FixActivity IS NOT NULL AND CashFlow.Fix != @AutoFixTag THEN CashFlow.FixActivity"
                + "\nWHEN CashFlow.FixActivity IS NULL AND CashFlow.Fix != @AutoFixTag THEN CashFlow.Activity");
            if (!string.IsNullOrWhiteSpace(commandText))
                setTextList.Add(commandText);

            commandText = GetMapSetCommandText("Fix", m => string.Format("'{0}'", m.Fix.Oid), m => m.Fix != null, step,
                "WHEN CashFlow.Fix IS NOT NULL AND CashFlow.Fix != @AutoFixTag THEN CashFlow.Fix");
            if (!string.IsNullOrWhiteSpace(commandText))
                setTextList.Add(commandText);

            commandText = GetMapSetCommandText("FixRank", m => string.Format("'{0}'", m.FixRank),
                m => m.FixRank != 0, step,
                "WHEN CashFlow.{0} IS NOT NULL AND CashFlow.{0} != 0 THEN CashFlow.{0}");
            if (!string.IsNullOrWhiteSpace(commandText))
                setTextList.Add(commandText);

            commandText = GetMapSetCommandText("FixFromDate", m => string.Format("{0}", m.FixFromDateExpr), m => !string.IsNullOrEmpty(m.FixFromDateExpr), step,
                "WHEN CashFlow.FixFromDate IS NOT NULL AND CashFlow.Fix != @AutoFixTag THEN CashFlow.FixFromDate");
            if (!string.IsNullOrWhiteSpace(commandText))
                setTextList.Add(commandText);

            commandText = GetMapSetCommandText("FixToDate", m => string.Format("{0}", m.FixToDateExpr), m => !string.IsNullOrEmpty(m.FixToDateExpr), step,
                "WHEN CashFlow.FixToDate IS NOT NULL AND CashFlow.Fix != @AutoFixTag THEN CashFlow.FixToDate");
            if (!string.IsNullOrWhiteSpace(commandText))
                setTextList.Add(commandText);

            // skip settle type = EXCLUDE and AUTO
            commandText = GetMapSetCommandText("ForexSettleType", m => string.Format("{0}", Convert.ToInt32(m.ForexSettleType)),
                m => m.ForexSettleType != CashFlowForexSettleType.Auto, step,
                "WHEN CashFlow.ForexSettleType IS NOT NULL AND CashFlow.ForexSettleType != @AutoForexSettleType THEN CashFlow.ForexSettleType");
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
                    mapTextList.Add(string.Format(MapCommandTextListSqlTemplate,
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

        // defaultValueSql: The first condition in the CASE statement.
        public string GetMapSetCommandText(string mapPropertyName,
            Func<CashFlowFixMapping, string> mapPropertyValue, 
            Predicate<CashFlowFixMapping> predicate, int step, 
            string defaultValueSql)
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
                if (string.IsNullOrEmpty(defaultValueSql))
                    defaultValueSql = "WHEN CashFlow.{0} IS NOT NULL THEN CashFlow.{0}";

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
