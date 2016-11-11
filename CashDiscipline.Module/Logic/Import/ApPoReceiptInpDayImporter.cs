using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp.Xpo;
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

            var connectionString = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};
Extended Properties = ""Excel 12.0 Xml;HDR=YES""", inputFilePath);
            var adapter = new OleDbDataAdapter("SELECT * FROM [PoMatchDaysInput$]", connectionString);

            var ds = new DataSet();
            adapter.Fill(ds, "anyNameHere");
            DataTable data = ds.Tables["anyNameHere"];

            var conn = (SqlConnection)objSpace.Connection;
            int rowCount = 0;

            using (var csv = data.CreateDataReader())
            using (var cmd = conn.CreateCommand())
            using (var bc = new SqlBulkCopy(conn))
            {
                //cmd.Transaction = trn;
                cmd.CommandTimeout = CashDiscipline.Common.Constants.SqlCommandTimeout;

                cmd.CommandText = createSql;
                cmd.ExecuteNonQuery();

                bc.DestinationTableName = "#TmpApPoMatchDaysInput";
                bc.WriteToServer(csv);

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
