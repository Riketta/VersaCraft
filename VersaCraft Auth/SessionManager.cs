﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using VersaCraft.Logger;
using VersaCraft.Protocol;

namespace VersaCraft_Auth
{
    class SessionManager
    {
        private static readonly Logger logger = Logger.GetLogger();


        private struct Session
        {
            public DateTime Expires;
            public TcpClient Client;
            public string Username;
            public string PassHash;
        }

        static readonly List<Session> sessions = new List<Session>();
        static readonly Timer cleanup = new Timer(Config.SessinCleanupInterval);

        public static void StartSessionCleaner()
        {
            logger.Info("Starting session cleaner with interval {0} ms", Config.SessinCleanupInterval);

            cleanup.Elapsed += Cleanup_Elapsed;
            cleanup.Start();
        }

        private static void Cleanup_Elapsed(object sender, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;
            int removedAmount = sessions.RemoveAll(s => s.Expires < now);
            if (removedAmount > 0)
                logger.Info("Sessions cleanup: removed {0} entries", removedAmount);
        }

        public static void AddSession(AuthData authData, TcpClient client)
        {
            IPAddress address = ((IPEndPoint)client.Client.RemoteEndPoint).Address;

            if (sessions.Count(s => ((IPEndPoint)s.Client.Client.RemoteEndPoint).Address.Equals(address)) >= Config.SessionsLimitPerAddress)
            {
                logger.Warn("Client {0} trying to exceed address per IP limitation! Rejecting his connection.", address.ToString());
                client.Close();
                
                return;
            }

            Session session = new Session()
            {
                Expires = DateTime.Now.AddSeconds(Config.SessionTTL),
                Client = client,
                Username = authData.Username,
                PassHash = authData.PassHash,
            };

            sessions.Add(session);
            logger.Info("Added session {3}: Client {0}; Username: \"{1}\"; PassHash: {2}", address, authData.Username, authData.PassHash, authData.Session);
        }
    }
}
