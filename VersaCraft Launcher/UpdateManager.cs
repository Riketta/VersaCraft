using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VersaCraft.Logger;
using VersaCraft.Protocol;

namespace VersaCraft_Launcher
{
    class UpdateManager
    {
        private static Logger logger = Logger.GetLogger();


        static readonly string updatedPostfix = "_update";
        static readonly string backupPostfix = "_backup";

        public static void SelfUpdate(byte[] launcher)
        {
            logger.Info("Launcher self updating");
            ControlsManager.DisableLoginButton();

            string currentLauncherPath = Assembly.GetExecutingAssembly().Location;
            string backupLauncherPath = currentLauncherPath + backupPostfix;
            string newLauncherPath = currentLauncherPath + updatedPostfix;

            if (File.Exists(newLauncherPath))
            {
                logger.Warn("File with name of new launcher already exist for some reason! Removing file first: \"{0}\"", newLauncherPath);
                File.Delete(newLauncherPath);
            }

            logger.Info("Writing new launcher as \"{0}\"", newLauncherPath);
            using (FileStream stream = File.OpenWrite(newLauncherPath))
                stream.Write(launcher, 0, launcher.Length);

            if (File.Exists(backupLauncherPath))
            {
                logger.Info("Removing backup launcher file: \"{0}\"", backupLauncherPath);
                File.Delete(backupLauncherPath);
            }

            logger.Info("Forming update CLI for cmd.exe");
            string args = $"/C taskkill /PID {Process.GetCurrentProcess().Id} & move \"{currentLauncherPath}\" \"{backupLauncherPath}\" & move \"{newLauncherPath}\" \"{currentLauncherPath}\" & start \"\" \"{currentLauncherPath}\" & exit";

            logger.Info("Launching external updater");
            Process.Start("cmd", args);
        }

        public static void UpdateClient(ClientsData.Client client)
        {
            logger.Info("Updating client with path \"{0}\"", client.Path);
            ControlsManager.DisableLoginButton();
            
            List<string> filesToUpdate = new List<string>();
         
            if (Config.Instance.ClientsFiles.Files == null)
            {
                logger.Error("No clients files data available!");
                return;
            }

            var remoteFiles = Config.Instance.ClientsFiles.Files.Where(f => f.Filepath.StartsWith(client.Path + Path.DirectorySeparatorChar) || f.Filepath.StartsWith(client.Path + Path.AltDirectorySeparatorChar));
            foreach (var remoteFile in remoteFiles)
            {
                if (File.Exists(remoteFile.Filepath))
                    if (CryptoUtils.CalculateFileMD5(remoteFile.Filepath) == remoteFile.Hash) // file exist and up to date, no reason to update it
                        continue;
                
                filesToUpdate.Add(remoteFile.Filepath);
            }

            if (filesToUpdate.Count > 0) // lock button untill everything downloaded
                ControlsManager.EnableLoginButtonAfter(filesToUpdate.Count);
            else
                ControlsManager.EnableLoginButton();

            // TODO: prepare progress bar

            foreach (var file in filesToUpdate)
                Client.RequestFile(file);
        }

        public static void SaveFile(FileData fileData)
        {
            string filepath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileData.Filepath);

            if (File.Exists(filepath))
            {
                logger.Debug("File \"{0}\" already exist, removing first.", filepath);
                File.Delete(filepath);
            }

            string directory = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (FileStream stream = File.OpenWrite(filepath))
                stream.Write(fileData.File, 0, fileData.File.Length);

            ControlsManager.EnableLoginButtonAfter();
        }
    }
}
