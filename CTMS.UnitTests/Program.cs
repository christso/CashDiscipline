using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTMS.UnitTests.InMemoryDbTest;

namespace CTMS.UnitTests
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var tests = new ForexTradeTests();
            tests.Setup();
            tests.UploadForexTradeToCashFlow();

            Console.ReadKey();
        }
    }
}
