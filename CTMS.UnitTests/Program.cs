using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTMS.UnitTests.InMemoryDbTest;
using CTMS.UnitTests.MSSqlDbTest;

namespace CTMS.UnitTests
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var tests = new CashFlowTests();
            tests.SetUpFixture();
            tests.Setup();
            tests.AccountCcyAmtIsCorrectIfForexLinkFifo();
            tests.TearDown();
            tests.TearDownFixture();
            Console.ReadKey();
        }
    }
}
