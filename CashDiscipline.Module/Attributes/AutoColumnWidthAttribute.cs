using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.Attributes
{
    public class AutoColumnWidthAttribute : Attribute
    {
        private bool autoColumnWidth;
        public AutoColumnWidthAttribute(bool autoColumnWidth)
        {
            this.autoColumnWidth = autoColumnWidth;
        }

        public bool AutoColumnWidth
        {
            get
            {
                return autoColumnWidth;
            }
            set
            {
                autoColumnWidth = value;
            }
        }
    }
}
