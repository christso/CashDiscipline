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
        public IntegrationPackageResult Execute(ImportBankStmtServiceParam svcParam)
        {
            CashDisciplineServiceReference.Service1Client client = new
                CashDisciplineServiceReference.Service1Client();

            IntegrationPackageResult result = client.ImportBankStmt(svcParam);
            return result;
        }

        public IntegrationPackageResult Execute(ImportBankStmtParam paramObj)
        {
            var svcParam = MapParamObj(paramObj);
            return Execute(svcParam);
        }

        public ImportBankStmtServiceParam MapParamObj(ImportBankStmtParam paramObj)
        {
            var svcParam = new ImportBankStmtServiceParam();
            svcParam.AnzFilePath = paramObj.AnzFilePath;
            svcParam.AnzEnabled = paramObj.AnzEnabled;
            svcParam.WbcFilePath = paramObj.WbcFilePath;
            svcParam.WbcEnabled = paramObj.WbcEnabled;
            svcParam.CbaBosFilePath = paramObj.CbaBosFilePath;
            svcParam.CbaBosEnabled = paramObj.CbaBosEnabled;
            svcParam.CbaOpFilePath = paramObj.CbaOpFilePath;
            svcParam.CbaOpEnabled = paramObj.CbaOpEnabled;
            return svcParam;
        }
    }
}
