using CashDiscipline.Module.ServiceReference1;
using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Forex
{
    public class WbcForexRateImporter
    {
        public IntegrationPackageResult Execute(string inputFilePath)
        {
            Service1Client client = new Service1Client();

            var parameters = new SsisParameter[] {
                new SsisParameter() { ParameterName = "SourceConnectionString", ParameterValue = inputFilePath ?? "" }
            };

            IntegrationPackageResult result = client.ExecuteSsisPackage("WbcForexRate.dtsx", parameters);
            
            return result;
        }
    }
}
