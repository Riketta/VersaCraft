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
        private static readonly Logger logger = Logger.GetLogger();

        public static MainWindow MainForm { private get; set; } = null;
        public static Label StatusLabel { private get; set; } = null;
        public static Button LoginButton { private get; set; } = null;
        public static ComboBox ClientsComboBox { private get; set; } = null;

        public static void HideForm()
        {
            if (StatusLabel == null)
            {
                logger.Warn("Form assigned! Can't modify.");
                return;
            }

            Application.Current.Dispatcher.Invoke(() => { MainForm.WindowState = WindowState.Minimized; });
        }

        public static void SetStatus(string text)
        {
            if (StatusLabel == null)
            {
                logger.Warn("Status label not assigned! Can't update.");
                return;
            }
            
            Application.Current.Dispatcher.Invoke(() => { StatusLabel.Content = text; });
        }

        public static void UpdateClientsComboBox(ClientsData clientsData)
        {
            if (ClientsComboBox == null)
            {
                logger.Warn("Clients box not assigned! Can't update.");
                return;
            }
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                ClientsComboBox.ItemsSource = clientsData.Clients.Select(c => c.Name);
                ClientsComboBox.SelectedItem = clientsData.Clients.FirstOrDefault(c => c.Name == Config.Instance.SelectedClient).Name;
            });
        }

        public static string GetSelectedClientName()
        {
            if (ClientsComboBox == null)
            {
                logger.Warn("Clients box not assigned! Can't access selection.");
                return null;
            }

            string clientName = null;
            Application.Current.Dispatcher.Invoke(() => { clientName = (string)ClientsComboBox.SelectedItem; });
            
            return clientName;
        }

        public static void DisableLoginButton()
        {
            if (LoginButton == null)
            {
                logger.Warn("Login button not assigned! Can't disable.");
                return;
            }
            
            Application.Current.Dispatcher.Invoke(() => { LoginButton.IsEnabled = false; });
        }

        public static void EnableLoginButton()
        {
            if (LoginButton == null)
            {
                logger.Warn("Login button not assigned! Can't enable.");
                return;
            }

            Application.Current.Dispatcher.Invoke(() => { LoginButton.IsEnabled = true; });
        }

        public static void SetLoginButtonOffline()
        {
            if (LoginButton == null)
            {
                logger.Warn("Login button not assigned! Can't rename.");
                return;
            }

            Application.Current.Dispatcher.Invoke(() => { LoginButton.Content = "Offline"; });
        }
    }
}
