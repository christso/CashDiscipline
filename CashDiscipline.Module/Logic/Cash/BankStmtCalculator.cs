using CashDiscipline.Module.BusinessObjects;
using CashDiscipline.Module.BusinessObjects.BankStatement;
using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.Xpo;
using SmartFormat;
using System.Data.SqlClient;

namespace CashDiscipline.Module.Logic.Cash
{
    public class BankStmtCalculator
    {
 
        public BankStmtCalculator(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;

            var clauses = new List<SqlDeclareClause>()
            {
                new SqlDeclareClause("FuncCcy", "uniqueidentifier",
                "(SELECT TOP 1 FunctionalCurrency FROM SetOfBooks WHERE GCRecord IS NULL)")
            };

            var sqlUtil = new SqlStringUtil();
            parameterCommandText = sqlUtil.CreateCommandText(clauses);
        }

        private XPObjectSpace objSpace;
        private readonly string parameterCommandText = string.Empty;
        
        public string CreateProcessCommandText(string conditionText)
        {
                return Smart.Format(
@"IF OBJECT_ID('tempdb..#BankStmtoids') IS NOT NULL DROP TABLE #BankStmtoids
IF OBJECT_ID('tempdb..#AcctForexRates') IS NOT NULL DROP TABLE #AcctForexRates

SELECT Oid
INTO #BankStmtOids
FROM BankStmt bs
WHERE {Condition}

SELECT 
	bs.*,
	(
		SELECT TOP 1 fr.ConversionRate
		FROM ForexRate fr
		WHERE fr.ConversionDate = MaxConvDate
			AND fr.FromCurrency = @FuncCcy
			AND fr.ToCurrency = bs.Currency
	) AS ConvRate
INTO #AcctForexRates
FROM
(
	SELECT 
		bs.*,
		(
			SELECT MAX(fr.ConversionDate)
			FROM ForexRate fr
			WHERE fr.ConversionDate <= bs.TranDate
				AND fr.FromCurrency = @FuncCcy
				AND fr.ToCurrency = bs.Currency
				AND fr.GCRecord IS NULL
		) AS MaxConvDate
	FROM
	(
		SELECT DISTINCT
			bs.TranDate,
			acct.Currency
		FROM BankStmt bs
		LEFT JOIN Account acct ON acct.Oid = bs.Account AND acct.GCRecord IS NULL
		WHERE EXISTS (SELECT * FROM #BankStmtOids bso WHERE bso.Oid = bs.Oid)
	) bs
) bs

UPDATE bs SET
FunctionalCcyAmt = TranAmount / (
	SELECT TOP 1 afr.ConvRate
	FROM #AcctForexRates afr
	WHERE afr.TranDate = bs.TranDate
		AND afr.Currency = acct.Currency
)
FROM BankStmt bs
LEFT JOIN Account acct ON acct.Oid = bs.Account AND acct.GCRecord IS NULL
WHERE EXISTS (SELECT * FROM #BankStmtOids bso WHERE bso.Oid = bs.Oid)
", new { Condition = conditionText });
        }
        
        public void Process(IEnumerable objs)
        {
            Type objType = null;
            foreach (var obj in objs)
            {
                objType = obj.GetType();
            }
            if (objType == null) return;
            var criteria = new InOperator(objSpace.GetKeyPropertyName(objType),
                objs);
            var sqlWhere = CriteriaToWhereClauseHelper.GetMsSqlWhere(XpoCriteriaFixer.Fix(criteria));

            string processCommandText = CreateProcessCommandText(sqlWhere);

            using (var cmd = ((SqlConnection)objSpace.Session.Connection).CreateCommand())
            {
                cmd.CommandText = parameterCommandText + "\n\n" + processCommandText;
                cmd.ExecuteNonQuery();
            }
        }

    }
}
