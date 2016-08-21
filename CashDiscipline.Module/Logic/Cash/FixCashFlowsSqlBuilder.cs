using System;
using System.Collections.Generic;
using System.Linq;
using SmartFormat;

namespace CashDiscipline.Module.Logic.Cash
{
    public class FixCashFlowsSqlBuilder
    {
        public string CommandText
        {
            get
            {
                return Smart.Format(
@"-- CashFlowsToFix

IF OBJECT_ID('{tfixee}') IS NOT NULL DROP TABLE {tfixee};

SELECT cf.* INTO {tfixee}
FROM CashFlow cf
LEFT JOIN CashForecastFixTag tag ON tag.Oid = cf.Fix
LEFT JOIN CashFlow fixer ON fixer.Oid = cf.Oid
WHERE
    cf.GCRecord IS NULL
    AND tag.GCRecord IS NULL
    AND cf.TranDate BETWEEN @FromDate AND @ToDate
    AND (cf.Fix = NULL OR tag.FixTagType != @IgnoreFixTagType)
    AND (cf.IsFixeeSynced=0 OR cf.IsFixerSynced=0 OR NOT cf.IsFixerFixeesSynced=0)
    OR fixer.GCRecord IS NOT NULL
    ;

-- FixeeFixer

IF OBJECT_ID('{tfixeefixer}') IS NOT NULL DROP TABLE {tfixeefixer};

SELECT 
	fixee.Oid AS Fixee,
	(
	SELECT TOP 1 fixer.Oid
	FROM {tfixee} fixer
	WHERE fixee.TranDate BETWEEN fixer.FixFromDate AND fixer.FixToDate
	) AS Fixer
INTO {tfixeefixer}
FROM {tfixee} fixee;",
new {
    tfixee = "#TmpCashFlowsToFix",
    tfixeefixer = "#TmpFixeeFixer"
});
            }
        }
    }
}
