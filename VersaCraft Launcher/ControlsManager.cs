using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VersaCraft.Logger;
using VersaCraft.Protocol;

namespace VersaCraft_Launcher
{
    public class ControlsManager
    {
        private static Logger logger = Logger.GetLogger();


        static int filesRemaining = 0;

        static Label status = null;
        static ComboBox clients = null;
        static Button loginButton = null;

        public static void SetStatusLabel(Label label)
        {
            status = label;
        }

        public static void SetClientsComboBox(ComboBox comboBox)
        {
            clients = comboBox;
        }

        public static void SetLoginButton(Button login)
        {
            loginButton = login;
        }


        public static void SetStatus(string text)
        {
            if (status == null)
            {
                logger.Warn("Status label not assigned! Can't update.");
                return;
            }
            
            Application.Current.Dispatcher.Invoke(() => { status.Content = text; });
        }

        public static void UpdateClientsComboBox(ClientsData clientsData)
        {
            if (clients == null)
            {
                logger.Warn("Clients box not assigned! Can't update.");
                return;
            }
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                clients.ItemsSource = clientsData.Clients.Select(c => c.Name);
                clients.SelectedItem = clientsData.Clients.FirstOrDefault(c => c.Name == Config.Instance.SelectedClient).Name;
            });
        }

        public static string GetSelectedClientName()
        {
            if (clients == null)
            {
                logger.Warn("Clients box not assigned! Can't access selection.");
                return null;
            }

            string clientName = null;
            Application.Current.Dispatcher.Invoke(() => { clientName = (string)clients.SelectedItem; });
            
            return clientName;
        }

        public static void DisableLoginButton()
        {
            if (loginButton == null)
            {
                logger.Warn("Login button not assigned! Can't disable.");
                return;
            }
            
            Application.Current.Dispatcher.Invoke(() => { loginButton.IsEnabled = false; });
        }

        public static void EnableLoginButton(bool force = false)
        {
            if (!force) filesRemaining--;

            if (loginButton == null)
            {
                logger.Warn("Login button not assigned! Can't enable.");
                return;
            }

            if (filesRemaining == 0 || force)
                Application.Current.Dispatcher.Invoke(() => { loginButton.IsEnabled = true; });
        }

        internal static void EnableLoginButtonAfter(int fileCount)
        {
            if (fileCount > 0)
                filesRemaining = fileCount;
        }
    }
}
