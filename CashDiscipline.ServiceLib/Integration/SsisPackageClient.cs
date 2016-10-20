using CashDiscipline.ServiceLib.Types;
using Microsoft.SqlServer.Management.IntegrationServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.ServiceLib.Integration
{
    public class SsisPackageClient
    {
        private const string SqlConnectionString = CashDiscipline.Common.Constants.SqlConnectionString;
        private const string catalogName = CashDiscipline.Common.Constants.SqlDatabase;
        private const string ssisFolderName = CashDiscipline.Common.Constants.SsisFolderName;

        public List<SsisMessage> SsisMessages;
        public IntegrationPackageResult PackageResult;

        public IntegrationPackageResult Execute(string packageName, SsisParameter[] parameters)
        {
            SqlConnection ssisConnection = new SqlConnection(SqlConnectionString);
            IntegrationServices ssisServer = new IntegrationServices(ssisConnection);
            
            // The reference to the package which you want to execute
            PackageInfo ssisPackage = ssisServer.Catalogs[CashDiscipline.Common.Constants.SsisCatalog].Folders[ssisFolderName].Projects[catalogName].Packages[packageName];
            
            // Add execution parameter to override the default asynchronized execution. If you leave this out the package is executed asynchronized
            Collection<PackageInfo.ExecutionValueParameterSet> executionParameter = new Collection<PackageInfo.ExecutionValueParameterSet>();
            executionParameter.Add(new PackageInfo.ExecutionValueParameterSet { ObjectType = 50, ParameterName = "SYNCHRONIZED", ParameterValue = 1 });

            // Modify package parameter
            foreach (var svcParam in parameters)
            {
                ParameterInfo pInfo = null;
                var pKey = new ParameterInfo.Key(svcParam.ParameterName);
                if (!ssisPackage.Parameters.TryGetValue(pKey, out pInfo))
                    throw new InvalidOperationException(string.Format("Parameter name {0} does not exist in package.", pKey.Name));
                pInfo.Set(ParameterInfo.ParameterValueType.Literal, svcParam.ParameterValue);
            }
            
            ssisPackage.Alter();

            // Get the identifier of the execution to get the log
            long executionIdentifier = ssisPackage.Execute(false, null, executionParameter);
            
            // Loop through the log and add the messages to the listbox
            SsisMessages = new List<SsisMessage>();
            var execution = ssisServer.Catalogs["SSISDB"].Executions[executionIdentifier];
            
            foreach (OperationMessage message in ssisServer.Catalogs["SSISDB"].Executions[executionIdentifier].Messages)
            {
                SsisMessages.Add(new SsisMessage()
                {
                    MessageSourceType = message.MessageSourceType,
                    Message = message.Message,
                    MessageType = message.MessageType
                });
            }
            
            // Update result
            PackageResult = new IntegrationPackageResult();
            PackageResult.SsisMessages = this.SsisMessages;
            PackageResult.ExecutionIdentifer = executionIdentifier;
            PackageResult.PackageName = packageName;
            PackageResult.OperationStatus = (SsisOperationStatus)execution.Status;

            return PackageResult;
        }
    }
}
