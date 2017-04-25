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
using System.ComponentModel;
using System.Linq;
using DevExpress.Persistent.BaseImpl;
using Xafology.ExpressApp.Xpo.Import;
using CashDiscipline.Module.BusinessObjects.Cash;
using CashDiscipline.Module.BusinessObjects;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Diagnostics;
using SmartFormat;

/* PARAMETERS
DECLARE @FromDate date = (SELECT TOP 1 FromDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @ToDate date = (SELECT TOP 1 ToDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @ApayableLockdownDate date = (SELECT TOP 1 ApayableLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @ApayableNextLockdownDate date = (SELECT TOP 1 ApayableNextLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @PayrollLockdownDate date = (SELECT TOP 1 PayrollLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @PayrollNextLockdownDate date = (SELECT TOP 1 PayrollNextLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @IgnoreFixTagType int = 0
DECLARE @AllocateFixTagType int = 1
DECLARE @ScheduleInFixTagType int = 2
DECLARE @ScheduleOutFixTagType int = 3
DECLARE @ForecastStatus int = 0
DECLARE @Snapshot uniqueidentifier = COALESCE(
	(SELECT TOP 1 [Snapshot] FROM CashFlowFixParam WHERE GCRecord IS NULL),
	(SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks WHERE GCRecord IS NULL)
)
DECLARE @DefaultCounterparty uniqueidentifier = (SELECT TOP 1 [Counterparty] FROM CashFlowDefaults WHERE GCRecord IS NULL)
DECLARE @FunctionalCurrency uniqueidentifier = (SELECT TOP 1 [FunctionalCurrency] FROM SetOfBooks WHERE GCRecord IS NULL)
DECLARE @ReversalFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'R' AND GCRecord IS NULL)
DECLARE @RevRecFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'RR' AND GCRecord IS NULL)
DECLARE @ResRevRecFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'RRR' AND GCRecord IS NULL)
DECLARE @PayrollFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'PR' AND GCRecord IS NULL)
DECLARE @AutoFixTag uniqueidentifier = (SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'Auto' AND GCRecord IS NULL)
DECLARE @ApReclassActivity uniqueidentifier = (SELECT TOP 1 ApReclassActivity FROM CashFlowFixParam WHERE GCRecord IS NULL)
DECLARE @UndefActivity uniqueidentifier = (select oid from activity where activity.name like 'UNDEFINED' and GCRecord IS NULL)
 */

/* DEBUG

SELECT *, '#TmpFixReversal' AS TableName FROM #TmpFixReversal
UNION ALL
SELECT *, '#TmpFixRevReclass_Fixee' AS TableName FROM #TmpFixRevReclass_Fixee
UNION ALL
SELECT *, '#TmpFixRevReclass_Fixer' AS TableName FROM #TmpFixRevReclass_Fixer
UNION ALL
SELECT *, '#TmpFixResRevRec_Fixee' AS TableName FROM #TmpFixResRevRec_Fixee
UNION ALL
SELECT *, '#TmpFixResRevReclass_Fixer' AS TableName FROM #TmpFixResRevReclass_Fixer

SELECT fixeeFixer.*, 
	fixee.TranDate, 
	fixee.Counterparty,
	fixee.AccountCcyAmt,
	fixee.Account,
	fixeeAccount.FixAccount,
	fixee.FixFromDate,
	fixee.FixToDate
FROM #TmpFixeeFixer fixeeFixer
LEFT JOIN CashFlow fixee ON fixee.Oid = fixeeFixer.Fixee
LEFT JOIN Account fixeeAccount ON fixee.Account = fixeeAccount.Oid
*/

namespace CashDiscipline.Module.Logic.Cash
{
    public class FixCashFlowsAlgorithm : IFixCashFlows
    {
        private XPObjectSpace objSpace;
        private CashFlowFixParam paramObj;
        private Activity paramApReclassActivity;
        private Counterparty defaultCounterparty;
        private CashForecastFixTag reversalFixTag;
        private CashForecastFixTag revRecFixTag;
        private CashForecastFixTag resRevRecFixTag;
        private CashForecastFixTag payrollFixTag;
        private CashForecastFixTag autoFixTag;
        private SetOfBooks setOfBooks;
        private CashFlowSnapshot currentSnapshot;
        private CashFlowFixMapper mapper;
        private List<SqlDeclareClause> sqlDeclareClauses;
        private readonly string parameterCommandText;

        public FixCashFlowsAlgorithm(XPObjectSpace objSpace, CashFlowFixParam paramObj)
            : this(objSpace, paramObj, new CashFlowFixMapper(objSpace))
        {

        }

