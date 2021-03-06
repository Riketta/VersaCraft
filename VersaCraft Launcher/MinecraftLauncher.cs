﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VersaCraft.Logger;
using VersaCraft.Protocol;

namespace VersaCraft_Launcher
{
    class MinecraftLauncher
    {
        private static readonly Logger logger = Logger.GetLogger();


        //static readonly string LoggerPrefix = "[Minecraft] ";
        public static readonly string VersionsFolder = @"versions";
        public static readonly string AssetFolder = @"assets";
        public static readonly string AssetIndexFolder = Path.Combine(AssetFolder, @"indexes");
        public static readonly string NativesFolder = @"natives";
        public static readonly string LibrariesFolder = @"libraries";
        public static readonly string ModsFolder = @"mods";
        public static readonly string JavaExecutable = @"jre\bin\javaw.exe";

        public static string GetMainClass(string version)
        {
            switch (version)
            {
                case "1.16":
                case "1.16.1":
                case "1.16.2":
                    return "net.minecraft.client.main.Main";
                
                default:
                    logger.Error("No main class known for Minecraft version {0}", version);
                    return null;
            }
        }

        public static string[] GetVersionFiles(string gameDir)
        {
            string path = Path.Combine(gameDir, VersionsFolder);
            if (Directory.Exists(path))
                return Directory.GetFiles(path, "*.jar", SearchOption.AllDirectories);
            
            return null;
        }

        public static void RemoveVersionFiles(string gameDir)
        {
            Directory.Delete(Path.Combine(gameDir, VersionsFolder), true);
        }

        public static string GetVersionFile(string gameDir)
        {
            string[] files = GetVersionFiles(gameDir);

            if (files.Length == 0)
            {
                logger.Error("No Minecraft version found!");
                return null;
            }
            if (files.Length > 1)
                logger.Warn("Found more than one Minecraft version!");

            return files[0];
        }

        public static string GetLibraries(string gameDir)
        {
            string libraries = "";
            string[] files = Directory.GetFiles(Path.Combine(gameDir, LibrariesFolder), "*.jar", SearchOption.AllDirectories);

            foreach (var file in files)
                libraries += string.Format("\"{0}\";", file);

            return libraries.Trim(';');
        }

        public static string GetAssetIndex(string gameDir)
        {
            string[] files = Directory.GetFiles(Path.Combine(gameDir, AssetIndexFolder), "*.json", SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                logger.Error("No asset index file found!");
                return null;
            }
            if (files.Length > 1)
                logger.Warn("Found more than one asset index file!");

            return Path.GetFileNameWithoutExtension(files[0]);
        }

        public static Process Start(string username, string session, string server, string gameDir)
        {
            gameDir = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), gameDir);

            string jre = Path.Combine(gameDir, JavaExecutable);
            string natives = Path.Combine(gameDir, NativesFolder);
            string versionFile = GetVersionFile(gameDir);
            string version = Path.GetFileNameWithoutExtension(versionFile);
            string libraries = GetLibraries(gameDir);
            string assets = Path.Combine(gameDir, AssetFolder);
            string assetIndex = GetAssetIndex(gameDir);

            // TODO: add additional minecraft and JRE CLI arguments from minecraft version config rules

            // all paths MUST be in quotes!
            string args = $@"--username {username} --version {version} --gameDir ""{gameDir}"" --assetsDir ""{assets}"" --assetIndex ""{assetIndex}"" --uuid {session} --accessToken {session} --userType mojang --versionType release";
            string coreargs = $@"-Djava.library.path=""{natives}"" -cp {libraries};""{versionFile}"" {GetMainClass(version)}";
            string additional = $@"-Xss1M -Dminecraft.launcher.brand=minecraft-launcher -Dminecraft.launcher.version=2.1.17315 -Dlog4j.configurationFile=""{gameDir}\assets\log_configs\client-1.12.xml""";


            if (!string.IsNullOrEmpty(server))
            {
                string[] rawserver = server.Split(':');
                string address = rawserver[0];
                string port = rawserver[1];

                server = $@"--server {address} --port {port}";
            }

            string execute = $@"{Config.Instance.JVMArguments} {additional} {coreargs} {args} {server}";

            Process minecraft = new Process();
            minecraft.StartInfo.FileName = jre;
            minecraft.StartInfo.Arguments = execute;
            minecraft.StartInfo.WorkingDirectory = gameDir;

            //minecraft.StartInfo.UseShellExecute = false;
            //minecraft.StartInfo.RedirectStandardError = true;
            //minecraft.StartInfo.RedirectStandardInput = true;
            //minecraft.StartInfo.RedirectStandardOutput = true;

            minecraft.Start();

            //Task.Run(() =>
            //{
            //    while (!minecraft.StandardOutput.EndOfStream)
            //    {
            //        string line = minecraft.StandardOutput.ReadLine();
            //        if (string.IsNullOrEmpty(line))
            //            continue;

