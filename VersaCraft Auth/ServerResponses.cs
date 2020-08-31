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
    class ServerResponses
    {
        private static Logger logger = Logger.GetLogger();


        public static void AcceptAuth(AuthData authData, TcpClient client)
        {
            logger.Debug("Received auth data from {0}: \"{1}\" \"{2}\"", ((IPEndPoint)client.Client.RemoteEndPoint).ToString(), authData.Username, authData.PassHash);
        }

        public static void SendLauncher(string version, TcpClient client)
        {
            if (Config.Instance.LauncherVersion.CompareTo(version) <= 0)
                return;

            FileData launcher = new FileData()
            {
                Filepath = ".",
                FileSize = Config.Instance.LauncherData.Length,
                File = Config.Instance.LauncherData,
            };

            Packet packet = Protocol.FormPacket(PacketType.ServerSendLauncherUpdate, launcher);
            Protocol.SendPacket(packet, client);
        }
    }
}
