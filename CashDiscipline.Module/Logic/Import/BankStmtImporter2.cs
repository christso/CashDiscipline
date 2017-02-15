using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp.Xpo;
using System.Data.SqlClient;
using CashDiscipline.Module.Clients;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using System.Data;

namespace CashDiscipline.Module.Logic.Import
{
    public class BankStmtImporter2
    {
        public BankStmtImporter2()
        {
            importActionDic = new Dictionary<string, ImportActionDelegate>();
            importActionDic.Add("ANZ", ExecuteAnz);
            importActionDic.Add("CBA BOS", ExecuteCba);
            importActionDic.Add("CBA OP", ExecuteCba);
            importActionDic.Add("HSBC", ExecuteHsbc);
            importActionDic.Add("WBC", ExecuteWbc);
        }

        private Dictionary<string, ImportActionDelegate> importActionDic;
        private delegate string ImportActionDelegate(ImportBankStmtParamItem paramObj);

        public string Execute(IList<ImportBankStmtParamItem> paramObjs)
        {
            string messageText = string.Empty;
            foreach (ImportBankStmtParamItem paramObj in paramObjs.Where(p => p.Enabled))
            {
                ImportActionDelegate actionCaller = null;
                if (importActionDic.TryGetValue(paramObj.Name, out actionCaller))
                {
                    if (messageText != string.Empty)
                        messageText += "\r\n";
                    messageText += string.Format("{0}: {1}",
                        paramObj.Name,
                        actionCaller(paramObj));
                }
            }
            return messageText;
        }

        public string ExecuteAnz(ImportBankStmtParamItem paramObj)
        {
            var session = paramObj.Session;
            using (var loader = new SqlServerLoader2((SqlConnection)session.Connection))
            {
   
                loader.ColumnMappings.Add("TranDate", "TranDate");
                loader.ColumnMappings.Add("BankAccountNumber", "BankAccountNumber");
                loader.ColumnMappings.Add("TranType", "TranType");
                loader.ColumnMappings.Add("TranRef", "TranRef");
                loader.ColumnMappings.Add("TranAmount", "TranAmount");
                loader.ColumnMappings.Add("TranDescription", "TranDescription");
                loader.ColumnMappings.Add("TranCode", "TranCode");

                loader.CreateSql = CreateSql;

                loader.PersistSql = PersistSql;

                using (var csvReader = DataObjectFactory.CreateReaderFromCsv(paramObj.FilePath, false))
                {
                    csvReader.Columns = new List<LumenWorks.Framework.IO.Csv.Column>
                    {
                        new LumenWorks.Framework.IO.Csv.Column { Name = "TranDate", Type = typeof(DateTime) },
                        new LumenWorks.Framework.IO.Csv.Column { Name = "BankAccountNumber", Type = typeof(string) },
                        new LumenWorks.Framework.IO.Csv.Column { Name = "Column_2", Type = typeof(string) },
                        new LumenWorks.Framework.IO.Csv.Column { Name = "Column_3", Type = typeof(string) },
                        new LumenWorks.Framework.IO.Csv.Column { Name = "Column_4", Type = typeof(string) },
                        new LumenWorks.Framework.IO.Csv.Column { Name = "Currency", Type = typeof(string) },
                        new LumenWorks.Framework.IO.Csv.Column { Name = "Column_6", Type = typeof(string) },
                        new LumenWorks.Framework.IO.Csv.Column { Name = "TranType", Type = typeof(string) },
                        new LumenWorks.Framework.IO.Csv.Column { Name = "TranRef", Type = typeof(string) },
                        new LumenWorks.Framework.IO.Csv.Column { Name = "TranAmount", Type = typeof(decimal) },
                        new LumenWorks.Framework.IO.Csv.Column { Name = "TranDescription", Type = typeof(string) },
                        new LumenWorks.Framework.IO.Csv.Column { Name = "Column_11", Type = typeof(string) },
                        new LumenWorks.Framework.IO.Csv.Column { Name = "Column_12", Type = typeof(string) },
                        new LumenWorks.Framework.IO.Csv.Column { Name = "TranCode", Type = typeof(string) },
                        new LumenWorks.Framework.IO.Csv.Column { Name = "Column_14", Type = typeof(string) }
                    };
                    
                    var messagesText = loader.Execute(csvReader);
                    return messagesText;
                }
            }
        }

