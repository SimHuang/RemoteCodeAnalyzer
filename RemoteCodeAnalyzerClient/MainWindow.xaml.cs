///////////////////////////////////////////////////////////////////////
///  MainWindow.xaml.cs -                                           ///
///                                                                 ///
///  Language:     Visual C#                                        ///
///  Platform:     Windows 10                                       ///
///  Application:  Remote Code Analyzer                             ///
///  Author:       Simon Huang shuang43@syr.edu                     ///
///////////////////////////////////////////////////////////////////////
/// Note:                                                           ///
///                                                                 ///
/// This file handles for the client side logic for the             ///
/// MainWindow.xaml. All client event hadlers is stored here.       ///
///////////////////////////////////////////////////////////////////////

using MessageService;
using RemoteCodeAnalyzer;
using RemoteCodeAnalyzer.Authentication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        Boolean isAdmin = false;

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

            initializeDirectoryTab();
        }

        /**
         * This method disables specific tabs until a user is authenticated
         * with a proper user login
         */
        private void initializeTabControl()
        {   
            DirectoryTab.IsEnabled = false;
            UploadTab.IsEnabled = false;
            LoginTab.IsEnabled = false;
            FileTab.IsEnabled = false;
            AdminTab.IsEnabled = false;
        }

        /**
         * Prep the authentication tab UI
         */
        private void initalizeAuthenticationTab()
        {
            messageLabel.Content = "";
        }

        /**
         * Prep the directory tab
         */
        public void initializeDirectoryTab()
        {
            DirectoryMessage.Content = "";
        }

        /*
         * prepare the upload/download tab 
         */
        public void initializeUpDownTab()
        {
            FileActionLabel.Content = "";
        }

        /*
         * Prepare the file tab for use
         */ 
        public void initializeFilesTab()
        {
            FileMessageLabel.Content = "";
            ArrayList files = channel.retrieveFiles(authenticatedUser);

            List<File> fileList = new List<File>();
            foreach(String file in files)
            {
                fileList.Add(new File() { name = file, value = file });
            }
           
            //display files to user
            FileDropdown.ItemsSource = files;
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
                msg.author = usernameTextbox.Text;
                msg.dateTime = DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt");
                msg.body = "<body>" +
                                "<username>"+usernameTextbox.Text+"</username>" +
                                "<password>"+passwordTextbox.Text+"</password>" +
                            "</body>";

                //messageLabel.Content = channel.getMessage();
                string userAuthResponse = channel.authenticateUser(msg);
                if(userAuthResponse.Equals(Authentication.USER_AUTHENTICATED) || 
                    userAuthResponse.Equals(Authentication.ADMIN_AUTHENTICATED))
                {
                    //enable all tab controls to allow user access
                    messageLabel.Content = "User Successfully Authenticated!";
                    authenticatedUser = msg.author;
                    isAuthenticated = true;
                    
                    DirectoryTab.IsEnabled = true;
                    UploadTab.IsEnabled = true;
                    FileTab.IsEnabled = true;
                    LoginTab.IsEnabled = true;

                    //initialize some ui data for user
                    initializeFilesTab();

                    //enable admin tab 
                    if(userAuthResponse.Equals(Authentication.ADMIN_AUTHENTICATED))
                    {
                        AdminTab.IsEnabled = true;
                        isAdmin = true;
                    }

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
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxReceivedMessageSize = 50000000;
            channel = ChannelFactory<IMessageService>.CreateChannel(binding, endpoint);
        }

        /*
         * Event handler for directory tab when use wants to retrieve a directory by clicking the 
         * retrieve button
         */
        private void RetrieveButton_Click(object sender, RoutedEventArgs e)
        {
            string directoryName = DirectoryNameBox.Text;
            if(directoryName.Equals("") || 
                directoryName.Equals(" ") ||
                directoryName.Equals("Enter a Valid Directory Name")) {

                string errorMessage = "Please enter a valid directory name!";
                DirectoryNameBox.Text = "";
                DirectoryMessage.Content = errorMessage;
            }

            //proper name is enter search for the directory
            Message msg = new Message();
            msg.sourceAddress = address;
            msg.destinationAddress = address;
            msg.messageType = "FILE_RETRIEVE";
            msg.author = usernameTextbox.Text;
            msg.dateTime = DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt");
            msg.body = DirectoryNameBox.Text;

            ArrayList files = channel.retrieveFiles(msg.body);

            //display files to user
            DirectoryList.ItemsSource = files;
        }

        /*
         * Grant file permission to a specific user
         */
        private void grantPermissionButton_Click(object sender, RoutedEventArgs e)
        {
            string fileSelected = FileDropdown.Text;
            string userToGrant = grantPermissionUserTextBox.Text;

            if(fileSelected.Equals("") || userToGrant.Equals("")) {
                FileMessageLabel.Content = "Please fill out file and user input.";
                return;
            }

            Message msg = new Message();
            msg.sourceAddress = address;
            msg.destinationAddress = address;
            msg.messageType = "FILE_PERMISSION_GRANT";
            msg.author = authenticatedUser;
            msg.dateTime = DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt");
            msg.body = userToGrant + "|" + fileSelected;

            string responseMessage = channel.grantFilePermission(msg);
            FileMessageLabel.Content = responseMessage;
        }

        /*Download the specify file. The file name should match
         a existing file in the FileStorage folder.*/
        private void Download_File(object sender, RoutedEventArgs e)
        {
            string SavePath = "..\\..\\Download";
            string fileToDownload = FileActionTextBox.Text;
            int BlockSize = 1024;
            byte[] block = new byte[BlockSize];

            try
            {
                Stream strm = channel.downloadFile(fileToDownload);
                string rfilename = System.IO.Path.Combine(SavePath, fileToDownload);
                if (!Directory.Exists(SavePath))
                    Directory.CreateDirectory(SavePath);
                using (var outputStream = new FileStream(rfilename, FileMode.Create))
                {
                    while (true)
                    {
                        int bytesRead = strm.Read(block, 0, BlockSize);
                        if (bytesRead > 0)
                            outputStream.Write(block, 0, bytesRead);
                        else
                            break;
                    }
                }
                Console.Write("\n  Received file \"{0}\"", fileToDownload);
                FileActionLabel.Content = "\n  Received file " + fileToDownload;
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}\n", ex.Message);
                FileActionLabel.Content = ex.Message;
            }
        }

        /*
         * Upload a file chosen by user to be uploadded to server
         */
        private void Upload_File(object sender, RoutedEventArgs e)
        {
            string filename = FileActionTextBox.Text;
            string ToSendPath = "..\\..\\ToSend";

            Console.Write("\n  sending file \"{0}\"", filename);
            string fqname = System.IO.Path.Combine(ToSendPath, filename);
            using (var inputStream = new FileStream(fqname, FileMode.Open))
            {
                FileTransferMessage msg = new FileTransferMessage();
                msg.filename = filename;
                msg.transferStream = inputStream;
                channel.uploadFile(msg);
                FileActionLabel.Content = "File " + filename + " uploaded.";
            }
        }

        private void DirectoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //not used yet
        }

        /*
         * Click Handler for when user clicks create new user
         */
        private void New_User_Handler(object sender, RoutedEventArgs e)
        {
            string newUserName = NewUserName.Text;
            string newPassword = NewUserPassword.Text;
            string newPrivelege = NewUsePrivelege.Text;

            //call the new user service
            Message msg = new Message();
            msg.sourceAddress = address;
            msg.destinationAddress = address;
            msg.messageType = "NEW_USER_CREATION";
            msg.author = authenticatedUser;
            msg.dateTime = DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt");
            msg.body = newUserName + "|" + newPassword + "|" + newPrivelege;

            string response = channel.createNewUser(msg);
            NewUserMessage.Content = response;
        }
    }
}
