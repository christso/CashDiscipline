using CashDiscipline.Module.CashDisciplineServiceReference;
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
            CashDisciplineServiceReference.Service1Client client = new
                CashDisciplineServiceReference.Service1Client();

            IntegrationPackageResult result = client.ImportWbcForexRates(inputFilePath);
            return result;
        }
    }
}
