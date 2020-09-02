using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VersaCraft.Logger;
using VersaCraft.Protocol;

namespace VersaCraft_Auth
{
    class Program
    {
        private static Logger logger = Logger.GetLogger();


        static void Main(string[] args)
        {
            try
            {
                logger.Info("VersaCraft Auth&Update Server ver. {0}", Assembly.GetEntryAssembly().GetName().Version.ToString());

                logger.Info("Loading config");
                Config.Instance.Load();

                logger.Info("Starting session cleaner");
                SessionManager.StartSessionCleaner();

                logger.Info("Starting server");
                Server.Start(); // blocking thread
                Server.Stop();
            }
            catch (Exception ex)
            {
                FatalError.Exception(ex, true);
            }
        }
    }
}
