using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
        private static readonly Logger logger = Logger.GetLogger();


        static readonly string updatedPostfix = "_update";
        static readonly string backupPostfix = "_backup";

        public static AutoResetEvent LauncherUpdate = new AutoResetEvent(false);
        public static AutoResetEvent ClientUpdate = new AutoResetEvent(false);

        public static int TotalFilesToUpdate
        {
            get
            {
                return totalFilesToUpdate;
            }
            private set
            {
                totalFilesToUpdate = value;
                ControlsManager.SetMaxProgress(value); // TODO: move it away
            }
        }
        static int totalFilesToUpdate = 0;

        public static int FilesRemainingToUpdate
        {
            get
            {
                return filesRemainingToUpdate;
            }
            private set
            {
                filesRemainingToUpdate = value;
                
                if (value == 0)
                    ClientUpdate.Set();

                ControlsManager.SetProgress(TotalFilesToUpdate - FilesRemainingToUpdate); // TODO: move it away
            }
        }
        static int filesRemainingToUpdate = 0;


        public static void SelfUpdate(FileData fileData)
        {
            logger.Info("Launcher self updating");
            ControlsManager.DisableLoginButton();

            if (fileData.FileSize == -1) // up to date
            {
                LauncherUpdate.Set();
                logger.Info("No launcher update required");
                return;
            }

            byte[] launcher = fileData.File;

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

        public static async Task UpdateClient(ClientsData.Client client)
        {
            logger.Info("Updating client placed in \"{0}\"", client.Path);
            List<string> filesToUpdate = new List<string>();
         
            if (Config.Instance.ClientsFiles.Files == null || Config.Instance.ClientsFiles.Files.Length == 0)
            {
                logger.Error("No clients files data available!");
                return;
            }

            await Task.Run(() =>
            {
                List<string> nonlistedFiles = null;
                if (Directory.Exists(client.Path))
                    nonlistedFiles = new List<string>(Directory.GetFiles(client.Path, "*", SearchOption.AllDirectories));

                var remoteClientFiles = Config.Instance.ClientsFiles.Files.Where(f => f.Filepath.StartsWith(client.Path + Path.DirectorySeparatorChar) || f.Filepath.StartsWith(client.Path + Path.AltDirectorySeparatorChar));
                foreach (var remoteFile in remoteClientFiles)
                {
                    if (File.Exists(remoteFile.Filepath))
                    {
                        nonlistedFiles?.Remove(remoteFile.Filepath); // filter valid file
                        
                        if (CryptoUtils.CalculateFileMD5(remoteFile.Filepath) == remoteFile.Hash) // file exist and up to date, no reason to update it
                            continue;
                    }

                    filesToUpdate.Add(remoteFile.Filepath);
                }

                if (nonlistedFiles != null && nonlistedFiles.Count > 0)
                {
                    string[] blacklistedFolders = new string[]
                    {
                        Path.Combine(client.Path, MinecraftLauncher.VersionsFolder) + Path.DirectorySeparatorChar,
                        Path.Combine(client.Path, MinecraftLauncher.LibrariesFolder) + Path.DirectorySeparatorChar,
                        Path.Combine(client.Path, MinecraftLauncher.NativesFolder) + Path.DirectorySeparatorChar,
                        Path.Combine(client.Path, MinecraftLauncher.ModsFolder) + Path.DirectorySeparatorChar,
                        Path.Combine(client.Path, MinecraftLauncher.AssetIndexFolder) + Path.DirectorySeparatorChar,
                    };

                    foreach (var file in nonlistedFiles)
                        if (blacklistedFolders.Any(file.StartsWith))
                        {
                            logger.Warn("Deleting unknown file \"{0}\" in blacklisted folder!", file);
                            File.Delete(file);
                        }
                }

                TotalFilesToUpdate = filesToUpdate.Count;
                FilesRemainingToUpdate = filesToUpdate.Count;

                foreach (var file in filesToUpdate)
                    Client.RequestFile(file);
            });
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

            FilesRemainingToUpdate--;
        }
    }
}
