using CashDiscipline.Common;
using CashDiscipline.Module.Clients;
using DG2NTT.AnalysisServicesHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADOMD = Microsoft.AnalysisServices.AdomdClient;

namespace CashDiscipline.Module.Logic.Cash
{
    public class CashFlowTabular16 : ICashFlowTabular
    {
        private static AdomdProcessor CreateAdomdClient()
        {
            return CshDscTabularHelper.CreateAdomdClient();
        }

        public void ProcessAll()
        {
            var ssas = CreateAdomdClient();
            ssas.ProcessCommand(@"{
  ""refresh"": {
    ""type"": ""full"",
    ""objects"": [
      {
        ""database"": ""CashFlow""
      }
    ]
  }
}");
        }

        public void ProcessCurrent()
        {
            var dax = new DaxClient();
            string connectionString = AppConfig.SsasConnectionString;
            var cnn = new ADOMD.AdomdConnection(connectionString);

            var ssas = CreateAdomdClient();
            ssas.ProcessCommand(@"{
  ""refresh"": {
    ""type"": ""full"",
    ""objects"": [
      {
        ""database"": ""CashFlow"",
        ""table"": ""Snapshot""
      },
      {
        ""database"": ""CashFlow"",
        ""table"": ""SnapshotReported""
      },
      {
        ""database"": ""CashFlow"",
        ""table"": ""CashFlow"",
        ""partition"": ""CashFlow_Report_Current""
      }
    ]
  }
}");
        }

        public void ProcessHist()
        {
            var ssas = CreateAdomdClient();
            ssas.ProcessCommand(@"{
  ""refresh"": {
    ""type"": ""full"",
    ""objects"": [
      {
        ""database"": ""CashFlow"",
        ""table"": ""CashFlow"",
        ""partition"": ""CashFlow_Current_Hist""
      }
    ]
  }");
        }

        public void ProcessSshot()
        {
            var ssas = CreateAdomdClient();
            ssas.ProcessCommand(@"{
  ""refresh"": {
    ""type"": ""full"",
    ""objects"": [
      {
        ""database"": ""CashFlow"",
        ""table"": ""CashFlow"",
        ""partition"": ""CashFlow_Snapshot""
      }
    ]
  }");
        }
    }
}
