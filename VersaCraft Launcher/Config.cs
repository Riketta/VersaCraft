using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersaCraft.Logger;
using VersaCraft.Protocol;

namespace VersaCraft_Launcher
{
    class Config
    {
        private static Logger logger = Logger.GetLogger();


        private static Config instance = null;
        private static readonly object padlock = new object();


        IniFile ini;
        static readonly string ConfigPath = "launcher.ini";
        static readonly string LauncherSection = "launcher";

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

        // no reason to store it, this data actual only during current update session
        public ClientsFilesData ClientsFiles { get => clientsFiles; }
        ClientsFilesData clientsFiles;

        public ClientsData Clients
        {
            get
            {
                if (clients.Clients == null || clients.Clients.Length == 0)
                    clients = new ClientsData()
                    {
                        Clients = ClientsData.Parse(ini.GetString(LauncherSection, nameof(clients), "")),
                    };

                return clients;
            }
            private set
            {
                if (ini != null)
                {
                    ini.WriteValue(LauncherSection, nameof(clients), value.ToString());
                    clients = value;
                }
                else
                    logger.Error("Failed to write data to config! Field: {0}", nameof(clients));
            }
        }
        private ClientsData clients;

        public string Username
        {
            get
            {
                if (string.IsNullOrEmpty(username))
                    username = ini.GetString(LauncherSection, nameof(username), "");
                return username;
            }
            set
            {
                if (ini != null)
                {
                    ini.WriteValue(LauncherSection, nameof(username), value);
                    username = value;
                }
                else
                    logger.Error("Failed to write data to config! Field: {0}", nameof(username));
            }
        }
        private string username;

        /// <summary>
        /// Stores password hash. If <see cref="IsSavingPassword"/> set to false then auto wipe stored password hash and return "".
        /// </summary>
        public string PassHash
        {
            get
            {
                // don't load hash if it for some reason stored in config without rule for it and also wipe it
                if (!IsSavingPassword)
                {
                    PassHash = "";
                    return "";
                }

                if (string.IsNullOrEmpty(pass_hash))
                    pass_hash = ini.GetString(LauncherSection, nameof(pass_hash), "");
                return pass_hash;
            }
            set
            {
                if (ini != null)
                {
                    if (IsSavingPassword)
                    {
                        ini.WriteValue(LauncherSection, nameof(pass_hash), IsSavingPassword ? value : "");
                        pass_hash = IsSavingPassword ? value : "";
                    }
                }
                else
                    logger.Error("Failed to write data to config! Field: {0}", nameof(pass_hash));
            }
        }
        private string pass_hash;

        /// <summary>
        /// Represents necessity to save password hash. If set to false auto cleanup <see cref="PassHash"/> hash data in local storage.
        /// </summary>
        public bool IsSavingPassword
        {
            get
            {
                if (!is_saving_password.HasValue)
                    is_saving_password = ini.GetBoolean(LauncherSection, nameof(is_saving_password), false);
                return is_saving_password.Value;
            }
            set
            {
                if (!value) PassHash = ""; // force clean-up possibly already stored hash
                
                if (ini != null)
                {
                    ini.WriteValue(LauncherSection, nameof(is_saving_password), value);
                    is_saving_password = value;
                }
                else
                    logger.Error("Failed to write data to config! Field: {0}", nameof(is_saving_password));
            }
        }
        private bool? is_saving_password;

        public string SelectedClient
        {
            get
            {
                if (string.IsNullOrEmpty(selected_client))
                    selected_client = ini.GetString(LauncherSection, nameof(selected_client), "");
                return selected_client;
            }
            set
            {
                if (ini != null)
                {
                    ini.WriteValue(LauncherSection, nameof(selected_client), value);
                    selected_client = value;
                }
                else
                    logger.Error("Failed to write data to config! Field: {0}", nameof(selected_client));
            }
        }
        private string selected_client;

        public string JVMArguments // TODO: bind per client (?)
        {
            get
            {
                if (string.IsNullOrEmpty(jvm_arguments))
                    jvm_arguments = ini.GetString(LauncherSection, nameof(jvm_arguments), "-Xmx2G -XX:+UnlockExperimentalVMOptions -XX:+UseG1GC -XX:G1NewSizePercent=20 -XX:G1ReservePercent=20 -XX:MaxGCPauseMillis=50 -XX:G1HeapRegionSize=32M");
                return jvm_arguments;
            }
            set
            {
                if (ini != null)
                {
                    ini.WriteValue(LauncherSection, nameof(jvm_arguments), value);
                    jvm_arguments = value;
                }
                else
                    logger.Error("Failed to write data to config! Field: {0}", nameof(jvm_arguments));
            }
        }
        private string jvm_arguments;

        public string Address
        {
            get
            {
                if (string.IsNullOrEmpty(address))
                    address = ini.GetString(LauncherSection, nameof(address), "versalita.net");
                return address;
            }
        }
        private string address;

        public Config Load()
        {
            logger.Debug("Creating INI reader");
            if (ini != null)
                logger.Warn("INI reader already exists! Recreating.");
            ini = new IniFile(ConfigPath);

            logger.Debug("Reading config");
            // init (pre-read/cache) next values:
            _ = Clients;
            _ = Username;
            _ = PassHash;
            _ = IsSavingPassword;
            _ = SelectedClient;
            _ = JVMArguments;
            _ = Address;
            logger.Debug(ToString());

            return this;
        }

        public void UpdateClients(ClientsData clientsData)
        {
            Clients = clientsData;

            // TODO: find outdated clients and remove it, except saves (?)
            ControlsManager.UpdateClientsComboBox(clientsData);
        }

        public void UpdateClientsFiles(ClientsFilesData clientsFilesData)
        {
            clientsFiles = clientsFilesData;
        }

        public override string ToString()
        {
            string value = "";

            value += string.Format($"{nameof(clients)}: \"{clients}\"; ");
            value += string.Format($"{nameof(username)}: \"{username}\"; ");
            value += string.Format($"{nameof(pass_hash)}: \"{pass_hash}\"; ");
            value += string.Format($"{nameof(is_saving_password)}: \"{is_saving_password}\"; ");
            value += string.Format($"{nameof(selected_client)}: \"{selected_client}\"; ");
            value += string.Format($"{nameof(jvm_arguments)}: \"{jvm_arguments}\"; ");
            value += string.Format($"{nameof(address)}: \"{address}\"; ");

            return value;
        }
    }
}
