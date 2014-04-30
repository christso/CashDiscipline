using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.BusinessObjects
{
    public interface ICalculateToggleObject
    {
        // Whether Real Time calculation is enabled. If not, then calculation will occur on saving.
        bool CalculateEnabled { get; set; }
    }
}
