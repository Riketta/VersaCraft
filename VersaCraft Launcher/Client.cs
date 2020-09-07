using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VersaCraft.Logger;
using VersaCraft.Protocol;

namespace VersaCraft_Launcher
{
    class Client
    {
        private static readonly Logger logger = Logger.GetLogger();


        public enum ClientState
        {
            Connecting,
            Offline
        }

        public static ClientState State { get; private set; } = ClientState.Connecting;

        static readonly int AttemptLimit = 5;

        static TcpClient client = null;
        static NetworkStream stream = null;


        public Client()
        {
        }

        public static bool IsConnected()
        {
            return client != null && client.Connected;
        }

        public async static void Connect()
        {
            logger.Info("Connecting to server {0}:{1}", Config.Instance.Address, Protocol.Port);

            for (int attempt = 0; attempt < AttemptLimit; attempt++)
            {
                try
                {
                    State = ClientState.Connecting;
                    
                    client = new TcpClient(Config.Instance.Address, Protocol.Port);
                    stream = client.GetStream();

                    attempt = 0; // reset attempts if successfully connected

                    await Task.Run(() =>
                    {
                        while (IsConnected())
                        {
                            byte[] packetBuffer = Protocol.ReceivePacket(stream);
                            Packet packet = Protocol.PacketDeserialize(packetBuffer);

                            //logger.Debug("Packet received: {0}", packet.Type.ToString());
                            PacketProcessing(packet, client);
                        }
                    });
                }
                catch (Exception ex)
                {
                    logger.Info("Reconnecting to server with try #{0}", attempt + 1);
                    logger.Error(ex.ToString());
                }
            }

            logger.Warn("Working in offline mode");
            State = ClientState.Offline;
        }

        static void PacketProcessing(Packet packet, TcpClient client)
        {
            switch (packet.Type)
            {
                case PacketType.ServerSendLauncherUpdate:
                    FileData fileData = Protocol.DataDeserialize<FileData>(packet.Data);
                    UpdateManager.SelfUpdate(fileData);
                    break;

                case PacketType.ServerSendClientsList:
                    ClientsData clients = Protocol.DataDeserialize<ClientsData>(packet.Data);
                    Config.Instance.UpdateClients(clients);
                    break;

                case PacketType.ServerSendClientsFiles:
                    ClientsFilesData clientsFiles = Protocol.DataDeserialize<ClientsFilesData>(packet.Data);
                    Config.Instance.UpdateClientsFiles(clientsFiles); 
                    break;

                case PacketType.ServerSendClientFile:
                    fileData = Protocol.DataDeserialize<FileData>(packet.Data);
                    UpdateManager.SaveFile(fileData);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="username"></param>
        /// <param name="passHash">Already hashed password with <see cref="CryptoUtils.CalculateStringVersaHash"/>.</param>
        public static void SendAuth(string session, string username, string passHash)
        {
            AuthData authData = new AuthData()
            {
                Session = session,
                Username = username,
                PassHash = passHash,
            };

            Packet packet = Protocol.FormPacket(PacketType.LauncherSendAuth, authData);
            Protocol.SendPacket(packet, client);
        }

        public static void RequestLauncherUpdate()
        {
            Packet packet = Protocol.FormPacket(PacketType.LauncherRequestLauncherUpdate, Assembly.GetEntryAssembly().GetName().Version.ToString());
            Protocol.SendPacket(packet, client);
        }

        public static void RequestClients()
        {
            Packet packet = Protocol.FormPacket(PacketType.LauncherRequestClients, "");
            Protocol.SendPacket(packet, client);
        }

        public static void RequestClientsFiles()
        {
            Packet packet = Protocol.FormPacket(PacketType.LauncherRequestClientsFiles, "");
            Protocol.SendPacket(packet, client);
        }

        public static void RequestFile(string filepath)
        {
            Packet packet = Protocol.FormPacket(PacketType.LauncherRequestClientFile, filepath);
            Protocol.SendPacket(packet, client);
        }
    }
}