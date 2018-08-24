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
            string argumentUserName = message.author;
            string messageBody = message.body;
            messageBody = messageBody.Split(new string[] { "<password>" }, StringSplitOptions.None)[1];
            string messagePassword = messageBody.Replace("</password>", "").Replace("</body>", "").Trim();

            //XmlDocument doc = new XmlDocument();
            //doc.Load(Path.Combine(Application.StartupPath, "user.xml"));

            //try
            //{
            //    doc.Load("user.xml");

            //}
            //catch (XmlException exception)
            //{
            //    Console.Write(exception);
            //}


            //Console.Write("Loaded xml");



            //foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            //{
            //    string username = node.Attributes[0].InnerText;
            //    if (username.Equals(argumentUserName))
            //    {
            //        foreach (XmlNode child in node.ChildNodes)
            //        {
            //            Console.Write(child.Name);
            //            if (child.InnerText.Equals(password))
            //            {
            //                return true;
            //            }
            //        }
            //    }
            //}

            Dictionary<string, string> users = new Dictionary<string, string>();
            users.Add("samsmith", "123456");
            users.Add("john", "123456");
            users.Add("jane", "123456");

            if (users.ContainsKey(argumentUserName))
            {
                string valuePassword = "";
                if(users.TryGetValue(argumentUserName, out valuePassword))
                {
                    if(valuePassword.Equals(messagePassword))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
