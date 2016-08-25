using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CashDiscipline.ServiceLib.Types
{
    [DataContract]
    public class IntegrationPackageResult
    {
        int returnValue = 0;
        string returnMessage = string.Empty;

        [DataMember]
        public int ReturnValue
        {
            get { return returnValue; }
            set { returnValue = value; }
        }

        [DataMember]
        public string ReturnMessage
        {
            get { return returnMessage; }
            set { returnMessage = value; }
        }

        [DataMember]
        public long ExecutionIdentifer { get; set; }

        [DataMember]
        public List<SsisMessage> SsisMessages { get; set; }

    }
}
