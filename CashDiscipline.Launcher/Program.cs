using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashDiscipline.Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string directoryName = System.IO.Path.GetDirectoryName(location);
            string programPath = System.IO.Path.Combine(directoryName, "Cash Discipline.bat");
            System.Diagnostics.Process.Start(programPath);
        }
    }
}
