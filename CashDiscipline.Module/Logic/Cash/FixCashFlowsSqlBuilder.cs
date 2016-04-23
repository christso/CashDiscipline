using System;
using System.Collections.Generic;
using System.Linq;

namespace CashDiscipline.Module.Logic.Cash
{
    public class FixCashFlowsSqlBuilder
    {
        public string CommandText
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
	WHERE fixee.TranDate BETWEEN fixer.FixFromDate AND fixer.FixToDate
	) AS Fixer
INTO temp_FixeeFixer
FROM temp_CashFlowsToFix fixee;
";
            }
        }
        
    }
}
