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
            CashDisciplineServiceReference.Service1Client client = new
                CashDisciplineServiceReference.Service1Client();

            IntegrationPackageResult result = client.ImportApPmtDistn(inputFilePath);
            return result;
        }
    }
}
