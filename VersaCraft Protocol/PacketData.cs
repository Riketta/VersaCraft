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
        public string Username;
        public string PassHash;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ClientsData : PacketData
    {
        public struct Client
        {
            /// <summary>
            /// Путь к клиенту на сервере обновлений
            /// </summary>
            public string Path;

            /// <summary>
            /// Имя клиента, может быть пустым (тогда за имя считать путь)
            /// </summary>
            public string Name;

            /// <summary>
            /// Адрес индивидуальной страницы клиента, может быть пустым
            /// </summary>
            public string URL;
        }

        /// <summary>
        /// Список версий клиентов на сервере обновлений
        /// </summary>
        public Client[] Clients;

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
