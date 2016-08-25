using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.ServiceLib.Types
{
    [DataContract]
    public class SsisParameter
    {
        [DataMember]
        public string ParameterName { get; set; }
        [DataMember]
        public string ParameterValue { get; set; }
    }
}