        public string ExecuteCba(ImportBankStmtParamItem paramObj)
        {
            var session = paramObj.Session;
            using (var loader = new SqlServerLoader2((SqlConnection)session.Connection))
            {
                loader.ColumnMappings.Add("Process date", "TranDate");
                loader.ColumnMappings.Add("Debit", "Debit");
                loader.ColumnMappings.Add("Credit", "Credit");
                loader.ColumnMappings.Add("Description", "TranDescription");

                loader.CreateSql = CreateSql + @"
ALTER TABLE {TempTable} ADD [Debit] money, [Credit] money;";

                loader.PersistSql =@"UPDATE {TempTable} SET 
TranAmount = COALESCE([Credit],0) - COALESCE([Debit],0),
BankAccountNumber = '{Account}'" + PersistSql;

                loader.SqlStringReplacers.Add("Account", paramObj.Account == null ? "" : paramObj.Account.Name);

                using (var csvReader = DataObjectFactory.CreateReaderFromCsv(paramObj.FilePath, true))
                {
                    var messagesText = loader.Execute(csvReader);
                    return messagesText;
                }
            }
        }

        public string ExecuteHsbc(ImportBankStmtParamItem paramObj)
        {
            var connection = (SqlConnection)paramObj.Session.Connection;
            using (var loader = new SqlServerLoader2((SqlConnection)connection))
            {
                
                loader.ColumnMappings.Add("Account number", "BankAccountNumber");
                loader.ColumnMappings.Add("Value date (dd/mm/yyyy)", "TranDate");
                loader.ColumnMappings.Add("TRN Type", "TranType");
                loader.ColumnMappings.Add("Customer reference", "TranRef");
                loader.ColumnMappings.Add("Additional narrative", "TranDescription");
                loader.ColumnMappings.Add("Credit amount", "Credit");
                loader.ColumnMappings.Add("Debit amount", "Debit");

                loader.CreateSql = CreateSql + @"
ALTER TABLE {TempTable} ADD [Debit] money, [Credit] money;";

                loader.PersistSql = @"UPDATE {TempTable} SET 
TranAmount = COALESCE([Credit],0) + COALESCE([Debit],0)
" + PersistSql;

                using (var sourceTable = DataObjectFactory.CreateTableFromExcelXml(paramObj.FilePath, "Data"))
                {
                    var messagesText = loader.Execute(sourceTable);
                    return messagesText;
                }

            }
        }

        public string ExecuteWbc(ImportBankStmtParamItem paramObj)
        {
            var session = paramObj.Session;
            using (var loader = new SqlServerLoader2((SqlConnection)session.Connection))
            {
                loader.ColumnMappings.Add("TRAN_DATE", "TranDateText");
                loader.ColumnMappings.Add("ACCOUNT_NO", "BankAccountNumber");
                loader.ColumnMappings.Add("AMOUNT", "TranAmount");
                loader.ColumnMappings.Add("NARRATIVE", "TranDescription");
                loader.ColumnMappings.Add("TRAN_CODE", "TranCode");
                loader.ColumnMappings.Add("SERIAL", "TranRef");

                loader.CreateSql = CreateSql + @"
ALTER TABLE {TempTable} ADD [TranDateText] nvarchar(50);";
                loader.PersistSql = @"UPDATE {TempTable} SET 
TranDate = CAST(TranDateText AS date);
" + PersistSql;
                
                using (var csvReader = DataObjectFactory.CreateReaderFromCsv(paramObj.FilePath, true))
                {
                    var messagesText = loader.Execute(csvReader);
                    return messagesText;
                }
            }
        }

