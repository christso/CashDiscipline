using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelReportFieldAttribute : System.Attribute
    {
        private string _DisplayName = null;

        public string DisplayName
        {
            get
            {
                return _DisplayName;
            }
            set
            {
                DisplayName = value;
            }
        }

        public ExcelReportFieldAttribute(string displayName)
        {
            _DisplayName = displayName;
        }
        public ExcelReportFieldAttribute()
        {
            
        }
         
    }
}
