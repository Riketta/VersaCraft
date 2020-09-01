using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VersaCraft.Logger;
using VersaCraft.Protocol;
using static VersaCraft.Protocol.ClientsData;

namespace VersaCraft_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger logger = Logger.GetLogger();

        public MainWindow()
        {
            InitializeComponent();

            string version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            MainForm.Title = string.Format("{0} ver. {1}", MainForm.Title, version);
            logger.Info("VersaCraft Launcher ver. {0}", version);

            ControlsManager.SetClientsComboBox(clients);
            ControlsManager.SetStatusLabel(status);
            ControlsManager.SetLoginButton(login);
        }

        private void WebBrowser_Initialized(object sender, EventArgs e)
        {
            //((WebBrowser)sender).CanGoForward;
        }

        private void Username_TextChanged(object sender, TextChangedEventArgs e)
        {
            password.Password = "";
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string clientName = ControlsManager.GetSelectedClientName();
            if (string.IsNullOrEmpty(clientName))
            {
                logger.Error("Client not selected!");
                return;
            }

            logger.Info("Requesting client update");
            ClientsData.Client client = Config.Instance.Clients.Clients.First(c => c.Name == clientName);
            UpdateManager.UpdateClient(client);

            if (string.IsNullOrEmpty(username.Text) || string.IsNullOrEmpty(password.Password))
            {
                logger.Warn("No login data entered to request auth");
                return;
            }
            else
            {
                logger.Info("Requesting auth");
                Client.RequestAuth(username.Text, password.Password);
            }
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            logger.Info("Connecting to server");
            Client.Connect();

            logger.Info("Waiting for connection");
            while (!Client.IsConnected()) { }

            logger.Info("Requesting launcher update");
            Client.RequestLauncherUpdate();

            logger.Info("Requesting clients data");
            Client.RequestClients();

            logger.Info("Requesting clients files data");
            Client.RequestClientsFiles();
        }
    }
}
