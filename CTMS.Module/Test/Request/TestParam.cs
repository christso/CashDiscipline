using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.Module.Test
{
    [NonPersistent]
    [AutoCreatableObject]
    [FileAttachment("File")]
    public class TestParam
    {
        public TestParam()
        {

        }

        public string Name { get; set; }
    }
}
