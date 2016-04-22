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
            RunTestMetheod();

            Console.ReadKey();
        }

        static void RunTestMetheod()
        {
            var tests = new CashDiscipline.UnitTests.TempTests();
            tests.TempTest();
        }

        static void RunTest()
        {
            var tests = new CashDiscipline.UnitTests.TempTests();
            tests.SetUpFixture();
            tests.Setup();
            tests.TempTest();
            tests.TearDown();
            tests.TearDownFixture();
            Console.WriteLine("Test Passed");
        }
    }
}
