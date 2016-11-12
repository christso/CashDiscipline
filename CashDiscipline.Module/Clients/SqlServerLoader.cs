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
    public class SqlServerLoader
    {
        public SqlServerLoader(SqlConnection sqlConn)
        {
            this.sqlConn = sqlConn;
        }
        private readonly SqlConnection sqlConn;
        private string tempTableName;


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

        public string Execute(DataTable sourceTable)
        {
            return Execute((object)sourceTable);
        }

        public string Execute(IDataReader sourceReader)
        {
            return Execute((object)sourceReader);
        }

        private string FormatSql(string commandText)
        {
            return Smart.Format(commandText, new { TempTable = tempTableName });
        }

        private string Execute(object source)
        {
            tempTableName = "#tmp_" + Guid.NewGuid().ToString("N");

            int rowCount = 0;
            var statusMessage = string.Empty;

            using (var cmd = sqlConn.CreateCommand())
            using (var bc = new SqlBulkCopy(sqlConn))
            {
                cmd.CommandTimeout = CashDiscipline.Common.Constants.SqlCommandTimeout;

                cmd.CommandText = FormatSql(
                    "IF OBJECT_ID('tempdb..{TempTable}') IS NOT NULL DROP TABLE {TempTable}\r\n"
                    + createSql);
                cmd.ExecuteNonQuery();

                bc.DestinationTableName = tempTableName;

                #region write source to destination
                if (source is IDataReader)
                    bc.WriteToServer((IDataReader)source);
                else if (source is DataTable)
                    bc.WriteToServer((DataTable)source);
                else
                    throw new InvalidOperationException(string.Format(
                        "Source must be of type IDataReader or DataTable. Type is {0}", source.GetType()));
                #endregion

                cmd.CommandText = FormatSql("SELECT COUNT(*) FROM {TempTable}");
                rowCount = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.CommandText = FormatSql(persistSql);
                cmd.ExecuteNonQuery();
            }

            statusMessage = string.Format("{0} rows processed.", rowCount);
            return statusMessage;
        }

    }
}