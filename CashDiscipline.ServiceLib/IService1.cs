using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using CashDiscipline.ServiceLib.Types;

namespace CashDiscipline.ServiceLib
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        string GetData(string value);

        [OperationContract]
        IntegrationPackageResult ExecuteSsisPackage(string packageName, List<SsisParameter> parameters);

        [OperationContract]
        CashDiscipline.ServiceLib.Types.IntegrationPackageResult ImportWbcForexRates(string filePath);

        [OperationContract]
        CashDiscipline.ServiceLib.Types.IntegrationPackageResult ImportBankStmt(CashDiscipline.ServiceLib.Types.ImportBankStmtServiceParam paramObj);

        [OperationContract]
        CashDiscipline.ServiceLib.Types.IntegrationPackageResult ImportApPmtDistn(string filePath);
    }
}
