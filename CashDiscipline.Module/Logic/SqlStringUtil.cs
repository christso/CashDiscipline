using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFormat;
using DevExpress.Data.Filtering;
using Xafology.ExpressApp.Xpo;

namespace CashDiscipline.Module.Logic
{
    public class SqlStringUtil
    {
        public string CreateCommandText(List<SqlDeclareClause> clauses)
        {
            string result = string.Empty;
            foreach (var clause in clauses)
            {
                if (result != string.Empty)
                    result += "\n";
                result += string.Format("DECLARE @{0} {1} = {2}",
                    clause.ParameterName, clause.DataType, clause.CommandText);
            }
            return result;
        }

        public string AddSqlCriteria(string sqlUpdate, CriteriaOperator criteria)
        {
            var sqlWhere = CriteriaToWhereClauseHelper.GetMsSqlWhere(XpoCriteriaFixer.Fix(criteria));
            var sqlTemplate = sqlUpdate.Replace("{criteria}", (string.IsNullOrEmpty(sqlWhere) ? "" : " AND " + sqlWhere));
            return sqlTemplate;
        }
    }
}
