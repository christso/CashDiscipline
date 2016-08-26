using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CashDiscipline.ServiceLib.Types
{
    [DataContract]
    public class IntegrationPackageResult
    {
        [DataMember]
        public string PackageName { get; set; }

        [DataMember]
        public long ExecutionIdentifer { get; set; }

        [DataMember]
        public List<SsisMessage> SsisMessages { get; set; }

        [DataMember]
        public SsisOperationStatus OperationStatus { get; set; }
    }
}
