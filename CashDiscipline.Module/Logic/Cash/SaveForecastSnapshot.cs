using CashDiscipline.Module.BusinessObjects.Cash;
using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
IF OBJECT_ID('tempdb..#TmpCashFlow') IS NOT NULL
BEGIN
DROP TABLE #TmpCashFlow
END

declare @ActualStatus int = 1
declare @ForecastStatus int = 0
*/

namespace CashDiscipline.Module.Logic.Cash
{
    public class SaveForecastSnapshot
    {
        private readonly XPObjectSpace objSpace;
        public SaveForecastSnapshot(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }

        public string ProcessCommandText

        {
            get
            {
                return
@"declare @NewSnapshot as uniqueidentifier = (SELECT CAST(CAST(NEWID() AS BINARY(10)) + CAST(GETDATE() AS BINARY(6)) AS UNIQUEIDENTIFIER))
declare @CurrentSnapshot as uniqueidentifier = (SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks WHERE GCRecord IS NULL)
declare @SnapshotNum as int = (SELECT Max(SequentialNumber) + 1 FROM CashFlowSnapshot WHERE GCRecord IS NULL)
declare @MinDate as datetime = (
	SELECT Min(TranDate) FROM CashFlow 
	WHERE GCRecord IS NULL AND Status = @ForecastStatus
		AND Snapshot = @CurrentSnapshot
)

INSERT INTO CashFlowSnapshot (Oid, Name, SequentialNumber, Description, FromDate, TimeCreated)
VALUES 
(
	@NewSnapshot, 
	'Snapshot ' + CAST(@SnapshotNum AS nvarchar(255)), 
	@SnapshotNum,
	'Snapshot ' + CAST(@SnapshotNum AS nvarchar(255)),
	@MinDate,
	GETDATE()
)

SELECT c.*
INTO #TmpCashFlow
FROM CashFlow c
WHERE c.GCRecord IS NULL
AND c.Snapshot = @CurrentSnapshot
AND c.TranDate >= @MinDate

UPDATE #TmpCashFlow SET
OrigCashFlow = Oid,
Oid = (SELECT CAST(CAST(NEWID() AS BINARY(10)) + CAST(GETDATE() AS BINARY(6)) AS UNIQUEIDENTIFIER)),
Snapshot = @NewSnapshot

INSERT INTO CashFlow
SELECT * FROM #TmpCashFlow

UPDATE CashFlowSnapshot
SET TimeCreated = GETDATE(),
FromDate = (SELECT Min(TranDate) FROM CashFlow WHERE GCRecord IS NULL)
WHERE Oid = @CurrentSnapshot";
            }
        }

        public void Process()
        {
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
                new SqlDeclareClause("ForecastStatus", "int", Convert.ToInt32(CashFlowStatus.Forecast).ToString()),
                new SqlDeclareClause("ActualStatus", "int", Convert.ToInt32(CashFlowStatus.Actual).ToString())
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
