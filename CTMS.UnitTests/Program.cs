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
            var tests = new CTMS.UnitTests.FinAccountingDbTests();
            tests.SetUpFixture();
            tests.Setup();
            tests.DeleteJournals();
            tests.TearDown();
            tests.TearDownFixture();
            Console.WriteLine("Test Passed");
            Console.ReadKey();
        }
    }
}
