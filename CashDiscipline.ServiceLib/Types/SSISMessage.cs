using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.ServiceLib.Types
{
    [DataContract]
    public class SsisMessage
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public short? MessageType { get; set; }

        [DataMember]
        public short? MessageSourceType { get; set; }
    }
}
