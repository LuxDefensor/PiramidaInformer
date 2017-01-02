using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PiramidaInformer
{


    static class Logger
    {
        static string fName = "Informer.log";

        public static void Log(string message)
        {
            StreamWriter log = new StreamWriter(fName, true);
            log.WriteLine(DateTime.Now.ToString() + ": " + message);
            log.Close();
        }

    }
}
