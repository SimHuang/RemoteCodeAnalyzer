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
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(Application.StartupPath, "user.xml"));

            //foreach(XmlNode node in doc.DocumentElement.ChildNodes)
            //{
              //  string username 
            //}
            return true;
        }
    }
}
