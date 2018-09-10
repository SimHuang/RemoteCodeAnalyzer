using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
///////////////////////////////////////////////////////////////////////
///  FilePermission.cs -                                            ///
///                                                                 ///
///  Language:     Visual C#                                        ///
///  Platform:     Windows 10                                       ///
///  Application:  Remote Code Analyzer                             ///
///  Author:       Simon Huang shuang43@syr.edu                     ///
///////////////////////////////////////////////////////////////////////
/// Note:                                                           ///
/// Method: grantFilePermission()                                   ///
///                                                                 ///
/// The method contains the logic to grant permission to other      ///
/// users to access other user's files. Itso allows the user to     ///
/// access their own file directory and admin can access all        ///
/// directories                                                     ///
///////////////////////////////////////////////////////////////////////

using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RemoteCodeAnalyzer.File
{
    class FilePermission
    {
        /*
         * File Owner can grant file/directory permission to another user
         */
        public static string grantFilePermission(Message message)
        {
            Console.Write(message.dateTime + " " + message.author + " " + message.messageType + "\n");
            if (message == null)
            {
                Console.Write("Error: Null Message Object.");
                return "Error granting file permission";
            }

            string currentUser = message.author;
            string[] body = message.body.Split('|');

            string file = body[1];
            string grantUser = body[0];

            //check if logged in user matches file owner
            XmlDocument doc = new XmlDocument();
            doc.Load("../../Authentication/user.xml");
            foreach (XmlNode node in doc.DocumentElement)
            {
                string fileOwner = node["username"].InnerText;
                if (fileOwner.Equals(currentUser))
                {
                    string[] files = node["files"].InnerText.ToString().Split('|');
                    for (int i = 0; i < files.Length; i++)
                    {
                        //found a matching file for the logged in user
                        if (files[i].Equals(file))
                        {

                            doc.Load("../../Authentication/user.xml");
                            //find user to grant permission to in user.xml
                            foreach (XmlNode grantNode in doc.DocumentElement)
                            {
                                string userToGrant = grantNode["username"].InnerText;
                                if (userToGrant.Equals(grantUser.ToLower()))
                                {
                                    string newPermissionFiles = grantNode["permission"].InnerText;
                                    newPermissionFiles += "|" + file;
                                    grantNode["permission"].InnerText = newPermissionFiles.TrimEnd('|').TrimStart('|');
                                    //doc.Save(Console.Out);
                                    FileStream fs = new FileStream("../../Authentication/user.xml", FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                                    fs.SetLength(0);
                                    doc.Save(fs);
                                    fs.Close();

                                    Console.Write("File Permission " + file + " granted to " + grantUser);
                                    return "File Permission " + file + " granted to " + grantUser;
                                }
                            }
                        }
                    }
                }
            }
            return "Error granting file permission";

        }

        /*
         * Retrieve the files and directories of user
         */
        public static ArrayList retrieveFiles(Message message)
        {
            string author = message.author;
            string directoryPath = message.body;
            string pathToRetrieve = "..\\..\\FileStorage\\" + directoryPath;
            ArrayList fileList = new ArrayList();
            XmlDocument doc = new XmlDocument();
            doc.Load("../../Authentication/user.xml");

            //get all files and directories under the specified paths
            //DirectoryInfo directoryFileInfo = new DirectoryInfo(pathToRetrieve);
            string[] filePath = Directory.GetFiles(pathToRetrieve);
            string[] directoryPaths = Directory.GetDirectories(pathToRetrieve);

            //get all directory names
            foreach (string dir in directoryPaths)
            {
                fileList.Add(Path.GetFileName(dir));
            }

            //get all file names
            foreach (string file in filePath)
            {
                fileList.Add(Path.GetFileName(file));
            }
         
            //foreach (XmlNode node in doc.DocumentElement)
            //{
            //    string actualDirectoryName = node["username"].InnerText;
            //    if (actualDirectoryName.Equals(directory))
            //    {
            //        string[] files = node["files"].InnerText.ToString().Split('|');
            //        fileList.AddRange(files);
            //        break;
            //    }
            //}

            return fileList;
        }
    }
}
