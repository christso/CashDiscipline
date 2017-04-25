using CashDiscipline.Module.BusinessObjects.AccountsPayable;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Logic.SqlMap;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.Xpo;

namespace CashDiscipline.Module.Logic.Cash
{
    public class ApInvoiceBalanceMapper
    {
        private readonly XPObjectSpace objSpace;
        private Mapper<ApInvoiceBalanceMapping> mapper;

        private const string MapCommandTextListByObjectSqlTemplate =
            MapCommandTextListSqlTemplateCommon + @"
AND ApInvoiceBalance.[Oid] IN ({oids})";

        private const string MapCommandTextListSqlTemplateCommon = @"UPDATE ApInvoiceBalance SET
{setpairs}
FROM ApInvoiceBalance
LEFT JOIN Activity AccountActivity ON AccountActivity.Oid = ApInvoiceBalance.AccountActivity
LEFT JOIN Activity CostCentreActivity ON CostCentreActivity.Oid = ApInvoiceBalance.CostCentreActivity
WHERE ApInvoiceBalance.GCRecord IS NULL
{criteria}";

        private const string BeforeSql = @"
DECLARE @UndefActivityOid uniqueidentifier = (SELECT TOP 1 Oid FROM Activity WHERE Name LIKE 'AP Pymt' AND GCRecord IS NULL);

IF OBJECT_ID('tempdb..#TmpApInvoiceBalance') IS NOT NULL DROP TABLE #TmpApInvoiceBalance;
SELECT Oid INTO #TmpApInvoiceBalance
FROM ApInvoiceBalance
WHERE 1=1 {criteria}

/* --- Map --- */

UPDATE b1 SET 
AccountActivity = COALESCE (
(
SELECT m1.Activity
FROM GlAccountActivityMap m1
WHERE m1.Code = b1.ExpenseAccount
), @UndefActivityOid)
FROM ApInvoiceBalance b1
WHERE EXISTS (SELECT * FROM #TmpApInvoiceBalance b2 WHERE b2.Oid = b1.Oid)

UPDATE b1 SET 
CostCentreActivity = COALESCE (
(
SELECT m1.Activity
FROM GlCostCentreActivityMap m1
WHERE m1.Code = b1.ExpenseCostCentre
), @UndefActivityOid)
FROM ApInvoiceBalance b1
WHERE EXISTS (SELECT * FROM #TmpApInvoiceBalance b2 WHERE b2.Oid = b1.Oid)

/* --- LEGACY CODE BELOW --- */

/* Vendors */
INSERT INTO VHAFinance.dbo.Vendors
SELECT DISTINCT T1.[Supplier] FROM ApInvoiceBalance T1
WHERE EXISTS (SELECT * FROM #TmpApInvoiceBalance b2 WHERE b2.Oid = T1.Oid)
AND T1.[Supplier] NOT IN (SELECT VHAFinance.dbo.Vendors.Vendor FROM VHAFinance.dbo.Vendors)

/* Invoices */
INSERT INTO VHAFinance.dbo.ApInvoices
SELECT DISTINCT T1.InvoiceNumber, T1.InvoiceDate
FROM ApInvoiceBalance T1
WHERE T1.InvoiceNumber NOT IN (SELECT VHAFinance.dbo.ApInvoices.InvoiceNum FROM VHAFinance.dbo.ApInvoices)
AND T1.InvoiceNumber IS NOT NULL
AND EXISTS (SELECT * FROM #TmpApInvoiceBalance b2 WHERE b2.Oid = T1.Oid);

/* Invoice Received Date */
UPDATE ApInvoiceBalance SET 
InvoiceReceivedDate =
(
	SELECT MAX(T1.[Invoice Received Date])
	FROM VHAFinance.dbo.ApInvoiceHeader T1
	WHERE T1.[Vendor Name] = b1.Supplier
		AND T1.[Invoice Num] = b1.InvoiceNumber
)
FROM ApInvoiceBalance b1
WHERE EXISTS (SELECT * FROM #TmpApInvoiceBalance b2 WHERE b2.Oid = b1.Oid);";

        public ApInvoiceBalanceMapper(XPObjectSpace objspace)
        {
            this.objSpace = objspace;
            this.mapper = new Mapper<ApInvoiceBalanceMapping>(objspace, "ApInvoiceBalance", GetMapSetCommandTextList);
            mapper.MapCommandTextListByObjectSqlTemplate = MapCommandTextListByObjectSqlTemplate;
            mapper.MapCommandTextListSqlTemplate = string.Empty;
        }

        public List<SqlDeclareClause> CreateSqlParameters()
        {
            var clauses = new List<SqlDeclareClause>()
            {
                new SqlDeclareClause("UndefActivityOid", "uniqueidentifer",
                "(select oid from activity where activity.name like 'UNDEFINED' and GCRecord IS NULL)"),
            };
            return clauses;
        }

        public List<SqlParameter> CreateParameters(List<SqlDeclareClause> clauses)
        {
            var parameters = new List<SqlParameter>();
            using (var cmd = objSpace.Session.Connection.CreateCommand())
            {
                foreach (var clause in clauses)
                {
                    parameters.Add(new SqlParameter(clause.ParameterName, clause.ExecuteScalar(cmd)));
                }
            }
            return parameters;
        }

        public IList<ApInvoiceBalanceMapping> RefreshMaps()
        {
            return mapper.RefreshMaps();
        }

        public void Process(IEnumerable objs)
        {
            BeforeProcess(objs);

            var clauses = CreateSqlParameters();
            var sqlParams = CreateParameters(clauses);
            mapper.SqlParameters = sqlParams;

            mapper.Process(objs);
        }

        public void Process(ImportApInvoiceBalanceParam paramObj)
        {
            var criteria = CriteriaOperator.Parse("AsAtDate Between(?,?)", paramObj.FromDate, paramObj.ToDate);
            Process(criteria);
        }

        public void Process(CriteriaOperator criteria)
        {
            var sqlTemplate = mapper.ConvertToSql(MapCommandTextListSqlTemplateCommon, criteria);

            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            command.CommandText = mapper.ConvertToSql(BeforeSql, criteria);

            BeforeProcess(criteria);

            var clauses = CreateSqlParameters();
            var sqlParams = CreateParameters(clauses);
            mapper.SqlParameters = sqlParams;


            mapper.Process(sqlTemplate);
        }

        private void BeforeProcess(IEnumerable objs)
        {
            var oids = new List<string>();
            foreach (BaseObject obj in objs)
            {
                oids.Add(string.Format("'{0}'", obj.Oid));
            }

            var util = new SqlStringUtil();

            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            command.CommandText = BeforeSql.Replace(SqlStringUtil.CriteriaToken, "AND Oid IN (" + string.Join(",", oids) + ")"); // replace {criteria} with filter
            command.ExecuteNonQuery();
        }


        private void BeforeProcess(CriteriaOperator criteria)
        {
            var util = new SqlStringUtil();

            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            command.CommandText = util.AddSqlCriteria(BeforeSql, criteria); 
            command.ExecuteNonQuery();
        }

        public List<string> GetMapSetCommandTextList(int step)
        {
            var setTextList = new List<string>();

            var commandText = mapper.GetMapSetCommandText("Activity",
                m => {
                    if (m.Activity != null)
                        return string.Format("'{0}'", m.Activity.Oid);
                    else
                        return string.Format("(SELECT TOP 1 c.Oid FROM Activity c WHERE c.Name LIKE {0} AND c.GCRecord IS NULL)", m.ActivityExpr);
                },
                m => m.Activity != null || !string.IsNullOrWhiteSpace(m.ActivityExpr),
                step,
                "WHEN ApInvoiceBalance.Activity IS NOT NULL AND ApInvoiceBalance.Activity <> @UndefActivityOid THEN ApInvoiceBalance.Activity");
            if (!string.IsNullOrWhiteSpace(commandText))
                setTextList.Add(commandText);
            
            return setTextList;
        }

    }
}
