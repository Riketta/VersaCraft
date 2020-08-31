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
            while (true)
            {
                logger.Info("Connecting");
                client = new TcpClient(host, Protocol.Port);
                stream = client.GetStream();

                await Task.Run(() =>
                {
                    try
                    {
                        while (IsConnected())
                        {
                            byte[] packetBuffer = Protocol.ReceivePacket(stream);
                            Packet packet = Protocol.PacketDeserialize(packetBuffer);

                            logger.Info("Packet received: {0}", packet.Type.ToString());
                            PacketProcessing(packet, client);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });

                logger.Info("Reconnecting");
            }
        }

        static void PacketProcessing(Packet packet, TcpClient client)
        {
            switch (packet.Type)
            {
                case PacketType.ServerSendLauncherUpdate:
                    logger.Info("Self updating");
                    FileData fileData = Protocol.DataDeserialize<FileData>(packet.Data);
                    UpdateManager.SelfUpdate(fileData.File);
                    break;

                case PacketType.ServerSendClientsList:
                    //string version = Protocol.DataDeserialize<string>(packet.Data);
                    break;

                case PacketType.ServerSendClientsFiles:

                    break;

                case PacketType.ServerSendClientFile:

                    break;

                default:
                    break;
            }
        }

        public static void RequestAuth(string username, string password)
        {
            AuthData authData = new AuthData()
            {
                Username = username,
                PassHash = CryptoUtils.CalculateStringVersaHash(password),
            };

            Packet packet = Protocol.FormPacket(PacketType.LauncherRequestAuth, authData);
            Protocol.SendPacket(packet, client);
        }

        public static void RequestLauncherUpdate()
        {
            Packet packet = Protocol.FormPacket(PacketType.LauncherRequestLauncherUpdate, Assembly.GetEntryAssembly().GetName().Version.ToString());
            Protocol.SendPacket(packet, client);
        }
    }
}