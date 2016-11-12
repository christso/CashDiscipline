using CashDiscipline.Module.ParamObjects.Import;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using LumenWorks.Framework.IO.Csv;
using SmartFormat;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Import
{
    public class VendorSitePaymentTermsImporter
    {
        public VendorSitePaymentTermsImporter(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }

        #region Constants
        private string createSql
        {
            get
            {
                return @"IF OBJECT_ID('tempdb..#TmpVendorSitePaymentTerms') IS NOT NULL DROP TABLE #TmpVendorSitePaymentTerms
CREATE TABLE #TmpVendorSitePaymentTerms (
[Org] nvarchar(255),
[Vendor id] nvarchar(255),
[Vendor site id] nvarchar(255),
[Vendor name] nvarchar(255),
[Vendor site code] nvarchar(255),
[Vendor Site Payment Terms] nvarchar(255),
[Last update date] nvarchar(255),
[Vendor type lookup code] nvarchar(255),
[Vendor End date active] nvarchar(255),
[Site Inactive date] nvarchar(255),
[Vendor Creation date] nvarchar(255),
[Vendor Number] nvarchar(255),
[Vendor Payment Terms] nvarchar(255),
[Creation date] nvarchar(255),
[Vat registration num] nvarchar(255),
[State] nvarchar(255),
[Country] nvarchar(255),
[Address line1] nvarchar(255),
[Address lines alt] nvarchar(255),
[Address line2] nvarchar(255),
[Address line3] nvarchar(255),
[City] nvarchar(255),
[Zip] nvarchar(255),
[Vat code] nvarchar(255)
)";
            }
        }

        private string persistSql
        {
            get
            {
                return
@"DELETE FROM VHAFinance.dbo.VendorSitePaymentTerms
WHERE [As At Date] = '{AsAtDate}'

INSERT INTO VHAFinance.dbo.VendorSitePaymentTerms
(
    [Oid],
    [As At Date],
    [Org],
    [Vendor id],
    [Vendor site id],
    [Vendor name],
    [Vendor site code],
    [Vendor Site Payment Terms],
    [Last update date],
    [Vendor type lookup code],
    [Vendor End date active],
    [Site Inactive date],
    [Vendor Creation date],
    [Vendor Number],
    [Vendor Payment Terms],
    [Creation date],
    [Vat registration num],
    [State],
    [Country],
    [Address line1],
    [Address lines alt],
    [Address line2],
    [Address line3],
    [City],
    [Zip],
    [Vat code]
)
SELECT
    NEWID() AS Oid,
    '{AsAtDate}' AS [As At Date],
    [Org],
    TRY_CAST([Vendor id] AS int),
    TRY_CAST([Vendor site id] AS int),
    [Vendor name],
    [Vendor site code],
    [Vendor Site Payment Terms],
    TRY_CAST([Last update date] AS date),
    [Vendor type lookup code],
    TRY_CAST([Vendor End date active] AS date),
    TRY_CAST([Site Inactive date] AS date),
    TRY_CAST([Vendor Creation date] AS date),
    [Vendor Number],
    [Vendor Payment Terms],
    TRY_CAST([Creation date] AS date),
    [Vat registration num],
    [State],
    [Country],
    [Address line1],
    [Address lines alt],
    [Address line2],
    [Address line3],
    [City],
    [Zip],
    [Vat code]
FROM #TmpVendorSitePaymentTerms
";
            }
        }

        #endregion

        private XPObjectSpace objSpace;

        public string Execute(ImportVendorSitePaymentTermsParam paramObj)
        {
            var statusMessage = string.Empty;

            Func<string, string> formatSql = delegate (string sql)
            {
                return Smart.Format(sql, new
                {
                    AsAtDate = string.Format("{0:yyyy-MM-dd}", paramObj.AsAtDate.Date),
                });
            };

            var conn = (SqlConnection)objSpace.Connection;

            int rowCount = 0;

            using (var csv = new CachedCsvReader(new StreamReader(paramObj.FilePath), true))
            using (var cmd = conn.CreateCommand())
            using (var bc = new SqlBulkCopy(conn))
            {
                //cmd.Transaction = trn;
                cmd.CommandTimeout = CashDiscipline.Common.Constants.SqlCommandTimeout;

                cmd.CommandText = createSql;
                cmd.ExecuteNonQuery();

                bc.DestinationTableName = "#TmpVendorSitePaymentTerms";
                bc.WriteToServer(csv);

                cmd.CommandText = "SELECT COUNT(*) FROM #TmpVendorSitePaymentTerms";
                rowCount = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.CommandText = formatSql(persistSql);
                cmd.ExecuteNonQuery();

            }
            statusMessage = string.Format("{0} rows processed.", rowCount);
            return statusMessage;
        }
    }
}
