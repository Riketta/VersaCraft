using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VersaCraft.Logger;

namespace VersaCraft_Launcher
{
    class UpdateManager
    {
        private static Logger logger = Logger.GetLogger();


        public static void SelfUpdate(byte[] launcher)
        {
            string newLauncherPath = Assembly.GetExecutingAssembly().Location + "_";

            if (File.Exists(newLauncherPath))
            {
                logger.Warn("File with name of new launcher already exist for some reason! Removing file first: \"{0}\"", newLauncherPath);
                File.Delete(newLauncherPath);
            }

            logger.Info("Writing new launcher as \"{0}\"", newLauncherPath);
            using (FileStream stream = File.OpenWrite(newLauncherPath))
                stream.Write(launcher, 0, launcher.Length);

            logger.Info("Launching external updater");
            Process.Start("cmd", "");
        }
    }
}
