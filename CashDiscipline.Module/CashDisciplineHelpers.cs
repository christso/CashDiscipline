using DG2NTT.AnalysisServicesHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module
{
    public static class CashDisciplineHelpers
    {
        public static ServerProcessor CreateSsasClient()
        {
            return new ServerProcessor("FINSERV01", "CashFlow");
        }

        public static AdomdProcessor CreateAdomdClient()
        {
            return new AdomdProcessor("FINSERV01");
        }
    }
}
