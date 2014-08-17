using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace CTMS.Module.ParamObjects.Setup
{
    [NonPersistent]
    public class CreateDateDimParam
    {
        // Fields...
        private DateTime _FromDate;
        private DateTime _ToDate;
        
        public DateTime FromDate {
            get
            {
                return _FromDate;
            }
            set
            {
                _FromDate = value;
            }
        }
        
        public DateTime ToDate
        {
            get
            {
                return _ToDate;
            }
            set
            {
                _ToDate = value;
            }
        }
    }
}
