using CashDiscipline.Module.ParamObjects.Import;
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
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Import
{
    public class ApPoReceiptInpDateImporter
    {
        public ApPoReceiptInpDateImporter(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }


        private XPObjectSpace objSpace;

        private string createSql
        {
            get
            {
                return
@"IF OBJECT_ID('tempdb..#TmpApPoMatchDateInput') IS NOT NULL DROP TABLE #TmpApPoMatchDateInput
CREATE TABLE #TmpApPoMatchDateInput
(
    PoNum nvarchar(255),
    ForecastMatchDate date
)";
            }
        }

        private string persistSql
        {
            get
            {
                return
@"DELETE FROM VHAFinance.dbo.ApPoMatchDateInput
INSERT INTO VHAFinance.dbo.ApPoMatchDateInput (PoNum, ForecastMatchDate)
SELECT PoNum, ForecastMatchDate FROM #TmpApPoMatchDateInput";
            }
        }

        public string Execute(string inputFilePath)
        {
            var statusMessage = string.Empty;
            
            var conn = (SqlConnection)objSpace.Connection;
            int rowCount = 0;

            using (FileStream stream = File.Open(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var cmd = conn.CreateCommand())
            using (var bc = new SqlBulkCopy(conn))
            {
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                excelReader.IsFirstRowAsColumnNames = true;
                DataSet xlsDataSet = excelReader.AsDataSet();
                excelReader.Close();
                DataTable xlsDataTable = xlsDataSet.Tables["ManualMatchDate"];

                cmd.CommandTimeout = CashDiscipline.Common.Constants.SqlCommandTimeout;

                cmd.CommandText = createSql;
                cmd.ExecuteNonQuery();

                bc.DestinationTableName = "#TmpApPoMatchDateInput";
                bc.WriteToServer(xlsDataTable);

                cmd.CommandText = "SELECT COUNT(*) FROM #TmpApPoMatchDateInput";
                rowCount = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.CommandText = persistSql;
                cmd.ExecuteNonQuery();

            }
            statusMessage = string.Format("{0} rows processed.", rowCount);
            return statusMessage;
        }
    }
}
