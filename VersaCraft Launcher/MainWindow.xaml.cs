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

namespace VersaCraft_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Logger logger = Logger.GetLogger();

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
            Config.Instance.Load(); // TODO: not read config before launcher update?

            logger.Info("Preparing UI");
            username.Text = Config.Instance.Username;
            password.Password = Config.Instance.PassHash;
            isSavingPassword.IsChecked = Config.Instance.IsSavingPassword;
            ControlsManager.UpdateClientsComboBox(Config.Instance.Clients);
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
            //isSavingPassword.IsChecked = false;
        }

        private void Password_GotFocus(object sender, RoutedEventArgs e)
        {
            password.Dispatcher.BeginInvoke(new Action(delegate { password.SelectAll(); }), System.Windows.Threading.DispatcherPriority.Input);
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsConnected())
            {
                if (string.IsNullOrEmpty(username.Text) || string.IsNullOrEmpty(password.Password))
                {
                    logger.Warn("No login data entered to request auth");
                    ControlsManager.SetStatus("Enter login and password!");
                    return;
                }

                string clientName = ControlsManager.GetSelectedClientName();
                if (string.IsNullOrEmpty(clientName))
                {
                    logger.Error("Client not selected!");
                    ControlsManager.SetStatus("Select client!");
                    return;
                }

                ControlsManager.DisableLoginButton();
                logger.Info("Requesting client update");
                ClientsData.Client client = Config.Instance.Clients.Clients.First(c => c.Name == clientName);

                await UpdateManager.UpdateClient(client);
                logger.Info("Waiting client updates to finish");
                await Task.Run(() => { while (!UpdateManager.IsClientUpdateDone()) { } });
                logger.Info("Client updating done");

                logger.Info("Updating config with current login data");
                Config.Instance.Username = username.Text;

                string passHash = Config.Instance.PassHash == password.Password ? Config.Instance.PassHash : CryptoUtils.CalculateStringVersaHash(password.Password);
                Config.Instance.PassHash = passHash;

                string session = Anticheat.Session;

                logger.Info("Requesting auth");
                ControlsManager.SetStatus("Requesting auth...");
                Client.SendAuth(session, username.Text, passHash);

                logger.Info("Launching Minecraft");
                ControlsManager.SetStatus("Launching Minecraft...");
                Anticheat.HideLauncher(this);
                var minecraft = MinecraftLauncher.Start(username.Text, session, client.Server, client.Path);

                if (Config.Instance.WindowedFullscreen)
                    MinecraftLauncher.EnableWindowedFullscreen(minecraft);

                Anticheat.Protect();

                logger.Info("All jobs done");
                Application.Current.Shutdown();
            }
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            ControlsManager.DisableLoginButton();

            Task.Run(() =>
            {
                logger.Info("Connecting to server");
                ControlsManager.SetStatus("Connecting to auth&login server...");
                Client.Connect();

                logger.Info("Waiting for connection");
                ControlsManager.SetStatus("Waiting for connection...");
                while (!Client.IsConnected() && Client.State != Client.ClientState.Offline) { }

                if (Client.IsConnected())
                {
                    logger.Info("Requesting launcher update");
                    Client.RequestLauncherUpdate();

                    logger.Info("Waiting launcher to update");
                    while (!UpdateManager.IsLauncherUpToDate()) { }

                    logger.Info("Requesting clients data");
                    Client.RequestClients();

                    logger.Info("Requesting clients files data");
                    Client.RequestClientsFiles();
                }
                else
                    ControlsManager.SetLoginButtonOffline();

                ControlsManager.SetStatus("Ready");
                ControlsManager.EnableLoginButton();
            });
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
            
            string url = Config.Instance.Clients.Clients.FirstOrDefault(c => c.Name == Config.Instance.SelectedClient).URL;
            if (!string.IsNullOrEmpty(url))
                browser.Navigate(new Uri(url));
        }
    }
}
