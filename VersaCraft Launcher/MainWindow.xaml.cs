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

            logger.Info("Loading config");
            Config.Instance.Load();

            logger.Info("Preparing UI");
            username.Text = Config.Instance.Username;
            password.Password = Config.Instance.PassHash;
            isSavingPassword.IsChecked = Config.Instance.IsSavingPassword;
            ControlsManager.UpdateClientsComboBox(Config.Instance.Clients);

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

        private void WebBrowser_Initialized(object sender, EventArgs e)
        {
            //((WebBrowser)sender).CanGoForward;
        }

        private void Username_TextChanged(object sender, TextChangedEventArgs e)
        {
            password.Password = "";
        }
        private void Username_GotFocus(object sender, RoutedEventArgs e)
        {
            username.Dispatcher.BeginInvoke(new Action(delegate { username.SelectAll(); }), System.Windows.Threading.DispatcherPriority.Input);
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            isSavingPassword.IsChecked = false;
        }

        private void Password_GotFocus(object sender, RoutedEventArgs e)
        {
            password.Dispatcher.BeginInvoke(new Action(delegate { password.SelectAll(); }), System.Windows.Threading.DispatcherPriority.Input);
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsConnected() && (string.IsNullOrEmpty(username.Text) || string.IsNullOrEmpty(password.Password)))
            {
                logger.Warn("No login data entered to request auth");
                return;
            }

            string clientName = ControlsManager.GetSelectedClientName();
            if (string.IsNullOrEmpty(clientName))
            {
                logger.Error("Client not selected!");
                return;
            }

            logger.Info("Requesting client update");
            ClientsData.Client client = Config.Instance.Clients.Clients.First(c => c.Name == clientName);
            UpdateManager.UpdateClient(client);

            logger.Info("Updating config with current login data");
            Config.Instance.Username = username.Text;
            if (string.IsNullOrEmpty(Config.Instance.PassHash)) // if we required to save hash and doing it first time (no currently password saved)
                Config.Instance.PassHash = CryptoUtils.CalculateStringVersaHash(password.Password);

            string session = CryptoUtils.CalculateStringSHA1(string.Format("{0}_{1}_{2}", Config.Instance.Username, DateTime.Now.Ticks.ToString(), CryptoUtils.GetComputerSid().ToString()));

            logger.Info("Requesting auth");
            Client.RequestAuth(session, username.Text, Config.Instance.IsSavingPassword ? Config.Instance.PassHash : password.Password, Config.Instance.IsSavingPassword);
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void IsSavingPassword_Checked(object sender, RoutedEventArgs e)
        {
            IsSavingPassword_Switched(sender, e);
        }

        private void IsSavingPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            IsSavingPassword_Switched(sender, e);
            password.Clear();
        }

        private void IsSavingPassword_Switched(object sender, RoutedEventArgs e)
        {
            var cb = ((CheckBox)sender).IsChecked;
            if (cb.HasValue)
                Config.Instance.IsSavingPassword = cb.Value; // that will also cleanup PassHash if required (if false)
        }

        private void Clients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Config.Instance.SelectedClient = (string)((ComboBox)sender).SelectedItem;
        }
    }
}
