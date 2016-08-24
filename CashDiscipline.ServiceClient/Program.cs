using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.ServiceClient
{
    class Program
    {
        static void Main(string[] args)
        {
            CashDiscServiceReference.Service1Client client = new
                CashDiscServiceReference.Service1Client();

            while (true)
            {
                string inputString = Console.ReadLine();
                string returnString = client.GetData(inputString);

                Console.WriteLine(returnString);
            }
        }
    }
}
