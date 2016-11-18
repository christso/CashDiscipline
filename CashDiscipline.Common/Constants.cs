using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Common
{
    public class Constants
    {
        // Server
        public static bool MsasTabularCompatibility_13 = true;
        public static int SqlCommandTimeout = 999999;

        // Views
        public const string CashFlowListViewId = "CashFlow_ListView";
        public const string CashFlowAllListViewId = "CashFlowAll_ListView";

        // Contexts
        public const string AcceptActionContext = "AcceptAction";

        // Fix Tags
        public const string ReversalFixTag = "R";
        public const string RevRecFixTag = "RR";
        public const string ResRevRecFixTag = "RRR";
        public const string PayrollFixTag = "PR";
        public const string AutoFixTag = "Auto";
        public const string BankFeeFixTag = "BFEE";
        public const string ProgenFixTag = "PRGN";
        public const string ScheduleOutFixTag = "S";
        public const string AllocateFixTag = "A";
        public const string ScheduleInFixTag = "C";
        public const string TaxFixTag = "TAX";

        // Defaults
        public const string DefaultFixCounterparty = "UNDEFINED";

        // SSAS
        public const string SsasServerName = "vvsw31414";
        public const string SsasDatabase = "CashFlow";
        public const string SsasModel = "Model";
        public const string SsasSnapshotPartition = "CashFlow_Snapshot";
        public const string SsasSnapshotReported = "SnapshotReported";
        public const string SsasSnapshot = "Snapshot";
        public const string SqlConnectionString = @"Data Source=vvsw31414;Initial Catalog=CashDiscipline;Integrated Security=SSPI;";
        public const string SqlDatabase = "CashDiscipline";
        public const string SsisFolderName = "CashFlow";
        public const string SsisCatalog = "SSISDB";

        // Resources
        public const string CashDiscSqlInstallScriptPath = @"CashDiscipline.Module.Scripts.InstallClr.sql";

        // Connection strings

        public const string FinanceConnString = "Data Source=vvsw31414;Database=VHAFinance;Integrated Security=true;";
    }
}