        public FixCashFlowsAlgorithm(XPObjectSpace objSpace, CashFlowFixParam paramObj, CashFlowFixMapper mapper)
        {
            this.objSpace = objSpace;
            this.paramObj = paramObj;

            if (paramObj.Snapshot == null)
                currentSnapshot = GetCurrentSnapshot(objSpace.Session);
            else
                currentSnapshot = paramObj.Snapshot;

            if (paramObj.ApReclassActivity != null)
                paramApReclassActivity = objSpace.GetObjectByKey<Activity>(objSpace.GetKeyValue(paramObj.ApReclassActivity));

            defaultCounterparty = objSpace.FindObject<Counterparty>(
             CriteriaOperator.Parse("Name LIKE ?", CashDiscipline.Common.Constants.DefaultFixCounterparty));

            var query = new XPQuery<CashForecastFixTag>(objSpace.Session);

            reversalFixTag = query
                .Where(x => x.Name == CashDiscipline.Common.Constants.ReversalFixTag).FirstOrDefault();

            revRecFixTag = query
                .Where(x => x.Name == CashDiscipline.Common.Constants.RevRecFixTag).FirstOrDefault();

            resRevRecFixTag = query
                .Where(x => x.Name == CashDiscipline.Common.Constants.ResRevRecFixTag).FirstOrDefault();

            payrollFixTag = query
                .Where(x => x.Name == CashDiscipline.Common.Constants.PayrollFixTag).FirstOrDefault();

            autoFixTag = query
                .Where(x => x.Name == CashDiscipline.Common.Constants.AutoFixTag).FirstOrDefault();

            setOfBooks = SetOfBooks.GetInstance(objSpace);
            this.mapper = mapper;

            this.sqlDeclareClauses = CreateSqlDeclareClauses();
            var sqlStringUtil = new SqlStringUtil();
            this.parameterCommandText = sqlStringUtil.CreateCommandText(sqlDeclareClauses);
        }

        public void ProcessCashFlows()
        {
            mapper.Process();
            Rephase();
            ApplyFix();
        }

