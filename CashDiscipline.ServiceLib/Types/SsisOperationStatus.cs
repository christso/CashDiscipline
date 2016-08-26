using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.ServiceLib.Types
{
    public enum SsisOperationStatus
    {
        Created = 1,
        Running = 2,
        Canceled = 3,
        Failed = 4,
        Pending = 5,
        UnexpectTerminated = 6,
        Success = 7,
        Stopping = 8,
        Completion = 9
    }
}
