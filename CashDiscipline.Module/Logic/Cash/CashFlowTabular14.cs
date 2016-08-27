using DevExpress.ExpressApp.Xpo;
using DG2NTT.AnalysisServicesHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Cash
{
    public class CashFlowTabular14 : ICashFlowTabular
    {
        public string LastReturnMessage
        {
            get
            {
                return "";
            }
        }

        private static ServerProcessor CreateSsasClient()
        {
            return CashDiscipline.Module.Clients.CshDscTabularHelper.CreateSsasClient();
        }

        public void ProcessAll()
        {
            var ssas = CreateSsasClient();
            ssas.ProcessDatabase();
        }

        public void ProcessCurrent(XPObjectSpace objSpace)
        {
            var ssas = CreateSsasClient();
            ssas.ProcessPartition("Model", "CashFlow", "CashFlow_Report_Current");
        }

        public void ProcessHist()
        {
            var ssas = CreateSsasClient();
            ssas.ProcessPartition("CashFlow", "CashFlow", "CashFlow_Current_Hist");
        }

        public void ProcessSshot()
        {
            var ssas = CreateSsasClient();
            ssas.ProcessPartition("CashFlow", "CashFlow", "CashFlow_Snapshot");
        }
    }
}
