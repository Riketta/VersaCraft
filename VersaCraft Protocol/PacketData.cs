using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VersaCraft.Protocol
{
    public interface IPacketData
    {
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct AuthData : IPacketData
    {
        public string Session;
        public string Username;
        public string PassHash;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ClientsData : IPacketData
    {
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct Client
        {
            /// <summary>
            /// Path to client on update server. Unique value, primary key.
            /// </summary>
            public string Path;

            /// <summary>
            /// Client name, can be empty, path used as name than. Unique value.
            /// </summary>
            public string Name;

            /// <summary>
            /// Server address with port. Can be blank.
            /// </summary>
            public string Server;

            /// <summary>
            /// Individual client URL address. Can be blank.
            /// </summary>
            public string URL;
        }

        /// <summary>
        /// Clients list on update server.
        /// </summary>
        public Client[] Clients;

        public static Client[] Parse(string clients)
        {
            List<Client> newClients = new List<Client>();

            string[] rawClients = clients.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawClient in rawClients)
            {
                string[] clientFields = rawClient.Trim().Split(';'); // care with trim
                string path = clientFields[0];
                string name = clientFields[1];
                string server = clientFields[2];
                string url = clientFields[3];

                if (string.IsNullOrEmpty(path)) // path can't be empty as primary key
                    continue;

                Client client = new Client()
                {
                    Path = path,
                    Name = !string.IsNullOrEmpty(name) ? name : path, // not combined path with server folder for updates!
                    Server = !string.IsNullOrEmpty(server) ? server : "",
                    URL = !string.IsNullOrEmpty(url) ? url : "",
                };

                // Can't be more than one unique identifiers is same storage
                if (newClients.Exists(c => c.Name == client.Name) || newClients.Exists(c => c.Path == client.Path))
                    continue;

                newClients.Add(client);
            }

            return newClients.ToArray();
        }

        public override string ToString()
        {
            string clients = "";
            foreach (var ver in Clients)
                clients += string.Format("{0};{1};{2};{3}|", ver.Path, ver.Name, ver.Server, ver.URL);

            return clients;
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ClientsFilesData : IPacketData
    {
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct File
        {
            /// <summary>
            /// Path to file on update server.
            /// </summary>
            public string Filepath;

            /// <summary>
            /// File hash.
            /// </summary>
            public string Hash;
        }

        /// <summary>
        /// List of all clients files on update server.
        /// </summary>
        public File[] Files;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct FileData : IPacketData
    {
        public string Filepath;
        
        /// <summary>
        /// -1 if launcher up to date
        /// </summary>
        public int FileSize;
        
        public byte[] File;
    }
}
