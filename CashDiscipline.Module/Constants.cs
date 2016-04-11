using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module
{
    public class Constants
    {
   
        // Views
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
