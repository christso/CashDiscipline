using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module
{
    public class Constants
    {
        // Customer Type
        public const string c3Sub = "3 Banking";
        public const string cPaging = "3 Messaging";
        public const string cVSub = "Voda AR";
        public const string cSundryDebtor = "Sundry AR";
        public const string cRetail = "Retail";

        // Activity
        public const string c3Gateway = "3 Gateway";

        // GL Code
        public const string cTransferClearing = "Transfer Clearing";
        public const string cVDebtors = "V Debtors";
        public const string cAnzOp = "ANZ Operating";

        // Views
        public const string ArtfFromReceiptLookupListViewId = "ArtfFromReceipt_LookupListView";
        public const string ArtfToReceiptLookupListViewId = "ArtfToReceipt_LookupListView";
        public const string ArtfReconDetailViewId = "CTMS.Module.BusinessObjects.Artf.ArtfRecon_DetailView";
        public const string CashFlowListViewId = "CashFlow_ListView";
        public const string CashFlowAllListViewId = "CashFlowAll_ListView";

        // Contexts
        public const string AcceptActionContext = "AcceptAction";

        // Cash Flow Pivot Layout
        public const string CashFlowPivotLayoutDaily = "Daily";
        public const string CashFlowPivotLayoutWeekly = "Weekly";
        public const string CashFlowPivotLayoutMonthly = "Monthly";
        public const string CashFlowPivotLayoutMonthlyVariance = "MonthlyVariance";
        public const string CashFlowPivotLayoutFixForecast = "FixForecast";
    }
}
