using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VersaCraft.Protocol
{
    public interface PacketData
    {
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct AuthData : PacketData
    {
        public string Session;
        public string Username;
        public string PassHash;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ClientsData : PacketData
    {
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct Client
        {
            /// <summary>
            /// Путь к клиенту на сервере обновлений. Должно быть уникальным значением, основной ID клиента.
            /// </summary>
            public string Path;

            /// <summary>
            /// Имя клиента, может быть пустым (тогда за имя считать путь). Должно быть уникальным значением.
            /// </summary>
            public string Name;

            /// <summary>
            /// Адрес индивидуальной страницы клиента, может быть пустым.
            /// </summary>
            public string URL;
        }

        /// <summary>
        /// Список клиентов на сервере обновлений.
        /// </summary>
        public Client[] Clients;

        public static Client[] Parse(string clients)
        {
            List<Client> newClients = new List<Client>();

            string[] rawClients = clients.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var rawClient in rawClients)
            {
                string[] clientFields = rawClient.Trim().Split(':'); // care with trim
                string path = clientFields[0];
                string name = clientFields[1];
                string url = clientFields[2];

                if (string.IsNullOrEmpty(path)) // path can't be empty as primary key
                    continue;

                Client client = new Client()
                {
                    Path = path,
                    Name = !string.IsNullOrEmpty(name) ? name : path, // not combined path with server folder for updates!
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
                clients += string.Format("{0}:{1}:{2};", ver.Path, ver.Name, ver.URL);

            return clients;
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ClientsFilesData : PacketData
    {
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct File
        {
            /// <summary>
            /// Путь к файлу на сервере обновлений
            /// </summary>
            public string Filepath;

            /// <summary>
            /// Хэш клиента
            /// </summary>
            public string Hash;
        }

        /// <summary>
        /// Список файлов всех клиентов на сервере обновлений
        /// </summary>
        public File[] Files;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct FileData : PacketData
    {
        public string Filepath;
        public int FileSize;
        public byte[] File;
    }
}
