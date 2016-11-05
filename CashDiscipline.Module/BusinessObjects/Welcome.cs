using DevExpress.ExpressApp.DC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Module.BusinessObjects
{
    [DomainComponent]
    public class Welcome
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string TextValue { get; set; }
    }
}
