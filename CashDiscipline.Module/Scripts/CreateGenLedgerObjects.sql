/* Gen Ledger Upload */

IF OBJECT_ID('dbo.sp_GenLedgerUpload') IS NOT NULL
	BEGIN		
		DROP PROCEDURE dbo.sp_GenLedgerFile;
	END;

GO

CREATE PROCEDURE dbo.sp_GenLedgerUpload
(
	@RequestId int
)
AS
SELECT
	gl.GlCompany,
	gl.GlAccount,
	gl.GlCostCentre,
	gl.GlProduct,
	gl.GlSalesChannel,
	gl.GlCountry,
	gl.GlIntercompany,
	gl.GlProject,
	gl.GlLocation,
	SUM(CASE WHEN gl.FunctionalCcyAmt > 0 THEN gl.FunctionalCcyAmt ELSE NULL END) AS Debit,
	-SUM(CASE WHEN gl.FunctionalCcyAmt < 0 THEN gl.FunctionalCcyAmt ELSE NULL END) AS Credit,
	gl.GlDescription,
	gl.GlDate,
	(SELECT jg.Name FROM FinJournalGroup jg WHERE jg.GCRecord IS NULL AND jg.Oid = gl.JournalGroup) AS JournalGroup,
	acct.Name AS SrcAccount,
	acty.Name AS Activity,
	gl.IsActivity,
	SUM(gl.FunctionalCcyAmt) AS NetAmount
FROM (
	SELECT * FROM GenLedger gl
	WHERE gl.GCRecord IS NULL
) gl
LEFT JOIN (SELECT * FROM BankStmt bs WHERE bs.GCRecord IS NULL) bs ON bs.Oid = gl.SrcBankStmt
LEFT JOIN (SELECT * FROM CashFlow cf WHERE cf.GCRecord IS NULL) cf ON cf.Oid = gl.SrcCashFlow
LEFT JOIN (SELECT * FROM Activity acty WHERE acty.GCRecord IS NULL) acty ON acty.Oid = gl.Activity
LEFT JOIN (SELECT * FROM Account acct WHERE acct.GCRecord IS NULL) acct ON acct.Oid = COALESCE(bs.Account, cf.Account)
INNER JOIN TmpGenLedgerReportHdrParam tph ON gl.GlDate BETWEEN tph.FromDate AND tph.ToDate
AND tph.RequestId = @RequestId
INNER JOIN TmpGenLedgerReportParam tp ON 
tp.JournalGroup = (SELECT jg.Name FROM FinJournalGroup jg WHERE jg.GCRecord IS NULL AND jg.Oid = gl.JournalGroup)
AND tp.RequestId = @RequestId
GROUP BY
	gl.GlDate,
	gl.GlCompany,
	gl.GlAccount,
	gl.GlCostCentre,
	gl.GlProduct,
	gl.GlSalesChannel,
	gl.GlCountry,
	gl.GlIntercompany,
	gl.GlProject,
	gl.GlLocation,
	gl.GlDescription,
	acct.Name,
	acty.Name,
	gl.IsActivity,
	gl.JournalGroup
HAVING SUM(gl.FunctionalCcyAmt) <> 0
ORDER BY 
	gl.JournalGroup, 
	gl.GlDate, 
	acct.Name,
	gl.GlDescription,
	gl.IsActivity

/* Gen Ledger File */

IF OBJECT_ID('dbo.sp_GenLedgerFile') IS NOT NULL
	BEGIN		
		DROP PROCEDURE dbo.sp_GenLedgerFile;
	END;

GO

CREATE PROCEDURE sp_GenLedgerFile 
(
	@RequestId int
)
AS
SELECT
gl.*
FROM
(
	SELECT 
		CAST(gl.Oid AS nvarchar(255)) AS Oid,
		CAST(COALESCE(gl.SrcCashFlow, gl.SrcBankStmt) AS nvarchar(255)) AS SrcOid,
		CASE
			WHEN gl.SrcCashFlow IS NOT NULL THEN 'CashFlow'
			WHEN gl.SrcBankStmt IS NOT NULL THEN 'BankStmt'
			ELSE 'Unknown'
		END AS SourceType,
		(SELECT jg.Name FROM FinJournalGroup jg WHERE jg.GCRecord IS NULL AND jg.Oid = gl.JournalGroup) AS JournalGroup,
		gl.GlDate,
		acct.Name AS Account,
		acty.Name AS Activity,
		gl.FunctionalCcyAmt,
		gl.GlCompany,
		gl.GlAccount,
		gl.GlCostCentre,
		gl.GlProduct,
		gl.GlSalesChannel,
		gl.GlCountry,
		gl.GlIntercompany,
		gl.GlProject,
		gl.GlLocation,
		gl.GlDescription,
		CASE 
			WHEN gl.EntryType = 0 THEN 'Auto'
			WHEN gl.EntryType = 1 THEN 'Manual'
		END AS EntryType,
		gl.IsActivity
	FROM (
		SELECT * FROM GenLedger gl
		WHERE gl.GCRecord IS NULL
	) gl
	LEFT JOIN (SELECT * FROM BankStmt bs WHERE bs.GCRecord IS NULL) bs ON bs.Oid = gl.SrcBankStmt
	LEFT JOIN (SELECT * FROM CashFlow cf WHERE cf.GCRecord IS NULL) cf ON cf.Oid = gl.SrcCashFlow
	LEFT JOIN (SELECT * FROM Activity acty WHERE acty.GCRecord IS NULL) acty ON acty.Oid = gl.Activity
	LEFT JOIN (SELECT * FROM Account acct WHERE acct.GCRecord IS NULL) acct ON acct.Oid = COALESCE(bs.Account, cf.Account)
) gl
INNER JOIN TmpGenLedgerReportHdrParam tph ON gl.GlDate BETWEEN tph.FromDate AND tph.ToDate
	AND tph.RequestId = @RequestId
INNER JOIN TmpGenLedgerReportParam tp ON tp.JournalGroup = gl.JournalGroup AND tp.RequestId = @RequestId