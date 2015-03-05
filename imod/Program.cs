using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace imod
{
    class Program
    {
        static void Main(string[] args)
        {
            var scenarios = new Scenarios();

            string path = "c:\\dropbox\\instances\\imod\\freq2";

            DirectoryInfo dir = new DirectoryInfo(path);

//            foreach (var file in dir.GetFiles("*.csv"))
            foreach (var file in dir.GetFiles("home-250-*-60.csv"))
//            foreach (var file in dir.GetFiles("home-500-3-60.csv"))
            {
//                scenarios.runScenario1(file.FullName);
                scenarios.runScenario2(file.FullName);
//                scenarios.runScenario3(file.FullName);
//                scenarios.runScenario4(file.FullName);
            }
        }
    }
}
