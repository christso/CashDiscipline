using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.ParamObjects.Cash;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Forex
{
    public class ForexRateDeduplicator
    {
        private readonly XPObjectSpace objSpace;

        public ForexRateDeduplicator(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }

        public string ProcessCommandText

        {
            get
            {
                return
@"UPDATE ForexRate
SET GCRecord = CAST(RAND() * 2147483646 + 1 AS INT)
WHERE Oid IN
(
	SELECT
		Oid
	FROM
	(
		SELECT
			Oid,
			ConversionDate,
			FromCurrency,
			ToCurrency,
			ConversionRate,
			ROW_NUMBER() OVER (PARTITION BY ConversionDate, FromCurrency, ToCurrency ORDER BY ConversionDate) AS Row_Num
		FROM ForexRate
		WHERE GCRecord IS NULL
	) fr
	WHERE fr.Row_Num > 1
)"
;
            }
        }

        public void Process()
        {
            objSpace.CommitChanges(); // persist parameters

            var clauses = CreateSqlParameters();
            var parameters = CreateParameters(clauses);

            using (var cmd = ((SqlConnection)objSpace.Session.Connection).CreateCommand())
            {
                cmd.Parameters.AddRange(parameters.ToArray());
                cmd.CommandText = ProcessCommandText;
                cmd.ExecuteNonQuery();
            }
        }

        public List<SqlDeclareClause> CreateSqlParameters()
        {
            var clauses = new List<SqlDeclareClause>()
            {
                //new SqlDeclareClause("FromDate", "date", "(SELECT TOP 1 FromDate FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
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
    }
}
