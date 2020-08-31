using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VersaCraft.Logger;
using VersaCraft.Protocol;

namespace VersaCraft_Auth
{
    class Config
    {
        private static Logger logger = Logger.GetLogger();


        private static Config instance = null;
        private static readonly object padlock = new object();

        public static Config Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                        instance = new Config();
                    return instance;
                }
            }
        }

        Config()
        {
        }

        /// <summary>
        /// Путь к папке с данными обновлений
        /// </summary>
        private static readonly string updatesFolder = @"updates";

        /// <summary>
        /// Путь к актуальной версии лаунчера
        /// </summary>
        private static readonly string launcherFile = Path.Combine(updatesFolder, @"launcher.exe");
        //private static readonly string launcherFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"Updates\Launcher.exe");

        /// <summary>
        /// Путь к файлу со списком клиентов в формате Nx[client_path:client_name:client_url;] (см. реализацию <see cref="ClientsData.ToString"/>)
        /// </summary>
        private static readonly string clientsListFile = Path.Combine(updatesFolder, @"clients.txt");
        //private static readonly string clientsListFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"Updates\Clients.txt");


        byte[] launcherData = null;
        public byte[] LauncherData { get => launcherData; }

        string launcherVersion = null;
        public string LauncherVersion { get => launcherVersion; }

        ClientsData clientsData;
        public ClientsData ClientsData { get => clientsData; }

        ClientsFilesData clientsFiles;
        public ClientsFilesData ClientsFiles { get => clientsFiles; }


        public void Load()
        {
            logger.Info("Caching launcher");
            LoadLauncher();

            logger.Info("Caching client versions");
            LoadClientVersions();

            logger.Info("Caching clients files");
            LoadClientFilesData();
        }

        /// <summary>
        /// Кэширует файл лаунчера в память и фиксирует его версию
        /// </summary>
        public void LoadLauncher()
        {
            logger.Info("Looking for launcher: \"{0}\"", launcherFile);
            if (!File.Exists(launcherFile))
            {
                logger.Error("No launcher file found!");
                return;
            }

            launcherData = File.ReadAllBytes(launcherFile);
            launcherVersion = AssemblyName.GetAssemblyName(launcherFile).Version.ToString();
            logger.Info("Loaded into memory ({0} bytes) launcher ver. {1}", launcherData.Length, LauncherVersion);
        }

        /// <summary>
        /// Считывает данные из файла со списком клиентов о названии клиента, пути к нему и выделенной странице
        /// </summary>
        public void LoadClientVersions()
        {
            logger.Info("Looking for clients list file: \"{0}\"", clientsListFile);
            if (!File.Exists(clientsListFile))
            {
                logger.Error("No clients list file found!");
                return;
            }

            List<ClientsData.Client> newClients = new List<ClientsData.Client>();

            string versionsRaw = File.ReadAllText(clientsListFile);
            // client_path:client_name:client_url;client_path:client_name:client_url;...
            string[] clients = versionsRaw.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            logger.Info("Caching {0} clients", clients.Length);

            foreach (var rawClient in clients)
            {
                string[] clientFields = rawClient.Split(':');
                if (string.IsNullOrEmpty(clientFields[0]))
                {
                    logger.Error("Found client without path!");
                    continue;
                }

                string path = Path.Combine(updatesFolder, clientFields[0]);
                ClientsData.Client client = new ClientsData.Client()
                {
                    Path = path,
                    Name = !string.IsNullOrEmpty(clientFields[1]) ? clientFields[1] : clientFields[0], // not combined path!
                    URL = !string.IsNullOrEmpty(clientFields[2]) ? clientFields[2] : "",
                };

                logger.Debug("Added new client \"{0}\" with path \"{1}\" and URL \"{2}\"", client.Name, client.Path, client.URL);
                newClients.Add(client);
            }

            clientsData.Clients = newClients.ToArray();
        }

        /// <summary>
        /// Генерирует список всех файлов и их хэшей всех клиентов, требует наличия списка версий <see cref="ClientsData"/>
        /// </summary>
        public void LoadClientFilesData()
        {
            if (clientsData.Clients == null || clientsData.Clients.Length == 0)
            {
                logger.Warn("No clients found! Can't parse files for update lists");
                return;
            }

            List<ClientsFilesData.File> newFiles = new List<ClientsFilesData.File>();
            foreach (var client in clientsData.Clients)
            {
                string[] allFiles = Directory.GetFiles(client.Path, "*", SearchOption.AllDirectories);
                logger.Info("Caching {1} files for client \"{0}\"", client.Path, allFiles.Length);

                foreach (var filepath in allFiles)
                {
                    ClientsFilesData.File file = new ClientsFilesData.File()
                    {
                        Filepath = filepath,
                        Hash = CryptoUtils.CalculateFileMD5(filepath),
                    };

                    //logger.Debug("Added file [{0}] with hash {1}", file.Filepath, file.Hash);
                    newFiles.Add(file);
                }
            }

            logger.Info("Cached total {0} files for {1} clients", newFiles.Count, clientsData.Clients.Length);
            clientsFiles.Files = newFiles.ToArray();
        }
    }
}
