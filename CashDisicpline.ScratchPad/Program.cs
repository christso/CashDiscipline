using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CashDisicpline.ScratchPad
{
    class Program
    {
        static void Main(string[] args)
        {
            var type = Assembly.GetExecutingAssembly().GetType("string");


            Console.ReadKey();
        }
    }
}
