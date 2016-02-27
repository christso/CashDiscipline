using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CTMS.UnitTests
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var tests = new CTMS.UnitTests.IntegrationTests();
            tests.SetUpFixture();
            tests.Setup();
            tests.ForexTradeToCashFlowToBankStmt_Integrated_SumAreEqual();
            tests.TearDown();
            tests.TearDownFixture();
            Console.WriteLine("Test Passed");
            Console.ReadKey();
        }
    }
}
