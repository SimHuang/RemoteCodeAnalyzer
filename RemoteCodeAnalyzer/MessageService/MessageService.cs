using RemoteCodeAnalyzer;
using RemoteCodeAnalyzer.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using System.Collections;
using System.Xml;
using RemoteCodeAnalyzer.File;

/**
 * This contains the main service implementation for client and server message passing. The 
 * message will be in the form of a message object. The message object will contain 
 * information on what type of request it may be (authentication, file upload, download, string passing)
 * and the server will handle the service accordingly.
 */
namespace MessageService
{
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.PerCall)]
    public class MessageService : IMessageService
    {
        /*
         * Request to retrieve a message from server
         */
        public string getMessage()
        {
            return "message sent from server";
        }

        /*
         * Request to send a string message to server
         */
        public void sendMessage(string msg)
        {
            Console.Write("Service receieved message {0}", msg);
        }

        /*
         * Request to authenticate a user
         */
        public bool authenticateUser(Message message)
        {
            Console.Write(message.dateTime + " " + message.author + " - " + message.messageType + "\n");
            return Authentication.authenticateUser(message);
            //return true;
        }

        /*
         * Download a file from server to client
         */
        public Stream downloadFile(string filename)
        {
            return ServerFileStream.downloadFile(filename);
        }

        /*
         * Upload a file from client to server
         */
        public void uploadFile(FileTransferMessage msg)
        {
            ServerFileStream.uploadFile(msg);
        }

        /*Retrieve a list of file names base on directory name
          The directory name is base on the username. We search for the 
          node with a matching name and get all files.
         */
        public ArrayList retrieveFiles(string directory)
        {
            ArrayList fileList = new ArrayList();
            XmlDocument doc = new XmlDocument();
            doc.Load("../../Authentication/user.xml");
            
            foreach(XmlNode node in doc.DocumentElement)
            {
                string actualDirectoryName = node["username"].InnerText;
                if(actualDirectoryName.Equals(directory))
                {
                    string[] files = node["files"].InnerText.ToString().Split('|');
                    fileList.AddRange(files);
                    break;
                }
            }

            return fileList;
        }

        /*
         * compare the file string in the message object to the author. If the author matches 
         * the file owner, then grant permission to the user of their choice.
         */
        public string grantFilePermission(Message message)
        {
            return FilePermission.grantFilePermission(message);
        }
    }
}
