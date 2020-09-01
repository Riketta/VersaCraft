using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using VersaCraft.Logger;
using VersaCraft.Protocol;

namespace VersaCraft_Auth
{
    class Server
    {
        private static Logger logger = Logger.GetLogger();


        static TcpListener server = null;

        public static void Start()
        {
            logger.Info("Starting server using port {0}", Protocol.Port);
            server = new TcpListener(IPAddress.Any, Protocol.Port);
            server.Start();

            logger.Info("Waiting for clients...");
            while (true)
                _ = Accept(server.AcceptTcpClient());
        }

        public static void Stop()
        {
            logger.Info("Stopping server");
            server?.Stop();
        }

        static async Task Accept(TcpClient client)
        {
            await Task.Yield();

            logger.Info("New client: {0}", ((IPEndPoint)client.Client.RemoteEndPoint).ToString());

            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    while (client.Connected)
                    {
                        byte[] packetBuffer = Protocol.ReceivePacket(stream);
                        Packet packet = Protocol.PacketDeserialize(packetBuffer);

                        logger.Debug("Packet {0} received from client {1}", packet.Type.ToString(), ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                        PacketProcessing(packet, client);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Client {0} disconnected: {1}", ((IPEndPoint)client?.Client?.RemoteEndPoint).ToString(), ex.ToString());
            }
        }

        static void PacketProcessing(Packet packet, TcpClient client)
        {
            switch (packet.Type)
            {
                case PacketType.LauncherRequestAuth:
                    AuthData authData = Protocol.DataDeserialize<AuthData>(packet.Data);
                    ServerResponses.AcceptAuth(authData, client);
                    break;

                case PacketType.LauncherRequestLauncherUpdate:
                    string version = Protocol.DataDeserialize<string>(packet.Data);
                    ServerResponses.SendLauncher(version, client);
                    break;

                case PacketType.LauncherRequestClients:
                    ServerResponses.SendClients(client);
                    break;

                case PacketType.LauncherRequestClientsFiles:
                    ServerResponses.SendClientsFiles(client);
                    break;

                case PacketType.LauncherRequestClientFile:
                    string filepath = Protocol.DataDeserialize<string>(packet.Data);
                    ServerResponses.SendFile(filepath, client);
                    break;

                default:
                    logger.Warn("Unknown packet type ({0}) received from client {1}", (int)packet.Type, ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                    break;
            }
        }
    }
}
