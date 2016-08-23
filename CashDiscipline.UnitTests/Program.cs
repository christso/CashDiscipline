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

        static void RunTest()
        {
            var tests = new ForexTradeTests();
            tests.SetUpFixture();
            tests.Setup();

            tests.PredeliverForexTrade();

            tests.TearDown();
            tests.TearDownFixture();

            Console.WriteLine("Test passed");
        }

        static void RunTempTestMethod()
        {
            var tests = new CashDiscipline.UnitTests.TempTests();
            tests.TempTest();
        }

        static void RunExperiment()
        {
            var tests = new CashDiscipline.UnitTests.ForexLinkTests();
            tests.SetUpFixture();

            for (int i = 0; i < 1; i++)
            {
                var arr = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
                //var shuffled = Xafology.Utils.DataStructUtils.RandomizeArray<int>(arr);
                var shuffled = arr;

                try
                {
                    tests.Setup();
                    tests.ForexSettleFifoIntegrated(shuffled);
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
