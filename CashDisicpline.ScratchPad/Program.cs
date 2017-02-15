using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDisicpline.ScratchPad
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = "20170214";
            var year = Convert.ToInt32(input.Substring(0, 4));
            var month = Convert.ToInt32(input.Substring(4, 2));
            var day = Convert.ToInt32(input.Substring(6, 2));

            var output = new DateTime(year, month, day);
            Console.WriteLine("{0}", output);
            Console.ReadKey();
        }
    }
}
