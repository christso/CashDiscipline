using CashDiscipline.Module.CashDisciplineServiceReference;
using CashDiscipline.Module.ParamObjects.Import;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Logic.Cash
{
    public class ApPmtDistnImporter
    {
        public IntegrationPackageResult Execute(string inputFilePath)
        {
            Service1Client client = new Service1Client();

            var parameters = new SsisParameter[] {
                new SsisParameter() { ParameterName = "SourceConnectionString", ParameterValue = inputFilePath ?? "" }
            };

            IntegrationPackageResult result = client.ExecuteSsisPackage("ApPmtDistn.dtsx", parameters);

            return result;
        }
    }
}