        #region Scripts

        private const string CreateSql = @"CREATE TABLE {TempTable} (
    [TranDate] [datetime]
        NULL,
    [BankAccountNumber]
        [nvarchar](50) NULL,
    [TranType]
        [nvarchar](50) NULL,
    [TranRef]
        [nvarchar](50) NULL,
    [TranAmount]
        [money]
        NULL,
    [TranDescription]
        [nvarchar](255) NULL,
    [TranCode]
        [nvarchar](10) NULL,
    [Oid]
        [uniqueidentifier]
        NULL
    )";

        private const string PersistSql = @"/* Auto-Insert new TranCodes */
DECLARE @DefTranCode uniqueidentifier = (SELECT TranCode FROM BankStmtDefaults WHERE GCRecord IS NULL)

INSERT INTO BankStmtTranCode (Oid, Name)

SELECT 
	(SELECT CAST(CAST(NEWID() AS BINARY(10)) + CAST(GETDATE() AS BINARY(6)) AS UNIQUEIDENTIFIER)) AS Oid,
	b.TranCode
FROM
(
	SELECT DISTINCT b.TranCode
	FROM {TempTable} b
	WHERE b.TranCode NOT IN (SELECT tc.Name FROM BankStmtTranCode tc WHERE tc.GCRecord IS NULL)
) b

/* Persist Bank Stmt Data */
DECLARE @funcCcy uniqueidentifier
SET @funcCcy = (SELECT TOP 1 FunctionalCurrency FROM SetOfBooks WHERE GCRecord IS NULL)

INSERT INTO BankStmt
(
	Oid,
	TranDate,
	TranType,
	TranRef,
	TranAmount,
	TranDescription,
	TranCode,
	Account,
	ValueDate,
	FunctionalCcyAmt,
	CounterCcyAmt,
	CounterCcy,
	TimeEntered
)
SELECT 
	CAST(CAST(NEWID() AS BINARY(10)) + CAST(GETDATE() AS BINARY(6)) AS UNIQUEIDENTIFIER),
	tbs.TranDate,
	tbs.TranType,
	tbs.TranRef,
	tbs.TranAmount,
	tbs.TranDescription,
	tbs.TranCode,
	tbs.Account,
	tbs.TranDate,
	ROUND(COALESCE((
		SELECT TOP 1 r.ConversionRate FROM ForexRate r 
		WHERE r.GCRecord IS NULL
			AND r.FromCurrency = tbs.AccountCcy
			AND r.ToCurrency = @funcCcy
			AND r.ConversionDate = (
				SELECT MAX(r1.ConversionDate)
				FROM ForexRate r1
				WHERE r1.GCRecord IS NULL
					AND r1.FromCurrency = tbs.AccountCcy
					AND r1.ToCurrency = @funcCcy
			)
	),1) * tbs.TranAmount, 2) AS FunctionalCcyAmt,
	tbs.TranAmount AS CounterCcyAmt,	
	tbs.AccountCcy AS CounterCcy,
	GETDATE() AS TimeEntered
FROM
(
	SELECT
		tbs.*,
		(SELECT a.Currency FROM Account a WHERE a.GCRecord IS NULL AND a.Oid = tbs.Account) AS AccountCcy
	FROM
	(
		SELECT
			tbs.Oid,
			tbs.TranDate,
			tbs.TranType,
			tbs.TranRef,
			tbs.TranAmount,
			tbs.TranDescription,
			COALESCE ( (SELECT tc.Oid FROM BankStmtTranCode tc WHERE tc.GCRecord IS NULL AND tc.Name LIKE tbs.TranCode), @DefTranCode ) AS TranCode,
			(SELECT baim.Account FROM BankStmtAccountImportMapping baim WHERE baim.GCRecord IS NULL AND baim.InputAccountText LIKE tbs.BankAccountNumber) AS Account
		FROM {TempTable} tbs
		WHERE tbs.TranAmount <> 0
	) tbs
) tbs";

        #endregion
    }
}
