/* Parameters in SQL
DECLARE @FromDate date = '2013-01-01'
DECLARE @ToDate date = '2016-12-31'
DECLARE @Algorithm int = 1
DECLARE @TargetObject_All int = 0
DECLARE @TargetObject_BankStmt int = 1
DECLARE @TargetObject_CashFlow int = 2
DECLARE @GenLedgerEntryType int = 0
DECLARE @Snapshot uniqueidentifier = (SELECT TOP 1 CurrentCashFlowSnapshot FROM SetOfBooks)
DECLARE @StmtSource uniqueidentifier = (SELECT TOP 1 BankStmtCashFlowSource FROM SetOfBooks)
*/

/* Instructions
Override ProcessCommandTextTemplate and FilterCommandTextTemplate,
ensuring that you include {FA} and {JG} tokens in the text template.
*/

using CashDiscipline.Module.BusinessObjects.FinAccounting;
using CashDiscipline.Module.ParamObjects.FinAccounting;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using SmartFormat;

namespace CashDiscipline.Module.Logic.FinAccounting
{
    public class ActivitySqlJournalHelper
    {
        protected readonly XPObjectSpace objSpace;
        protected readonly FinGenJournalParam paramObj;
        protected DateTime fromDate;
        protected DateTime toDate;

        #region SQL

        protected virtual string ProcessCommandTextTemplate
        {
            get
            {
                return string.Empty;

            }
        }

        protected virtual string FilterCommandTextTemplate
        {
            get
            {
                return string.Empty;
            }
        }

        #endregion

        public ActivitySqlJournalHelper(XPObjectSpace objSpace, FinGenJournalParam paramObj)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;

            if (paramObj.FromDate <= SqlDateTime.MinValue.Value)
                this.fromDate = SqlDateTime.MinValue.Value;
            else
                this.fromDate = paramObj.FromDate;

            if (paramObj.ToDate <= SqlDateTime.MinValue.Value)
                this.toDate = SqlDateTime.MaxValue.Value;
            else
                this.toDate = paramObj.ToDate;
        }

        public void Process(IEnumerable<FinActivity> activityMaps)
        {
            var sourceObjCount = GetSourceObjectCount();
            if (sourceObjCount == 0) return;

            var sqlUtil = new SqlProcessorUtil(objSpace.Session);
            var clauses = CreateSqlParameters();
            var parameters = sqlUtil.CreateParameters(clauses);

            var commandText = CreateProcessCommandText(activityMaps);

            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            command.Parameters.AddRange(parameters.ToArray());
            command.CommandText = commandText;
            try
            {
                int result = command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ex.Message + "\r\nLine Number " + ex.LineNumber
                    + ". \r\n## SQL BEGIN ##\r\n" + command.CommandText + "\r\n## SQL END ##", ex);
            }
        }

        private int GetSourceObjectCount()
        {
            var sqlUtil = new SqlProcessorUtil(objSpace.Session);
            var clauses = CreateSqlParameters();
            var parameters = sqlUtil.CreateParameters(clauses);

            var commandText = "SELECT COUNT(*) "
                + FilterCommandTextTemplate.Replace("{JG}", JournalGroupsParamText);
            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            command.Parameters.AddRange(parameters.ToArray());
            command.CommandText = commandText;

            return Convert.ToInt32(command.ExecuteScalar());
        }

        public List<SqlDeclareClause> CreateSqlParameters()
        {
            var res = string.Format("{0}", Convert.ToInt32(FinMapAlgorithmType.SQL));
            var clauses = new List<SqlDeclareClause>()
            {
                new SqlDeclareClause("FromDate", "date", string.Format("'{0}'", fromDate.ToString("yyyy-MM-dd"))),
                new SqlDeclareClause("ToDate", "date", string.Format("'{0}'", toDate.ToString("yyyy-MM-dd"))),
                new SqlDeclareClause("TargetObject_All", "int",  string.Format("{0}", Convert.ToInt32(FinJournalTargetObject.All))),
                new SqlDeclareClause("TargetObject_BankStmt", "int",  string.Format("{0}", Convert.ToInt32(FinJournalTargetObject.BankStmt))),
                new SqlDeclareClause("TargetObject_CashFlow", "int", string.Format("{0}", Convert.ToInt32(FinJournalTargetObject.CashFlow))),
                new SqlDeclareClause("Algorithm", "int",  string.Format("{0}", Convert.ToInt32(FinMapAlgorithmType.SQL))),
                new SqlDeclareClause("Snapshot", "uniqueidentifier", "(SELECT TOP 1 CurrentCashFlowSnapshot FROM SetOfBooks WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("StmtSource", "uniqueidentifier", "(SELECT TOP 1 BankStmtCashFlowSource FROM SetOfBooks WHERE GCRecord IS NULL)")
            };
            return clauses;
        }
        public string CreateFunctionalCcyAmtCommandText(IEnumerable<FinActivity> activityMaps)
        {
            var activitiesToMap = activityMaps.GroupBy(m => new { m.FromActivity, m.JournalGroup } )
                .Select(g => new { Group = g, Count = g.Count() })
                .SelectMany(groupWithCount => groupWithCount.Group.Select(b => b)
                    .Zip(
                        Enumerable.Range( 1, groupWithCount.Count ),
                        ( j, i ) => new {
                            j.Oid, j.FromActivity, j.JournalGroup, j.FunctionalCcyAmtExpr, RowNumber = i
                        }
                    )
                );

            var caseList = new List<string>();
            foreach (var map in activitiesToMap)
            {
                string sql = string.Format("WHEN FromActivity = '{0}' AND FinActivity.JournalGroup = '{3}' AND CalcRow = {2} THEN {1}",
                    map.FromActivity.Oid.ToString().ToUpper(),
                    map.FunctionalCcyAmtExpr.Replace("{FA}", "CAST ( FunctionalCcyAmt*-1 AS money )"),
                    map.RowNumber,
                    map.JournalGroup.Oid.ToString().ToUpper());

                caseList.Add(sql);
            }

            return "CASE " + string.Join("\n", caseList) + " END";
        }

        private string JournalGroupsParamText
        {
            get
            {
                var journalGroupOids = paramObj.JournalGroupParams
    .Select(x => string.Format("'{0}'", x.JournalGroup.Oid).ToUpper());

                var journalGroupsParamText = string.Join(",", journalGroupOids);
                return journalGroupsParamText;
            }
        }

        public string CreateProcessCommandText(IEnumerable<FinActivity> activityMaps)
        {
            var amtCommandText = CreateFunctionalCcyAmtCommandText(activityMaps);
            var commandText = ProcessCommandTextTemplate
                .Replace("{filter}", FilterCommandTextTemplate)
                .Replace("{FA}", amtCommandText)
                .Replace("{JG}", JournalGroupsParamText);
            return commandText;
        }
    }
}
