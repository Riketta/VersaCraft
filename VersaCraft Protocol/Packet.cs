using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VersaCraft.Protocol
{
    [Serializable]
    public enum PacketType
    {
        /// <summary>
        /// Запрос на авторизацию от лаунчера с аунтификационными данными в виде <see cref="Packet.Data"/>.
        /// Является ответом на запрос сервера <see cref="ServerRequestAuth"/>.
        /// Ответ не ожидается, сервер вносит данные о ожидаемой сессии с клиентом на ближайшее время.
        /// </summary>
        LauncherSendAuth,


        /// <summary>
        /// Запрос на необходимость обновления от лаунчера в виде его текущей версии. В ответ ожидается <see cref="ServerSendLauncherUpdate"/> с новой версией.
        /// <see cref="Packet.Data"/> - строка с версией, результат вызова функции лаунчера <see cref="Assembly.GetEntryAssembly().GetName().Version.ToString()"/>.
        /// </summary>
        LauncherRequestLauncherUpdate,
        /// <summary>
        /// Ответ от сервера на пакет <see cref="LauncherRequestLauncherUpdate"/> если версия лаунчера устаревшая, в качестве данных передается новый файл лаунчера.
        /// <see cref="Packet.Data"/> - файл новой версии лаунчера в виде <see cref="FileData"/>
        /// </summary>
        ServerSendLauncherUpdate,


        /// <summary>
        /// Запрос от лаунчера клиентов доступных для загрузки. В ответ ожидается <see cref="ServerSendClientsList"/> со списком версий.
        /// </summary>
        LauncherRequestClients,
        /// <summary>
        /// Ответ от сервера на пакет <see cref="LauncherRequestClients"/> со списком клиентов.
        /// <see cref="Packet.Data"/> - список клиентов в виде <see cref="ClientsData"/>
        /// </summary>
        ServerSendClientsList,


        /// <summary>
        /// Запрос от лаунчера списка файлов актуальных версий клиентов. В ответ ожидается <see cref="ServerSendClientsFiles"/> со списоком файлов и их хэшами.
        /// </summary>
        LauncherRequestClientsFiles,
        /// <summary>
        /// Ответ от сервера на пакет <see cref="LauncherRequestClientsFiles"/> со списком файлов всех клиентов.
        /// <see cref="Packet.Data"/> - список клиентов в виде <see cref="ClientsFilesData"/>
        /// </summary>
        ServerSendClientsFiles,


        /// <summary>
        /// Запрос от лаунчера на передачу конкретного файла. В ответ ожидается <see cref="ServerSendClientFile"/> с запрошенным файлом.
        /// <see cref="Packet.Data"/> - строка, путь к файлу в формате полученном от сервера.
        /// </summary>
        LauncherRequestClientFile,
        /// <summary>
        /// Ответ сервера на пакет <see cref="LauncherRequestClientFile"/> с запрашиваемым файлом.
        /// <see cref="Packet.Data"/> - файл новой версии лаунчера в виде <see cref="FileData"/>.
        /// </summary>
        ServerSendClientFile,


        /// <summary>
        /// Запрос на повторную авторизацию от сервера, в ответ ожидается <see cref="AuthData"/>.
        /// </summary>
        ServerRequestAuth,
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Packet
    {
        public uint Size;
        public PacketType Type;
        public int DataSize;
        public byte[] Data;
    }
}
