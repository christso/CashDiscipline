using DevExpress.ExpressApp.Xpo;
using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Import
{
    public class ApInvoiceBalanceImporter
    {

        public ApInvoiceBalanceImporter(XPObjectSpace objSpace)
        {
            this.objSpace = objSpace;
        }


        #region Constants
        private string createSql
        {
            get
            {
                return @"IF OBJECT_ID('tempdb..#TmpApInvoiceBal') IS NOT NULL DROP TABLE #TmpApInvoiceBal
CREATE TABLE #TmpApInvoiceBal (
[Request id] int,
[As at Date] date,
[Supplier] nvarchar(255),
[Vendor Type] nvarchar(255),
[Category1] nvarchar(255),
[Category2] nvarchar(255),
[Invoice Number] nvarchar(255),
[Liability Company] nvarchar(10),
[Liability Account] nvarchar(10),
[Liability Cost Centre] nvarchar(10),
[Liability Intercompany] nvarchar(10),
[Invoice date] date,
[GL date] date,
[Trx currency code] nvarchar(10),
[Invoice Amount AUD] float,
[Invoice Remaining Amount AUD] float,
[Line Distribution Remaining Amount AUD] float,
[Opex  Capex  Other] nvarchar(255),
[Extra detail (Opex  Exec  Capex  Delivery Area)] nvarchar(255),
[Extra detail2 (Opex  GM  Capex  Project Number)] nvarchar(255),
[Expense Company] nvarchar(10),
[Expense Account] nvarchar(10),
[Expense Cost Centre] nvarchar(10),
[Expense Product] nvarchar(10),
[Expense Sales Channel] nvarchar(10),
[Expense Country] nvarchar(10),
[Expense Intercompany] nvarchar(10),
[Expense Project] nvarchar(10),
[Expense Location] nvarchar(10),
[Invoice Description] nvarchar(255),
[Due Date] date,
[Payment Term] nvarchar(50),
[Entered rounded orig amount] float,
[Entered rounded rem amount] float
)";
            }
        }

        private string persistSql
        {
            get
            {
                return
@"INSERT INTO VHAFinance.dbo.ApTradeCreditor
(
    [Oid],
    [RequestId],
    [AsAtDate],
    [Supplier],
    [VendorType],
    [Category1],
    [Category2],
    [InvoiceNumber],
    [LiabilityCompany],
    [LiabilityAccount],
    [LiabilityCostCentre],
    [LiabilityIntercompany],
    [InvoiceDate],
    [GlDate],
    [TrxCurrencyCode],
    [InvoiceAmountAud],
    [InvoiceRemainingAmountAud],
    [LineDistributionRemainingAmountAud],
    [Opex_Capex_Other],
    [ExtraDetail1],
    [ExtraDetail2],
    [ExpenseCompany],
    [ExpenseAccount],
    [ExpenseCostCentre],
    [ExpenseProduct],
    [ExpenseSalesChannel],
    [ExpenseCountry],
    [ExpenseIntercompany],
    [ExpenseProject],
    [ExpenseLocation],
    [InvoiceDescription],
    [DueDate],
    [PaymentTerm],
    [EnteredOrigAmount],
    [EnteredOrigRemainingAmount]
)
SELECT
    NEWID() AS Oid,
    [Request id],
    [As at Date],
    [Supplier],
    [Vendor Type],
    [Category1],
    [Category2],
    [Invoice Number],
    [Liability Company],
    [Liability Account],
    [Liability Cost Centre],
    [Liability Intercompany],
    [Invoice date],
    [GL date],
    [Trx currency code],
    [Invoice Amount AUD],
    [Invoice Remaining Amount AUD],
    [Line Distribution Remaining Amount AUD],
    [Opex  Capex  Other],
    [Extra detail (Opex  Exec  Capex  Delivery Area)],
    [Extra detail2 (Opex  GM  Capex  Project Number)],
    [Expense Company],
    [Expense Account],
    [Expense Cost Centre],
    [Expense Product],
    [Expense Sales Channel],
    [Expense Country],
    [Expense Intercompany],
    [Expense Project],
    [Expense Location],
    [Invoice Description],
    [Due Date],
    [Payment Term],
    [Entered rounded orig amount],
    [Entered rounded rem amount]
FROM #TmpApInvoiceBal
";
            }
        }

        #endregion

        private XPObjectSpace objSpace;

        public string Execute(string inputFilePath)
        {
            var statusMessage = string.Empty;

            var conn = (SqlConnection)objSpace.Connection;
  
            int rowCount = 0;

            using (var csv = new CachedCsvReader(new StreamReader(inputFilePath), true))
            using (var cmd = conn.CreateCommand())
            using (var bc = new SqlBulkCopy(conn))
            {
                //cmd.Transaction = trn;
                cmd.CommandTimeout = CashDiscipline.Common.Constants.SqlCommandTimeout;

                cmd.CommandText = createSql;
                cmd.ExecuteNonQuery();

                bc.DestinationTableName = "#TmpApInvoiceBal";
                bc.WriteToServer(csv);

                cmd.CommandText = "SELECT COUNT(*) FROM #TmpApInvoiceBal";
                rowCount = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.CommandText = persistSql;
                cmd.ExecuteNonQuery();

            }
            statusMessage = string.Format("{0} rows processed.", rowCount);
            return statusMessage;
        }
    }
}
