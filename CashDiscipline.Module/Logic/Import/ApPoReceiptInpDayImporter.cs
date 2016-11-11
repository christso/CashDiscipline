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
    public class ApPoReceiptInpDayImporter
    {
        public ApPoReceiptInpDayImporter(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }


        private XPObjectSpace objSpace;

        private string createSql
        {
            get
            {
                return
@"IF OBJECT_ID('tempdb..#TmpApPoMatchDaysInput') IS NOT NULL DROP TABLE #TmpApPoMatchDaysInput
CREATE TABLE #TmpApPoMatchDaysInput
(
    PoNum nvarchar(255),
    Vendor nvarchar(255),
    ForecastVendorMatchDays float,
    ForecastMatchDays float
)";
            }
        }

        private string persistSql
        {
            get
            {
                return
@"DELETE FROM VHAFinance.dbo.ApPoMatchInput
INSERT INTO VHAFinance.dbo.ApPoMatchInput (PoNum, ForecastMatchDays)
SELECT 
    PoNum,
    ForecastMatchDays
FROM #TmpApPoMatchDaysInput";
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
                DataTable xlsDataTable = xlsDataSet.Tables["PoMatchDaysInput"];

                //cmd.Transaction = trn;
                cmd.CommandTimeout = CashDiscipline.Common.Constants.SqlCommandTimeout;

                cmd.CommandText = createSql;
                cmd.ExecuteNonQuery();

                bc.DestinationTableName = "#TmpApPoMatchDaysInput";
                bc.WriteToServer(xlsDataTable);

                cmd.CommandText = "SELECT COUNT(*) FROM #TmpApPoMatchDaysInput";
                rowCount = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.CommandText = persistSql;
                cmd.ExecuteNonQuery();

            }
            statusMessage = string.Format("{0} rows processed.", rowCount);
            return statusMessage;
        }
    }
}
