using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDisicpline.ScratchPad
{
    public class DtsTestClient
    {
        public void Execute()
        {
            Microsoft.SqlServer.Dts.Runtime.Application app = 
                new Microsoft.SqlServer.Dts.Runtime.Application();
            Package pkg = new Package();
        }
    }
}
