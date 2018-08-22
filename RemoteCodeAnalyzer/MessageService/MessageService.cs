using RemoteCodeAnalyzer;
using RemoteCodeAnalyzer.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

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
            //return Authentication.authenticateUser(message);
            return true;
        }
    }
}
