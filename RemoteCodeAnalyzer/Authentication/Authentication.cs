///////////////////////////////////////////////////////////////////////
/// Authentication.cs -                                             ///
///                                                                 ///
///  Language:     Visual C#                                        ///
///  Platform:     Windows 10                                       ///
///  Application:  Remote Code Analyzer                             ///
///  Author:       Simon Huang shuang43@syr.edu                     ///
///////////////////////////////////////////////////////////////////////
/// Note:                                                           ///
/// Method: authenticationUser()                                    ///
///                                                                 ///
/// This file handles all the authentication logic for the          ///
/// remote code analyzer application. It will read from the user.xml///
/// to compare argument username, password to the actual actual xml ///
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace RemoteCodeAnalyzer.Authentication
{
    public class Authentication
    {
        public static string USER_AUTHENTICATED = "user_authenticated";
        public static string ADMIN_AUTHENTICATED = "admin_authenticated";
        public static string FAILED_AUTHENTICATION = "failed_authentication";

        //method to authenticate user
        public static string authenticateUser(Message message)
        {
            //parse message object for username and password
            string argumentUserName = message.author;
            string messageBody = message.body;
            messageBody = messageBody.Split(new string[] { "<password>" }, StringSplitOptions.None)[1];
            string messagePassword = messageBody.Replace("</password>", "").Replace("</body>", "").Trim();

            //parse xml for matching username and password
            XmlDocument doc = new XmlDocument();
            doc.Load("../../Authentication/user.xml");
            foreach(XmlNode node in doc.DocumentElement)
            {
                //match user
                string fileOwner = node["username"].InnerText;
                if(fileOwner.Equals(argumentUserName))
                {
                    //match password
                    string password = node["password"].InnerText;
                    if(password.Equals(messagePassword))
                    {
                        //check if user is admin
                        string privelege = node["privelege"].InnerText;
                        if(privelege.Equals("admin"))
                        {
                            return ADMIN_AUTHENTICATED;
                        }
                        return USER_AUTHENTICATED;
                    }
                }
            }

            return FAILED_AUTHENTICATION;
        }

        //method to create new user
        public static string createNewUser(Message message)
        {
            string messageBody = message.body;
            string[] parsedBody = messageBody.Split('|');
            string username = parsedBody[0];
            string password = parsedBody[1];
            string privelege = parsedBody[2];

            XDocument doc = XDocument.Load("../../Authentication/user.xml");
            XElement users = doc.Element("users");
            users.Add(new XElement("user",
                       new XElement("privelege", privelege),
                       new XElement("username", username),
                       new XElement("password", password),
                       new XElement("files", ""),
                       new XElement("permission", "")));
            doc.Save("../../Authentication/user.xml");

            //create folder for user
            System.IO.Directory.CreateDirectory("../../FileStorage/" + username);

            return "New User Successfuly Created!";
        }
    }
}
