using CashDiscipline.Module.BusinessObjects.Forex;
using CashDiscipline.Module.BusinessObjects.Setup;
using CashDiscipline.Module.ParamObjects.Cash;
using Xafology.ExpressApp.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xafology.Spreadsheet.Attributes;
using DevExpress.Persistent.BaseImpl;
using Xafology.ExpressApp.Xpo.Import;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects;
using System.Data.SqlClient;

namespace CashDiscipline.Module.Logic.Cash
{
    public class SqlFixCashFlowsAlgorithm : IFixCashFlows
    {
        private XPObjectSpace objSpace;
        private CashFlowFixParam paramObj;
        private Activity paramApReclassActivity;
        private Counterparty defaultCounterparty;
        private CashForecastFixTag reversalFixTag;
        private CashForecastFixTag revRecFixTag;
        private CashForecastFixTag resRevRecFixTag;
        private CashForecastFixTag payrollFixTag;
        private SetOfBooks setOfBooks;
        private FixCashFlowsRephaser rephaser;
        private IEnumerable<CashFlowFixMapping> cashFlowMappings;
        private CashFlowFixMapper cashFlowMapper;
        private CashFlowSnapshot currentSnapshot;

        public SqlFixCashFlowsAlgorithm(XPObjectSpace objSpace, CashFlowFixParam paramObj)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;

            if (paramObj.Snapshot == null)
                currentSnapshot = GetCurrentSnapshot(objSpace.Session);
            else
                currentSnapshot = paramObj.Snapshot;

            if (paramObj.ApReclassActivity == null)
                throw new ArgumentNullException("ApReclassActivity");
            paramApReclassActivity = objSpace.GetObjectByKey<Activity>(objSpace.GetKeyValue(paramObj.ApReclassActivity));
            defaultCounterparty = objSpace.FindObject<Counterparty>(
             CriteriaOperator.Parse("Name LIKE ?", Constants.DefaultFixCounterparty));

            var query = new XPQuery<CashForecastFixTag>(objSpace.Session);

            reversalFixTag = query
                .Where(x => x.Name == Constants.ReversalFixTag).FirstOrDefault();

            revRecFixTag = query
                .Where(x => x.Name == Constants.RevRecFixTag).FirstOrDefault();

            resRevRecFixTag = query
                .Where(x => x.Name == Constants.ResRevRecFixTag).FirstOrDefault();

            payrollFixTag = query
                .Where(x => x.Name == Constants.PayrollFixTag).FirstOrDefault();

            //this.cashFlowsToDelete = new List<CashFlow>();
            setOfBooks = SetOfBooks.GetInstance(objSpace);
        }

        public IEnumerable<CashFlow> GetCashFlowsToFix()
        {
            return null;
        }

        public void ProcessCashFlows()
        {

            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            command.CommandText = SqlCommandText;

            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("FromDate", paramObj.FromDate),
                new SqlParameter("ToDate", paramObj.ToDate),
                new SqlParameter("Snapshot", currentSnapshot.Oid),
                new SqlParameter("Fix", reversalFixTag.Oid),
                new SqlParameter("IgnoreFixTagType", Convert.ToInt32(CashForecastFixTagType.Ignore)),
            };

            command.Parameters.AddRange(parameters.ToArray());

            command.ExecuteNonQuery();
        }

        public void Reset()
        {

        }
        
        private CashFlowSnapshot GetCurrentSnapshot(Session session)
        {
            return CashFlowHelper.GetCurrentSnapshot(session);
        }

        #region Sql

        public string SqlCommandText
        {
            get
            {
                return
@"-- CashFlowsToFix

IF OBJECT_ID('temp_CashFlowsToFix') IS NOT NULL DROP TABLE temp_CashFlowsToFix;

SELECT cf.* INTO temp_CashFlowsToFix
FROM CashFlow cf
LEFT JOIN CashForecastFixTag tag ON tag.Oid = cf.Fix
LEFT JOIN CashFlow fixer ON fixer.Oid = cf.Oid
WHERE
    cf.GCRecord IS NULL
    AND tag.GCRecord IS NULL
    AND cf.TranDate BETWEEN @FromDate AND @ToDate
	AND cf.[Snapshot] = @Snapshot
    AND (cf.Fix = NULL OR tag.FixTagType != @IgnoreFixTagType)
    AND (cf.IsFixeeSynced=0 OR cf.IsFixerSynced=0 OR NOT cf.IsFixerFixeesSynced=0)
    OR fixer.GCRecord IS NOT NULL
    ;

-- FixeeFixer

IF OBJECT_ID('temp_FixeeFixer') IS NOT NULL DROP TABLE temp_FixeeFixer;

SELECT 
	fixee.Oid AS Fixee,
	(
	SELECT TOP 1 fixer.Oid
	FROM temp_CashFlowsToFix fixer
	WHERE 
		fixee.TranDate BETWEEN fixer.FixFromDate AND fixer.FixToDate
		AND fixee.FixActivity = fixer.FixActivity
		AND fixer.[Status] = @ForecastStatus
		AND fixer.FixRank > fixee.FixRank
		AND fixer.Counterparty IS NULL OR fixer.Counterparty = @DefaultCounterparty
	) AS Fixer
INTO temp_FixeeFixer
FROM temp_CashFlowsToFix fixee;
";
            }
        }

        #endregion
    }

}
