using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using CashDiscipline.ServiceLib;
using System.Configuration;

namespace CashDiscipline.ServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {

            // read settings
            var appSettings = ConfigurationManager.AppSettings;
            
            string baseAddressString = appSettings["baseAddress"];

            Uri baseAddress = baseAddressString == null ?
                new Uri("http://localhost:80/Temporary_Listen_Addresses/CashDiscipline") :
                new Uri(baseAddressString);

            using (System.ServiceModel.ServiceHost host = new System.ServiceModel.ServiceHost(typeof(Service1), baseAddress))
            {
                // Enable metadata publishing.
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(smb);

                // enable debugging
                ServiceBehaviorAttribute sba = (ServiceBehaviorAttribute)host.Description.Behaviors[typeof(ServiceBehaviorAttribute)];
                sba.IncludeExceptionDetailInFaults = true;

                // Open the ServiceHost to start listening for messages. Since
                // no endpoints are explicitly configured, the runtime will create
                // one endpoint per base address for each service contract implemented
                // by the service.
                host.Open();

                Console.WriteLine("The service is ready at {0}", baseAddress);
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();

                // Close the ServiceHost.
                host.Close();
            }
        }

    }
}
