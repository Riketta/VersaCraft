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
        public static readonly string UpdatesFolder = @"updates";

        /// <summary>
        /// Путь к актуальной версии лаунчера
        /// </summary>
        private static readonly string launcherFile = Path.Combine(UpdatesFolder, @"launcher.exe");
        //private static readonly string launcherFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"Updates\Launcher.exe");

        /// <summary>
        /// Путь к файлу со списком клиентов в формате Nx[client_path:client_name:client_url;] (см. реализацию <see cref="ClientsData.ToString"/>)
        /// </summary>
        private static readonly string clientsListFile = Path.Combine(UpdatesFolder, @"clients.txt");
        //private static readonly string clientsListFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"Updates\Clients.txt");


        byte[] launcherData = null;
        public byte[] LauncherData { get => launcherData; }

        string launcherVersion = null;
        public string LauncherVersion { get => launcherVersion; }

        ClientsData clients;
        public ClientsData Clients { get => clients; }

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
            string[] rawClients = versionsRaw.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            logger.Info("Caching {0} clients", rawClients.Length);

            foreach (var rawClient in rawClients)
            {
                string[] clientFields = rawClient.Trim().Split(':'); // care with trim
                string path = clientFields[0];
                string name = clientFields[1];
                string url = clientFields[2];

                if (string.IsNullOrEmpty(path))
                {
                    logger.Error("Found client without path!");
                    continue;
                }

                ClientsData.Client client = new ClientsData.Client()
                {
                    Path = path,
                    Name = !string.IsNullOrEmpty(name) ? name : path, // not combined path!
                    URL = !string.IsNullOrEmpty(url) ? url : "",
                };

                if (newClients.Exists(c => c.Name == client.Name) || newClients.Exists(c => c.Path == client.Path))
                {
                    logger.Error("Client with same name or path already added! Skipping this one: \"{0}\"; \"{1}\"", client.Name, client.Path);
                    continue;
                }
                
                newClients.Add(client);
                logger.Info("Added new client \"{0}\" with path \"{1}\" and URL \"{2}\"", client.Name, client.Path, client.URL);
            }

            ClientsData clientsData = new ClientsData()
            {
                Clients = newClients.ToArray(),
            };
            clients = clientsData;
        }

        /// <summary>
        /// Генерирует список всех файлов и их хэшей всех клиентов, требует наличия списка версий <see cref="Clients"/>
        /// </summary>
        public void LoadClientFilesData()
        {
            if (clients.Clients == null || clients.Clients.Length == 0)
            {
                logger.Warn("No clients found! Can't parse files for update lists");
                return;
            }

            List<ClientsFilesData.File> newFiles = new List<ClientsFilesData.File>();
            foreach (var client in clients.Clients)
            {
                string localpath = Path.Combine(UpdatesFolder, client.Path);
                string[] allFiles = Directory.GetFiles(localpath, "*", SearchOption.AllDirectories);
                logger.Info("Caching {1} files for client \"{0}\"", client.Path, allFiles.Length);

                foreach (var filepath in allFiles)
                {
                    string path = filepath.Split(new char[] { '\\' }, 2)[1]; // TODO: pretty scary way to fix path

                    ClientsFilesData.File file = new ClientsFilesData.File()
                    {
                        Filepath = path,
                        Hash = CryptoUtils.CalculateFileMD5(filepath),
                    };

                    logger.Debug("Added file [{0}] with hash {1}", file.Filepath, file.Hash);
                    newFiles.Add(file);
                }
            }

            ClientsFilesData clientsFilesData = new ClientsFilesData()
            {
                Files = newFiles.ToArray(),
            };
            clientsFiles = clientsFilesData;
            logger.Info("Cached total {0} files for {1} clients", newFiles.Count, clients.Clients.Length);
        }
    }
}
