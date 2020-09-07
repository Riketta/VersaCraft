using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using VersaCraft.Logger;
using VersaCraft.Protocol;

namespace VersaCraft_Auth
{
    class ServerResponses
    {
        private static readonly Logger logger = Logger.GetLogger();


        public static void AcceptAuth(AuthData authData, TcpClient client)
        {
            SessionManager.AddSession(authData, client);
        }

        public static void SendLauncher(string version, TcpClient client)
        {
            if (!Version.TryParse(version, out Version requester))
                return;

            bool isUpdateRequired = Config.Instance.LauncherVersion > requester;

            FileData launcher = new FileData()
            {
                Filepath = ".",
                FileSize = isUpdateRequired ? Config.Instance.LauncherData.Length : -1,
                File = isUpdateRequired ? Config.Instance.LauncherData : new byte[] { 0 },
            };

            Packet packet = Protocol.FormPacket(PacketType.ServerSendLauncherUpdate, launcher);
            Protocol.SendPacket(packet, client);
        }

        public static void SendClients(TcpClient client)
        {
            Packet packet = Protocol.FormPacket(PacketType.ServerSendClientsList, Config.Instance.Clients);
            Protocol.SendPacket(packet, client);
        }

        public static void SendClientsFiles(TcpClient client)
        {
            Packet packet = Protocol.FormPacket(PacketType.ServerSendClientsFiles, Config.Instance.ClientsFiles);
            Protocol.SendPacket(packet, client);
        }

        public static void SendFile(string filepath, TcpClient client)
        {
            string localFilepath = Path.Combine(Config.UpdatesFolder, filepath);

            //logger.Debug("Looking for file \"{0}\" to send to client {1}", localFilepath, ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
            if (!File.Exists(localFilepath))
            {
                logger.Error("No requested by client {1} file (\"{0}\") exist! Possible file system traverse attack!", localFilepath, ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                return;
            }

            if (Config.Instance.ClientsFiles.Files.Select(f => f.Filepath == filepath).ToArray().Length == 0)
            {
                logger.Error("No requested by client {1} file (\"{0}\") in clients files list! Possible file system traverse attack!", localFilepath, ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                return;
            }

            byte[] file = File.ReadAllBytes(localFilepath);
            FileData fileData = new FileData()
            {
                Filepath = filepath,
                FileSize = file.Length,
                File = file,
            };

            Packet packet = Protocol.FormPacket(PacketType.ServerSendClientFile, fileData);
            Protocol.SendPacket(packet, client);
        }
    }
}
