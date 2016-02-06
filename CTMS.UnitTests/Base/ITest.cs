using DevExpress.ExpressApp.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTMS.UnitTests.Base
{
    public interface ITest
    {
        void SetUpFixture();
        void Setup();
        void TearDown();
        void TearDownFixture();
        event EventHandler<EventArgs> OnSetupObjects;
        XPObjectSpace ObjectSpace { get; set; }
    }
}