            //        logger.Info(LoggerPrefix + line);
            //    }
            //});

            //Task.Run(() =>
            //{
            //    while (!minecraft.StandardError.EndOfStream)
            //    {
            //        string line = minecraft.StandardError.ReadLine();
            //        if (string.IsNullOrEmpty(line))
            //            continue;

            //        logger.Error(LoggerPrefix + line);
            //    }
            //});

            return minecraft;
        }

        public static async Task Login(string username, string password)
        {
            if (Client.IsConnected())
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    logger.Warn("No login data entered to request auth");
                    ControlsManager.SetStatus("Enter login and password!");
                    return;
                }

                string clientName = ControlsManager.GetSelectedClientName();
                if (string.IsNullOrEmpty(clientName))
                {
                    logger.Error("Client not selected!");
                    ControlsManager.SetStatus("Select client!");
                    return;
                }

                ControlsManager.DisableLoginButton();
                logger.Info("Requesting client update");
                ClientsData.Client client = Config.Instance.Clients.Clients.First(c => c.Name == clientName);

                ControlsManager.SetStatus("Checking for updates...");
                await UpdateManager.UpdateClient(client);

                logger.Info("Waiting client updates to finish");
                ControlsManager.SetStatus("Updating...");
                await Task.Run(() => { UpdateManager.ClientUpdate.WaitOne(); });
                logger.Info("Client updating done");
                ControlsManager.SetStatus("Ready");

                logger.Info("Updating config with current login data");
                Config.Instance.Username = username;

                string passHash = Config.Instance.PassHash == password ? Config.Instance.PassHash : CryptoUtils.CalculateStringVersaHash(password);
                Config.Instance.PassHash = passHash;

                string session = Anticheat.Session;

                logger.Info("Requesting auth");
                ControlsManager.SetStatus("Requesting auth...");
                Client.SendAuth(session, username, passHash);

                logger.Info("Launching Minecraft");
                ControlsManager.SetStatus("Launching Minecraft...");
                Anticheat.HideLauncher();
                var minecraft = Start(username, session, client.Server, client.Path);

                if (Config.Instance.WindowedFullscreen)
                    EnableWindowedFullscreen(minecraft);

                Anticheat.Protect();

                logger.Info("All jobs done");
                System.Windows.Application.Current.Shutdown();
            }

            // TODO: offline mode
        }

        public static void EnableWindowedFullscreen(Process minecraft)
        {
            minecraft.WaitForInputIdle();

            bool isAppeared = Task.Run(() =>
            {
                while (string.IsNullOrEmpty(minecraft.MainWindowTitle))
                    minecraft.Refresh();
            }).Wait(10 * 1000); // waiting for minecraft window appear up to _ seconds

            if (!isAppeared)
            {
                logger.Error("Timeout! No Minecraft window appeared!");
                return;
            }

            IntPtr hWnd = minecraft.MainWindowHandle;

            logger.Info("Applying window changes");
            long stylesToOff = Win32.WS_BORDER | Win32.WS_BORDER | Win32.WS_DLGFRAME | Win32.WS_SYSMENU | Win32.WS_MINIMIZEBOX | Win32.WS_MAXIMIZEBOX | Win32.WS_THICKFRAME;
            SetWindowStyleOff(hWnd, Win32.GWL_STYLE, stylesToOff);
            SetWindowFullscreen(hWnd);

            logger.Info("ReMaximizing window");
            MinimizeWindow(hWnd);
            MaximizeWindow(hWnd);
        }

        static void SetWindowFullscreen(IntPtr hWnd)
        {
            Rectangle rect = Screen.FromHandle(hWnd).Bounds;
            Win32.SetWindowPos(hWnd, hWnd, rect.X, rect.Y, rect.Width, rect.Height, Win32.SWP_SHOWWINDOW | Win32.SWP_FRAMECHANGED);
        }

        static void MinimizeWindow(IntPtr hWnd)
        {
            Win32.ShowWindow(hWnd, Win32.SW_MINIMIZE);
        }

        static void MaximizeWindow(IntPtr hWnd)
        {
            Win32.ShowWindow(hWnd, Win32.SW_MAXIMIZE);
        }

        static void SetWindowStyleOff(IntPtr hWnd, int nIndex, long dwStylesToOff)
        {
            IntPtr windowStyles = GetWindowStyles(hWnd, nIndex);
            Win32.SetWindowLongPtr(hWnd, nIndex, ((windowStyles.ToInt64() | dwStylesToOff) ^ dwStylesToOff));
        }

        static IntPtr GetWindowStyles(IntPtr hWnd, int nIndex)
        {
            return Win32.GetWindowLongPtr(hWnd, nIndex);
        }
    }
}
