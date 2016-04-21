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
        private FixCashFlowsSqlBuilder sqlBuilder;

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

            sqlBuilder = new FixCashFlowsSqlBuilder();

        }

        public IEnumerable<CashFlow> GetCashFlowsToFix()
        {
            return null;
        }

        public void ProcessCashFlows()
        {
            var sqlParamNames = new string[]
            {
                "FromDate",
                "ToDate",
                "Snapshot",
                "Fix"
            };
            var sqlParamValues = new object[]
            {
                paramObj.FromDate,
                paramObj.ToDate,
                currentSnapshot.Oid,
                reversalFixTag.Oid
            };
            
            objSpace.Session.ExecuteNonQuery(sqlBuilder.FixeeSql, sqlParamNames, sqlParamValues);

            objSpace.CommitChanges();
        }

        public void Reset()
        {

        }
        
        private CashFlowSnapshot GetCurrentSnapshot(Session session)
        {
            return CashFlowHelper.GetCurrentSnapshot(session);
        }

        #region Sql


        #endregion
    }

    public class FixCashFlowsSqlBuilder
    {
        public string FixeeTableName
        {
            get
            {
                return "tempdb..#Fixee";
            }
        }

        // params = @snapshot, @source
        public string FixeeSql
        {
            get
            {
                return string.Format(
@"
--Create Temp Table
SELECT * INTO [{0}]
FROM CashFlow
WHERE TranDate BETWEEN @FromDate AND @ToDate;

--Set new values
UPDATE [{0}] SET 
Snapshot = @Snapshot,
Oid = NEWID(),
Fix = @Fix

--insert back into Cash Flow table
INSERT INTO CashFlow
SELECT * FROM [{0}]", 
FixeeTableName);
            }
        }

    }
}
