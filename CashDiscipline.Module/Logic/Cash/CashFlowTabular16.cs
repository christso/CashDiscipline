using CashDiscipline.Common;
using CashDiscipline.Module.Clients;
using DevExpress.ExpressApp.Xpo;
using DG2NTT.AnalysisServicesHelpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        private string _LastReturnMessage;

        public string LastReturnMessage
        {
            get { return _LastReturnMessage; }
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

        public void ProcessCurrent(XPObjectSpace objSpace)
        {
            var prevSshot = CashFlowHelper.GetPreviousSnapshot(objSpace);
            if (prevSshot == null)
            {
                ProcessCurrentWithoutPrev();
                _LastReturnMessage = "Processed without previous snapshot is defined in XPO.";
                return;
            }

            var dax = new DaxClient();
            string connectionString = AppConfig.SsasConnectionString;
            var cnn = new ADOMD.AdomdConnection(connectionString);
            cnn.Open();
            var reader = dax.ExecuteReader(@"EVALUATE
ROW
(
	""Result"",
	CALCULATE (
		COUNTROWS( CashFlow ),
		CashFlow[Snapshot OID] = ""{OID}""
	)
)".Replace("{OID}", "{" + prevSshot.Oid + "}"), cnn);
            reader.Read();
            var prevRowCount = reader.GetValue(0);

            if (prevRowCount == null)
            {
                ProcessCurrentWithPrev();
                _LastReturnMessage = "Processed previous snapshot as it currently does not exist in SSAS.";
            }
            else
            {
                ProcessCurrentWithoutPrev();
                _LastReturnMessage = "Did not process previous snapshot because previous snapshot already exists in SSAS.";
            }
        }

        public void ProcessCurrentWithPrev()
        {
            var ssas = CreateAdomdClient();

            // include previous snapshot in processing
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
      },
      {
        ""database"": ""CashFlow"",
        ""table"": ""CashFlow"",
        ""partition"": ""CashFlow_Report_Previous""
      }
    ]
  }
}");

        }

        public void ProcessCurrentWithoutPrev()
        {
            var ssas = CreateAdomdClient();

            // exclude previous snapshot in processing
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
