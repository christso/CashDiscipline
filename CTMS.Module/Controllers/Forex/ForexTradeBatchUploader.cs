using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Controllers.Forex
{
    public interface ForexTradeBatchUploader
    {
        void UploadToCashFlowForecast();
    }
}
