using CashDiscipline.Module.BusinessObjects.AccountsPayable;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.Logic.SqlMap;
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
    public class ApPmtDistnMapper
    {
        private readonly XPObjectSpace objSpace;
        private Mapper<ApPmtDistnMapping> mapper;

        private const string MapCommandTextListByObjectSqlTemplate =
            MapCommandTextListSqlTemplateCommon + @"
AND ApPmtDistn.[Oid] IN ({oids})";

        private const string MapCommandTextListSqlTemplateCommon = @"UPDATE ApPmtDistn SET
{setpairs}
FROM 
(
	SELECT *,
	(
		SELECT SUM(a2.PaymentAmountAud)
		FROM ApPmtDistn a2
		WHERE a2.PaymentDate = ApPmtDistn.PaymentDate
			AND a2.Vendor = ApPmtDistn.Vendor
	) AS VendorPaymentAmountAud
	FROM ApPmtDistn
) ApPmtDistn
LEFT JOIN ApSource Source ON Source.Oid = ApPmtDistn.Source
LEFT JOIN ApBankAccount BankAccount ON BankAccount.Oid =  ApPmtDistn.BankAccount
LEFT JOIN ApPayGroup PayGroup ON PayGroup.Oid =  ApPmtDistn.PayGroup
LEFT JOIN ApInvSource InvSource ON InvSource.Oid = ApPmtDistn.InvSource
LEFT JOIN ApVendor Vendor ON Vendor.Oid = ApPmtDistn.Vendor
LEFT JOIN Currency InvoiceCurrency ON InvoiceCurrency.Oid = ApPmtDistn.InvoiceCurrency
LEFT JOIN Currency PaymentCurrency ON PaymentCurrency.Oid = ApPmtDistn.PaymentCurrency
LEFT JOIN Activity ON Activity.Oid = ApPmtDistn.Activity
LEFT JOIN Account ON Account.Oid = ApPmtDistn.Account
WHERE ApPmtDistn.GCRecord IS NULL";

        private const string AddCounterpartySql = @"DECLARE @DefaultFixCounterparty uniqueidentifier = (SELECT Oid FROM Counterparty WHERE Name LIKE 'UNDEFINED')
DECLARE @DefaultCounterpartyTag nvarchar(255) = 'UNDEFINED'

INSERT INTO Counterparty
(
	Oid,
	Name,
	FixCounterparty,
	CounterpartyL1,
	CounterpartyL2,
	DateTimeCreated
)
SELECT
(SELECT CAST(CAST(NEWID() AS BINARY(10)) + CAST(GETDATE() AS BINARY(6)) AS UNIQUEIDENTIFIER)) AS Oid,
v.Name,
@DefaultFixCounterparty AS FixCounterparty,
@DefaultCounterpartyTag AS CounterpartyL1,
@DefaultCounterpartyTag AS CounterpartyL2,
GETDATE()
FROM ApVendor v
WHERE NOT EXISTS (SELECT c.Name FROM Counterparty c WHERE c.Name = v.Name)";

        public ApPmtDistnMapper(XPObjectSpace objspace)
        {
            this.objSpace = objspace;
            this.mapper = new Mapper<ApPmtDistnMapping>(objspace, "ApPmtDistn", GetMapSetCommandTextList);
            mapper.MapCommandTextListByObjectSqlTemplate = MapCommandTextListByObjectSqlTemplate;
            mapper.MapCommandTextListSqlTemplate = string.Empty;
        }

        public List<SqlDeclareClause> CreateSqlParameters()
        {
            var clauses = new List<SqlDeclareClause>()
            {
                new SqlDeclareClause("UndefActivityOid", "uniqueidentifer",
                "(select oid from activity where activity.name like 'UNDEFINED' and GCRecord IS NULL)"),
                new SqlDeclareClause("UndefCounterpartyOid", "uniqueidentifer",
                "(select oid from counterparty where counterparty.name like 'UNDEFINED' and GCRecord IS NULL)")
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

        public IList<ApPmtDistnMapping> RefreshMaps()
        {
            return mapper.RefreshMaps();
        }

        public void Process(IEnumerable objs)
        {
            BeforeProcess();

            var clauses = CreateSqlParameters();
            var sqlParams = CreateParameters(clauses);
            mapper.SqlParameters = sqlParams;

            mapper.Process(objs);
        }

        public void Process(IXPObject obj)
        {
            BeforeProcess();

            var clauses = CreateSqlParameters();
            var sqlParams = CreateParameters(clauses);
            mapper.SqlParameters = sqlParams;
            mapper.Process(obj);
        }

        public void Process(CriteriaOperator criteria)
        {
            BeforeProcess();

            var clauses = CreateSqlParameters();
            var sqlParams = CreateParameters(clauses);
            mapper.SqlParameters = sqlParams;
            mapper.Process(MapCommandTextListSqlTemplateCommon, criteria);
        }

        private void BeforeProcess()
        {
            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            command.CommandText = AddCounterpartySql;
            command.ExecuteNonQuery();
        }

        public List<string> GetMapSetCommandTextList(int step)
        {
            var setTextList = new List<string>();

            var commandText = mapper.GetMapSetCommandText("Activity", m => string.Format("'{0}'", m.Activity.Oid), m => m.Activity != null, step,
                "WHEN ApPmtDistn.Activity IS NOT NULL AND ApPmtDistn.Activity <> @UndefActivityOid THEN ApPmtDistn.Activity");
            if (!string.IsNullOrWhiteSpace(commandText))
                setTextList.Add(commandText);

            commandText = mapper.GetMapSetCommandText("Counterparty", 
                m => {
                    if (m.Counterparty != null)
                        return string.Format("'{0}'", m.Counterparty.Oid);
                    else
                        return string.Format("(SELECT TOP 1 c.Oid FROM Counterparty c WHERE c.Name LIKE {0} AND c.GCRecord IS NULL)", m.CounterpartyExpr);
                },
                m => m.Counterparty != null || !string.IsNullOrWhiteSpace(m.CounterpartyExpr), 
                step,
                "WHEN ApPmtDistn.Counterparty IS NOT NULL AND ApPmtDistn.Counterparty <> @UndefCounterpartyOid THEN ApPmtDistn.Counterparty");
            if (!string.IsNullOrWhiteSpace(commandText))
                setTextList.Add(commandText);

            setTextList.Add(@"Account = 
(
    SELECT TOP 1 a.Account FROM ApBankAccount a
    WHERE a.Oid = ApPmtDistn.BankAccount
)");

            return setTextList;
        }

    }
}
