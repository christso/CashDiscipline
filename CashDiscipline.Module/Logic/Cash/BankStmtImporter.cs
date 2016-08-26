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
    public class BankStmtImporter
    {
        public List<IntegrationPackageResult> Execute(IList<ImportBankStmtParamItem> paramObjs)
        {
            Service1Client client = new Service1Client();
            var result = new List<IntegrationPackageResult>();

            foreach (var paramObj in paramObjs)
            {
                if (paramObj.Enabled)
                {
                    var parameters = new SsisParameter[]
                    {
                        new SsisParameter() { ParameterName = "ChildPackageName", ParameterValue = paramObj.PackageName },
                        new SsisParameter() { ParameterName = "SourceConnectionString", ParameterValue = paramObj.FilePath },
                        new SsisParameter() { ParameterName = "Account",
                            ParameterValue = paramObj.Account == null? "" : paramObj.Account.Name }
                    };

                    IntegrationPackageResult childResult = client.ExecuteSsisPackage("BankStmtFile.dtsx", parameters);
                    result.Add(childResult);
                }
            }

            return result;
        }
    }
}
