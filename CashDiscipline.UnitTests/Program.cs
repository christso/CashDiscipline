using NUnit.Framework;
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
            RunTest();

            Console.ReadKey();
        }

        static void RunTempTestMethod()
        {
            var tests = new CashDiscipline.UnitTests.TempTests();
            tests.TempTest();
        }

        static void RunTest()
        {
            var tests = new CashDiscipline.UnitTests.ForexLinkTests();
            tests.SetUpFixture();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    tests.Setup();
                    tests.ForexLinkFifo(i);
                    tests.TearDown();
                    Console.WriteLine("Test {0} Passed", i);
                }
                catch (AssertionException ex)
                {
                    Console.WriteLine("Test {0} Failed\n{1}", i,
                        ex.Message);
                }
            }
            tests.TearDownFixture();
            
        }
    }
}
