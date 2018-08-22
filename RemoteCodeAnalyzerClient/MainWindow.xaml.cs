using MessageService;
using RemoteCodeAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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

namespace RemoteCodeAnalyzerClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string address = "http://localhost:8080/MessageService";
        IMessageService channel;

        string authenticatedUser = null;
        Boolean isAuthenticated = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        /**
         * This method contains all the code initialization logic
         */
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //establish connection to backend service
            connectToService();

            initializeTabControl();

            initalizeAuthenticationTab();
        }

        /**
         * This method disables specific tabs until a user is authenticated
         * with a proper user login
         */
        private void initializeTabControl()
        {
            DownloadTab.IsEnabled = false;
            DirectoryTab.IsEnabled = false;
            UploadTab.IsEnabled = false;
            DownloadTab.IsEnabled = false;
            LoginTab.IsEnabled = false;
        }

        /**
         * Prep the authentication tab UI
         */
        private void initalizeAuthenticationTab()
        {
            messageLabel.Content = "";
        }

        /*
         * Event method to handle login when user enters a credential
         */
        private void Handle_User_Login(object sender, RoutedEventArgs e)
        {
            if (usernameTextbox.Text.ToString().Equals("") || usernameTextbox.Text.ToString().Equals("Username"))
            {
                messageLabel.Content = "Please enter a username";

            }
            else if (passwordTextbox.Text.ToString().Equals("") || passwordTextbox.Text.ToString().Equals("Password")) {
                messageLabel.Content = "Please enter a password";

            } else
            {
                //attempt to authenticate
                messageLabel.Content = "Authenticating user...";
                Message msg = new Message();
                msg.sourceAddress = address;
                msg.destinationAddress = address;
                msg.messageType = "AUTHENTICATION";
                msg.dateTime = DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt");
                msg.body = "<body>" +
                                "<username>"+usernameTextbox.Text+"</username>" +
                                "<password>"+passwordTextbox.Text+"</password>" +
                            "</body>";

                //messageLabel.Content = channel.getMessage();
                bool isValidUser = channel.authenticateUser(msg);
                if(isValidUser)
                {
                    //enable all tab controls to allow user access
                    messageLabel.Content = "User Successfully Authenticated!";
                    authenticatedUser = usernameTextbox.Text;
                    isAuthenticated = true;

                    DownloadTab.IsEnabled = true;
                    DirectoryTab.IsEnabled = true;
                    UploadTab.IsEnabled = true;
                    DownloadTab.IsEnabled = true;
                    LoginTab.IsEnabled = true;

                } else
                {
                    messageLabel.Content = "Error Authenticating User. Please enter valid credentials.";
                }
            }
        }

        /**
         * Connect to the wcf service to allow for various remote features.
         * After a success service connection, we can make request with the channel object.
         */
        private void connectToService()
        {
            EndpointAddress endpoint = new EndpointAddress(address);
            BasicHttpBinding binding = new BasicHttpBinding();
            channel = ChannelFactory<IMessageService>.CreateChannel(binding, endpoint);
        }
    }
}
