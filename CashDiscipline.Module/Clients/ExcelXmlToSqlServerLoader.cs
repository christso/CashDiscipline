using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Xpo;
using Excel;
using LumenWorks.Framework.IO.Csv;
using SmartFormat;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace CashDiscipline.Module.Clients
{
    public class ExcelXmlToSqlServerLoader
    {
        public ExcelXmlToSqlServerLoader(SqlConnection sqlConn)
        {
            this.sqlConn = sqlConn;
        }

        private readonly SqlConnection sqlConn;

        private string createSql;
        public string CreateSql
        {
            get { return this.createSql; }
            set { this.createSql = value; }
        }

        private string persistSql;
        public string PersistSql
        {
            get { return this.persistSql; }
            set { this.persistSql = value; }
        }

        private string excelFilePath;
        public string ExcelFilePath
        {
            get { return this.excelFilePath; }
            set { this.excelFilePath = value; }
        }

        private string excelSheetName;
        public string ExcelSheetName
        {
            get { return this.excelSheetName; }
            set { this.excelSheetName = value; }
        }

        public string Execute()
        {
            Guard.ArgumentNotNull(excelFilePath, "Excel File Path");
            Guard.ArgumentNotNull(excelSheetName, "Excel Sheet Name");
            Guard.ArgumentNotNull(createSql, "Create SQL");
            Guard.ArgumentNotNull(persistSql, "Persist SQL");

            string tempTableName = "#tmp_" + Guid.NewGuid().ToString("N");

            int rowCount = 0;
            var statusMessage = string.Empty;

            using (FileStream stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var cmd = sqlConn.CreateCommand())
            using (var bc = new SqlBulkCopy(sqlConn))
            {
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                excelReader.IsFirstRowAsColumnNames = true;
                DataSet xlsDataSet = excelReader.AsDataSet();
                excelReader.Close();

                DataTable xlsDataTable = xlsDataSet.Tables[excelSheetName];

                cmd.CommandTimeout = CashDiscipline.Common.Constants.SqlCommandTimeout;

                cmd.CommandText = createSql;
                cmd.ExecuteNonQuery();

                bc.DestinationTableName = tempTableName;
                bc.WriteToServer(xlsDataTable);

                cmd.CommandText = Smart.Format("SELECT COUNT(*) FROM {TempTable}", 
                    new { TempTable = tempTableName });
                rowCount = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.CommandText = Smart.Format(persistSql, 
                    new { TempTable = tempTableName });
                cmd.ExecuteNonQuery();
            }

            statusMessage = string.Format("{0} rows processed.", rowCount);
            return statusMessage;
        }
    }
}
