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
            importActionDic.Add("ANZ", ExecuteAnz2);
        }

        private Dictionary<string, ImportActionDelegate> importActionDic;
        private delegate string ImportActionDelegate(ImportBankStmtParamItem paramObj);

        public string Execute(IList<ImportBankStmtParamItem> paramObjs)
        {
            string messageText = string.Empty;
            foreach (ImportBankStmtParamItem paramObj in paramObjs)
            {
                ImportActionDelegate actionCaller = null;
                if (importActionDic.TryGetValue(paramObj.Name, out actionCaller))
                {
                    if (messageText != string.Empty)
                        messageText += "\r\n";
                    messageText += string.Format("{0}: {1} rows processed",
                        paramObj.Name,
                        actionCaller(paramObj));
                }
            }

            return messageText;
        }

        public string ExecuteAnz2(ImportBankStmtParamItem paramObj)
        {
            var fileName = paramObj.FilePath;
            var connection = (SqlConnection)paramObj.Session.Connection;
            using (var reader = new CsvReader(new StreamReader(fileName), false))
            {
                reader.Columns = new List<LumenWorks.Framework.IO.Csv.Column>
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

                var command = connection.CreateCommand();
                command.CommandText = @"CREATE TABLE #TmpBankStmtImport (
    [TranDate] [datetime] NULL,
    [BankAccountNumber] [nvarchar](50) NULL,
    [TranType] [nvarchar](50) NULL,
    [TranRef] [nvarchar](50) NULL,
    [TranAmount] [money] NULL,
    [TranDescription] [nvarchar](255) NULL,
    [TranCode] [nvarchar](10) NULL,
    [Oid] [uniqueidentifier] NULL
    )";
                command.ExecuteNonQuery();

                // Now use SQL Bulk Copy to move the data
                using (var sbc = new SqlBulkCopy(connection))
                {
                    sbc.DestinationTableName = "#TmpBankStmtImport";
                    sbc.BatchSize = 1000;

                    sbc.ColumnMappings.Add("TranDate", "TranDate");
                    sbc.ColumnMappings.Add("BankAccountNumber", "BankAccountNumber");
                    sbc.ColumnMappings.Add("TranType", "TranType");
                    sbc.ColumnMappings.Add("TranRef", "TranRef");
                    sbc.ColumnMappings.Add("TranAmount", "TranAmount");
                    sbc.ColumnMappings.Add("TranDescription", "TranDescription");
                    sbc.ColumnMappings.Add("TranCode", "TranCode");

                    sbc.WriteToServer(reader);
                    
                }
            }
            return string.Empty;
        }

        public string ExecuteAnz(ImportBankStmtParamItem paramObj)
        {
            var session = paramObj.Session;
            using (var loader = new SqlServerLoader2((SqlConnection)session.Connection))
            {

                loader.ColumnMappings.Add("TranDate", "TranDate");
                //loader.ColumnMappings.Add(1, "BankAccountNumber");
                //loader.ColumnMappings.Add(6, "TranType");
                //loader.ColumnMappings.Add(7, "TranRef");
                //loader.ColumnMappings.Add(8, "TranAmount");
                //loader.ColumnMappings.Add(9, "TranDescription");
                //loader.ColumnMappings.Add(12, "TranCode");
                loader.CreateSql = @"CREATE TABLE {TempTable} (
[TranDate] [nvarchar](200) NULL
)";
                /*
                loader.CreateSql = @"CREATE TABLE {TempTable} (
    [TranDate] [datetime] NULL,
    [BankAccountNumber] [nvarchar](50) NULL,
    [TranType] [nvarchar](50) NULL,
    [TranRef] [nvarchar](50) NULL,
    [TranAmount] [money] NULL,
    [TranDescription] [nvarchar](255) NULL,
    [TranCode] [nvarchar](10) NULL,
    [Oid] [uniqueidentifier] NULL
    )";
    */
                //                loader.PersistSql = @"
                //INSERT INTO BankStmt (Oid)
                //SELECT
                //    (SELECT CAST(CAST(NEWID() AS BINARY(10)) + CAST(GETDATE() AS BINARY(6)) AS UNIQUEIDENTIFIER)) AS Oid
                //FROM {TempTable}";
                loader.PersistSql = "SELECT 1" ;
                using (var csvReader = DataObjectFactory.CreateReaderFromCsv(paramObj.FilePath, false))
                {

                    csvReader.Columns = new List<Column>()
                    {
                        new Column() { Name = "TranDate", Type = typeof(DateTime) }
                    };

                    var reader = (IDataReader)csvReader;
                    var ordinal = reader.GetOrdinal("TranDate");
                    var messagesText = loader.Execute(csvReader);
                    return messagesText;
                }
         
            }
        }
    }
}
