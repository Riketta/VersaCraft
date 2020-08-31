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
            logger.Info("VersaCraft Launcher ver. {0}", Assembly.GetEntryAssembly().GetName().Version.ToString());

            InitializeComponent();
            StatusManager.SetStatusLabel(status);
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
            logger.Info("Requesting auth");
            Client.RequestAuth(username.Text, password.Password);
        }

        private void MainForm_Loaded(object sender, RoutedEventArgs e)
        {
            logger.Info("Connecting to server");
            Client.Connect();

            logger.Info("Waiting for connection");
            while (!Client.IsConnected()) { }

            logger.Info("Requesting launcher update");
            Client.RequestLauncherUpdate();
        }
    }
}
