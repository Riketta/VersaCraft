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
        private static Logger logger = Logger.GetLogger();


        //static readonly string host = "versalita.net";
        static readonly string host = "127.0.0.1";
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
            logger.Info("Connecting to server");

            while (true)
            {
                try
                {
                    client = new TcpClient(host, Protocol.Port);
                    stream = client.GetStream();

                    await Task.Run(() =>
                    {
                        while (IsConnected())
                        {
                            byte[] packetBuffer = Protocol.ReceivePacket(stream);
                            Packet packet = Protocol.PacketDeserialize(packetBuffer);

                            logger.Info("Packet received: {0}", packet.Type.ToString());
                            PacketProcessing(packet, client);
                        }
                    });

                    logger.Info("Reconnecting to server");
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                }
            }
        }

        static void PacketProcessing(Packet packet, TcpClient client)
        {
            switch (packet.Type)
            {
                case PacketType.ServerSendLauncherUpdate:
                    FileData fileData = Protocol.DataDeserialize<FileData>(packet.Data);
                    UpdateManager.SelfUpdate(fileData.File);
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
        /// <param name="password"></param>
        /// <param name="hashed">true - password already hashed, use it directly</param>
        public static void RequestAuth(string session, string username, string password, bool hashed = false)
        {
            AuthData authData = new AuthData()
            {
                Session = session,
                Username = username,
                PassHash = hashed ? password : CryptoUtils.CalculateStringVersaHash(password),
            };

            Packet packet = Protocol.FormPacket(PacketType.LauncherRequestAuth, authData);
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