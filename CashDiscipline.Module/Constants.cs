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

        // Fix Tags
        public const string ReversalFixTag = "R";
        public const string RevRecFixTag = "RR";
        public const string ResRevRecFixTag = "RRR";
        public const string PayrollFixTag = "PR";
        public const string BankFeeFixTag = "BFEE";
        public const string ProgenFixTag = "PRGN";
        public const string ScheduleOutFixTag = "S";
        public const string AllocateFixTag = "A";
        public const string ScheduleInFixTag = "C";
        public const string TaxFixTag = "TAX";

        // Defaults
        public const string DefaultFixCounterparty = "UNDEFINED";
    }
}
