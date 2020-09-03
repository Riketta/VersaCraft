using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VersaCraft.Logger;

namespace VersaCraft_Launcher
{
    class FatalError
    {
        private static readonly Logger logger = Logger.GetLogger();

        public static void Exception(Exception ex, bool crash = false)
        {
            Exception(ex.ToString());
            if (crash) throw ex;
        }

        public static void Exception(string error)
        {
            Console.WriteLine(error);
            
            using (StreamWriter writer = new StreamWriter(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "error.txt"), true))
                writer.WriteLine("[{0}] {1}", DateTime.Now.ToString("o"), error);
            
            logger.Error(error);
        }
    }
}
