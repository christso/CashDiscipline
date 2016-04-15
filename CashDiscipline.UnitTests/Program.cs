using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CashDiscipline.UnitTests
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var tests = new CashDiscipline.UnitTests.ForexLinkTests();
            tests.SetUpFixture();
            tests.Setup();
            //tests.ForexLinkFifo();
            tests.TearDown();
            tests.TearDownFixture();
            Console.WriteLine("Test Passed");
            Console.ReadKey();
        }
    }
}
