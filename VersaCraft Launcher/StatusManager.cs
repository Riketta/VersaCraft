using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using VersaCraft.Logger;

namespace VersaCraft_Launcher
{
    public class StatusManager
    {
        private static Logger logger = Logger.GetLogger();


        static Label status = null;

        public static void SetStatusLabel(Label statusLabel)
        {
            status = statusLabel;
        }

        public static void SetStatus(string text)
        {
            logger.Info(text);
            status.Content = text;
        }
    }
}
