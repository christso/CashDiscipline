using CashDiscipline.Common;
using DG2NTT.AnalysisServicesHelpers;
using Microsoft.AnalysisServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADOMD = Microsoft.AnalysisServices.AdomdClient;
using SSAS = Microsoft.AnalysisServices;

namespace CashDiscipline.Module.Clients
{
    public class CshDscTabularHelper
    {
        private const string ServerName = Constants.SsasServerName;
        private const string DatabaseName = Constants.SsasDatabase;

        public static ServerProcessor CreateSsasClient()
        {
            return new ServerProcessor(ServerName, DatabaseName);
        }

        public static AdomdProcessor CreateAdomdClient()
        {
            return new AdomdProcessor(ServerName);
        }
    }
}
