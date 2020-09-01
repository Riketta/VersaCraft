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

        ClientsData clients;
        public ClientsData Clients { get => clients; }
        
        ClientsFilesData clientsFiles;
        public ClientsFilesData ClientsFiles { get => clientsFiles; }

        public void Load()
        {

        }

        public void Save()
        {

        }

        public void UpdateClients(ClientsData clientsData)
        {
            clients = clientsData;
            // TODO: find outdated clients and remove it, except saves (?)
            
            ControlsManager.UpdateClientsComboBox(clientsData);
        }

        public void UpdateClientsFiles(ClientsFilesData clientsFilesData)
        {
            clientsFiles = clientsFilesData;
        }
    }
}
