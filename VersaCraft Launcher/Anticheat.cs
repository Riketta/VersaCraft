using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersaCraft.Logger;
using VersaCraft.Protocol;

namespace VersaCraft_Launcher
{
    class Anticheat
    {
        private static readonly Logger logger = Logger.GetLogger();

        public static string Session
        {
            get
            {
                if (string.IsNullOrEmpty(session))
                    session = CryptoUtils.CalculateStringSHA1(string.Format("{0}_{1}_{2}", Config.Instance.Username, DateTime.Now.Ticks.ToString(), CryptoUtils.GetComputerSid().ToString()));
                return session;
            }
        }
        private static string session;

        public static void HideLauncher()
        {
            // TODO: implement
        }
    }
}
