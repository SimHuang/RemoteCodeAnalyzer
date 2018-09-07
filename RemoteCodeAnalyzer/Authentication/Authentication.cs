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

namespace RemoteCodeAnalyzer.Authentication
{
    public class Authentication
    {
        public static bool authenticateUser(Message message)
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
                        return true;
                    }
                }
            }

            //Dictionary<string, string> users = new Dictionary<string, string>();
            //users.Add("samsmith", "123456");
            //users.Add("john", "123456");
            //users.Add("jane", "123456");

            //if (users.ContainsKey(argumentUserName))
            //{
            //    string valuePassword = "";
            //    if(users.TryGetValue(argumentUserName, out valuePassword))
            //    {
            //        if(valuePassword.Equals(messagePassword))
            //        {
            //            return true;
            //        }
            //    }
            //}

            return false;
        }
    }
}
