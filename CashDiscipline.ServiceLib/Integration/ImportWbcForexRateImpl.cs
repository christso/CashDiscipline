using CashDiscipline.Common;
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
 public class ImportWbcForexRateImpl
    {
        private const string SqlConnectionString = Constants.SqlConnectionString;
        private const string catalogName = Constants.SqlDatabase;
        private const string ssisFolderName = Constants.SsisFolderName;
        private const string pkgName = "WbcForexRate.dtsx";

        public ImportWbcForexRateImpl()
        {
            SSISMessagesList = new List<string>();
        }

        public List<string> SSISMessagesList;

        //inputFilePath = @"\\FINSERV01\Downloads\VHA Import\SSIS\Westpac Forex Rates\GLXR160527.txt";
        public void Execute(string inputFilePath)
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
            ssisPackage.Parameters["SourceConnectionString"].Set(ParameterInfo.ParameterValueType.Literal, inputFilePath);
            ssisPackage.Alter();

            // Get the identifier of the execution to get the log
            long executionIdentifier = ssisPackage.Execute(false, null, executionParameter);

            // Loop through the log and add the messages to the listbox
            foreach (OperationMessage message in ssisServer.Catalogs["SSISDB"].Executions[executionIdentifier].Messages)
            {
                SSISMessagesList.Add(message.MessageType.ToString() + ": " + message.Message);
            }
        }

        public string ShowMessageText()
        {
            string messagesText = string.Empty;
            foreach (var message in SSISMessagesList)
            {
                if (messagesText != string.Empty)
                    messagesText += "\r\n";
                messagesText += message;
            }
            messagesText = messagesText.Replace("\r\n\r\n", "\r\n");
            return messagesText;
        }
    }
}