        public List<SqlDeclareClause> CreateSqlDeclareClauses()
        {
            var clauses = new List<SqlDeclareClause>()
            {
                new SqlDeclareClause("FromDate", "date", "(SELECT TOP 1 FromDate FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("ToDate", "date", "(SELECT TOP 1 ToDate FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("ApayableLockdownDate", "date", "(SELECT TOP 1 ApayableLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("ApayableNextLockdownDate", "date", "(SELECT TOP 1 ApayableNextLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("IgnoreFixTagType", "int", "0"),
                new SqlDeclareClause("AllocateFixTagType", "int", "1"),
                new SqlDeclareClause("ScheduleInFixTagType", "int", "2"),
                new SqlDeclareClause("ScheduleOutFixTagType", "int", "3"),
                new SqlDeclareClause("BasicFixTagType", "int", "4"),
                new SqlDeclareClause("DlrComDayOfWeek", "int", "(SELECT TOP 1 DlrComDayOfWeek FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("ForecastStatus", "int", "0"),
                new SqlDeclareClause("Snapshot", "uniqueidentifier", @"COALESCE(
	(SELECT TOP 1 [Snapshot] FROM CashFlowFixParam WHERE GCRecord IS NULL),
	(SELECT TOP 1 [CurrentCashFlowSnapshot] FROM SetOfBooks WHERE GCRecord IS NULL)
)"),
                new SqlDeclareClause("DefaultCounterparty", "uniqueidentifier",
                    "(SELECT TOP 1 [Counterparty] FROM CashFlowDefaults WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("FunctionalCurrency", "uniqueidentifier",
                    "(SELECT TOP 1 [FunctionalCurrency] FROM SetOfBooks WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("ReversalFixTag", "uniqueidentifier",
                    "(SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'R' AND GCRecord IS NULL)"),
                new SqlDeclareClause("RevRecFixTag", "uniqueidentifier",
                    "(SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'RR' AND GCRecord IS NULL)"),
                new SqlDeclareClause("ResRevRecFixTag", "uniqueidentifier",
                    "(SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'RRR' AND GCRecord IS NULL)"),
                new SqlDeclareClause("PayrollFixTag", "uniqueidentifier",
                    "(SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'PR' AND GCRecord IS NULL)"),
                new SqlDeclareClause("DlrComFixTag", "uniqueidentifier",
                    "(SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'DC' AND GCRecord IS NULL)"),
                new SqlDeclareClause("AutoFixTag", "uniqueidentifier",
                    "(SELECT TOP 1 Oid FROM CashForecastFixTag WHERE Name LIKE 'Auto' AND GCRecord IS NULL)"),
                new SqlDeclareClause("ApReclassActivity", "uniqueidentifier",
                    "(SELECT TOP 1 ApReclassActivity FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("PayrollLockdownDate", "date",
                    "(SELECT TOP 1 PayrollLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("PayrollNextLockdownDate", "date",
                    "(SELECT TOP 1 PayrollNextLockdownDate FROM CashFlowFixParam WHERE GCRecord IS NULL)"),
                new SqlDeclareClause("AutoForexSettleType", "int", Convert.ToString(
                        Convert.ToInt32(CashFlowForexSettleType.Auto)))

            };
            return clauses;
        }


        public void ApplyFix()
        {
            if (defaultCounterparty == null)
                throw new ArgumentException("DefaultCounterparty");
            if (paramApReclassActivity == null)
                throw new ArgumentNullException("ApReclassActivity");
            //if (payrollFixTag == null)
            //    throw new ArgumentException("PayrollFixTag");

            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            command.CommandTimeout = CashDiscipline.Common.Constants.SqlCommandTimeout;
            //command.Parameters.AddRange(parameters.ToArray());
            //command.CommandText = this.parameterCommandText + "\n\n" + ProcessCommandText;
            command.CommandText = "EXEC dbo.sp_cashflow_fix";
            int result = command.ExecuteNonQuery();
        }

        public void Map()
        {
            mapper.Process();
        }

        public void Rephase()
        {
            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            //command.Parameters.AddRange(parameters.ToArray());

            command.CommandText =  this.parameterCommandText + "\n\n" + RephaseCommandText;
            command.ExecuteNonQuery();
        }



        public void Reset()
        {
            var conn = (SqlConnection)objSpace.Session.Connection;
            var command = conn.CreateCommand();
            command.CommandText = ResetCommandText;

            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("Snapshot", currentSnapshot.Oid),
            };

            command.Parameters.AddRange(parameters.ToArray());
            command.ExecuteNonQuery();
        }

        private CashFlowSnapshot GetCurrentSnapshot(Session session)
        {
            return CashFlowHelper.GetCurrentSnapshot(session);
        }

        #region Core Sql

        public static string ResetCommandText
        {
            get
            {
                //TODO: unhack: ft.Name IN ('R','RR','RRR')
                return
@"UPDATE CashFlow SET
GCRecord = CAST(RAND() * 2147483646 + 1 AS INT),
Fixer = NULL,
ParentCashFlow = NULL
FROM CashFlow
LEFT JOIN CashForecastFixTag ft 
	ON ft.Oid = CashFlow.Fix AND ft.GCRecord IS NULL

WHERE ft.Name IN ('R','RR','RRR')
AND CashFlow.[Snapshot] = @Snapshot
AND CashFlow.GCRecord IS NULL;";
            }
        }

        public string RephaseCommandText
        {
            get
            {
                return
@"DECLARE @MaxActualDate date = (
    SELECT Max(TranDate) FROM CashFlow
    WHERE CashFlow.[Snapshot] = @Snapshot
    AND CashFlow.Status != @ForecastStatus
	AND CashFlow.GCRecord IS NULL
);

-- Set Monday as day number 1
SET DATEFIRST 1

UPDATE cf SET TranDate = CASE

-- adjust date of dealer commission payments to the next Wednesday or day set by DlrComDayOfWeek
WHEN cf.Fix = @DlrComFixTag
AND cf.FixRank > 2 AND cf.TranDate <= @ApayableLockdownDate
THEN CASE
    WHEN @DlrComDayOfWeek <= DATEPART(weekday, @ApayableLockdownDate)
    THEN DATEADD(d, - DATEPART(weekday, @ApayableLockdownDate) + @DlrComDayOfWeek + 7, @ApayableLockdownDate)
    ELSE DATEADD(d, - DATEPART(weekday, @ApayableLockdownDate) + @DlrComDayOfWeek, @ApayableLockdownDate) 
    END

-- adjust date of payroll payments
WHEN cf.Fix = @PayrollFixTag
	AND cf.FixRank > 2 AND cf.TranDate <= @PayrollLockdownDate
THEN @PayrollNextLockdownDate

-- move AP payments forecast to next lockdown date
WHEN FixTag.FixTagType IN (@ScheduleOutFixTagType)
	AND cf.FixRank > 2 AND cf.TranDate <= @ApayableLockdownDate
THEN @ApayableNextLockdownDate

-- move business and planning forecast to next week 
WHEN FixTag.FixTagType IN (@AllocateFixTagType, @BasicFixTagType)
	AND cf.FixRank > 2 AND cf.TranDate <= @MaxActualDate
THEN DATEADD(d, 7, @MaxActualDate)

-- move receipts to forecast period
WHEN FixTag.FixTagType = @ScheduleInFixTagType
    AND cf.FixRank > 2
    AND cf.TranDate <= @MaxActualDate
THEN DATEADD(d, 1, @MaxActualDate)

ELSE cf.TranDate

END
FROM CashFlow cf
LEFT JOIN CashForecastFixTag FixTag ON FixTag.Oid = cf.Fix
WHERE cf.[Status] = @ForecastStatus
AND cf.[Snapshot] = @Snapshot
AND cf.GCRecord IS NULL

-- delete everything else
UPDATE CashFlow SET
GCRecord = CAST(RAND() * 2147483646 + 1 AS INT),
Activity = NULL,
Account = NULL,
Counterparty = NULL,
Source = NULL,
CounterCcy = NULL
WHERE 
    Snapshot = @Snapshot
    AND [Status] = @ForecastStatus
    AND TranDate <= @MaxActualDate
	AND GCRecord IS NULL

";
            }
        }

        #endregion
    }
}
