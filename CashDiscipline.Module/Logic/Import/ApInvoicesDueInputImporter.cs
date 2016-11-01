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
    public class ApInvoicesDueInputImporter
    {
        public ApInvoicesDueInputImporter(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }


        private XPObjectSpace objSpace;

        private string createSql
        {
            get
            {
                return
@"IF OBJECT_ID('tempdb..#TmpApInvoicesDueInput') IS NOT NULL DROP TABLE #TmpApInvoicesDueInput
CREATE TABLE #TmpApInvoicesDueInput
(
    Supplier nvarchar(255),
    InvoiceNumber nvarchar(255),
    InvoiceDueDate nvarchar(255)
)";
            }
        }

        private string persistSql
        {
            get
            {
                return
@"DELETE FROM VHAFinance.dbo.ApInvoicesDueInput
INSERT INTO VHAFinance.dbo.ApInvoicesDueInput
SELECT * FROM #TmpApInvoicesDueInput";
            }
        }

        public string Execute(string inputFilePath)
        {
            var statusMessage = string.Empty;

            var connectionString = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};
Extended Properties = ""Excel 12.0 Xml;HDR=YES""", inputFilePath);
            var adapter = new OleDbDataAdapter("SELECT * FROM [ApInvoicesDueInput$]", connectionString);

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

                bc.DestinationTableName = "#TmpApInvoicesDueInput";
                bc.WriteToServer(csv);

                cmd.CommandText = "SELECT COUNT(*) FROM #TmpApInvoicesDueInput";
                rowCount = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.CommandText = persistSql;
                cmd.ExecuteNonQuery();

            }
            statusMessage = string.Format("{0} rows processed.", rowCount);
            return statusMessage;
        }
    }
}
