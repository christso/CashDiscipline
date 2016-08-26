using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using CashDiscipline.ServiceLib.Types;
using CashDiscipline.ServiceLib.Integration;

namespace CashDiscipline.ServiceLib
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class Service1 : IService1
    {
        public string Ping(string value)
        {
            return string.Format("You entered: {0}", value);
        }

        public IntegrationPackageResult ExecuteSsisPackage(string packageName, SsisParameter[] parameters)
        {
            var client = new SsisPackageClient();
            client.Execute(packageName, parameters);
            return client.PackageResult;
        }
    }
}
