using CashDiscipline.Module.ParamObjects.Import;
using Microsoft.SqlServer.Management.IntegrationServices;
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
        private const string SqlConnectionString = Constants.SqlConnectionString;
        private const string catalogName = Constants.SqlDatabase;
        private const string ssisFolderName = Constants.SsisFolderName;
        private const string pkgName = "BankStmt.dtsx";

        public BankStmtImporter()
        {
            SSISMessagesList = new List<string>();
        }


        public List<string> SSISMessagesList;

        public void Execute(ImportBankStmtParam paramObj)
        {
            SSISMessagesList.Clear();

            SqlConnection ssisConnection = new SqlConnection(SqlConnectionString);
            IntegrationServices ssisServer = new IntegrationServices(ssisConnection);

            // The reference to the package which you want to execute
            PackageInfo ssisPackage = ssisServer.Catalogs["SSISDB"].Folders[ssisFolderName].Projects[catalogName].Packages[pkgName];

            // Add execution parameter to override the default asynchronized execution. If you leave this out the package is executed asynchronized
            Collection<PackageInfo.ExecutionValueParameterSet> executionParameter = new Collection<PackageInfo.ExecutionValueParameterSet>();
            executionParameter.Add(new PackageInfo.ExecutionValueParameterSet { ObjectType = 50, ParameterName = "SYNCHRONIZED", ParameterValue = 1 });

            // Modify package parameter
            ssisPackage.Parameters["AnzSourceConnectionString"].Set(ParameterInfo.ParameterValueType.Literal, paramObj.AnzFilePath ?? "");
            ssisPackage.Parameters["AnzDisabled"].Set(ParameterInfo.ParameterValueType.Literal, !paramObj.AnzEnabled);
            ssisPackage.Parameters["WbcSourceConnectionString"].Set(ParameterInfo.ParameterValueType.Literal, paramObj.WbcFilePath ?? "");
            ssisPackage.Parameters["WbcDisabled"].Set(ParameterInfo.ParameterValueType.Literal, !paramObj.WbcEnabled);
            ssisPackage.Parameters["CbaOpSourceConnectionString"].Set(ParameterInfo.ParameterValueType.Literal, paramObj.CbaOpFilePath ?? "");
            ssisPackage.Parameters["CbaOpDisabled"].Set(ParameterInfo.ParameterValueType.Literal, !paramObj.CbaOpEnabled);
            ssisPackage.Parameters["CbaBosSourceConnectionString"].Set(ParameterInfo.ParameterValueType.Literal, paramObj.CbaBosFilePath ?? "");
            ssisPackage.Parameters["CbaBosDisabled"].Set(ParameterInfo.ParameterValueType.Literal, !paramObj.CbaBosEnabled);
            ssisPackage.Alter();

            // Get the identifier of the execution to get the log
            long executionIdentifier = ssisPackage.Execute(false, null, executionParameter);

            // Loop through the log and add the messages to the listbox
            foreach (OperationMessage message in ssisServer.Catalogs["SSISDB"].Executions[executionIdentifier].Messages)
            {
                SSISMessagesList.Add(message.MessageType.ToString() + ": " + message.Message);
            }
        }
    }
}
