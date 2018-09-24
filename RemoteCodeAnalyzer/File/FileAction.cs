using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RemoteCodeAnalyzer.File
{
    class FileAction
    {
        public static string AddFileComment(Message message)
        {
            Console.Write(message.dateTime + " " + message.author + " " + message.messageType + "\n");
            if (message == null)
            {
                Console.Write("Error: Null Message Object.");
                return "Error adding file cpmment";
            }

            string currentUser = message.author;
            string[] body = message.body.Split('|');

            string filepath = body[0]; //this contains the path to the directory/file
            string comment = body[1];

            string[] paths = filepath.Split('\\');

            //commenting on the users root directory
            if(paths.Length == 1)
            {
                //NO IMPLEMENTATION NEEDED. CANNOT COMMENT ON THE USERS ROOT DIRECTORY
            }
            //commenting on a file/directory within the root directory
            else if(paths.Length == 2)
            {
                XDocument doc = XDocument.Load("../../File/file_metaData.xml");
                XElement userDir = doc.Element("Directories")
                                .Elements("Directory")
                                .Where(x => x.Attribute("name").Value.Equals(paths[0])).FirstOrDefault();

                if(paths[1].Contains(".")) {
                    XElement userFile = userDir.Elements("File")
                        .Where(x => x.Attribute("name").Value.Equals(paths[1])).FirstOrDefault();

                    userFile.Add(new XElement("Comment", new XAttribute("commenter", currentUser), comment));
                    doc.Save("../../File/file_metaData.xml");
                }
                else
                {
                    XElement commentDir = userDir.Elements("Directory")
                        .Where(x => x.Attribute("name").Value.Equals(paths[1])).FirstOrDefault();
                    commentDir.Add(new XElement("Comment", new XAttribute("commenter", currentUser), comment));
                    doc.Save("../../File/file_metaData.xml");
                }
            }
            //commenting on a file in the third level
            else if(paths.Length == 3)
            {
                Console.Write(paths[2]);
                XDocument doc = XDocument.Load("../../File/file_metaData.xml");
                XElement userDir = doc.Element("Directories")
                                .Elements("Directory")
                                .Where(x => x.Attribute("name").Value.Equals(paths[0])).FirstOrDefault();

                XElement commentDir = userDir.Elements("Directory")
                        .Where(x => x.Attribute("name").Value.Equals(paths[1])).FirstOrDefault();

                XElement userFile = commentDir.Elements("File")
                        .Where(x => x.Attribute("name").Value.Equals(paths[2])).FirstOrDefault();

                userFile.Add(new XElement("Comment", new XAttribute("commenter", currentUser), comment));
                doc.Save("../../File/file_metaData.xml");
            }
          
            return "Comment successfully added to file";
        }

        //john\\3\\file
        public static ArrayList getComment(Message message)
        {
            Console.Write(message.dateTime + " " + message.author + " " + message.messageType + "\n");
            ArrayList commentList = new ArrayList();
            string author = message.author;
            string path = message.body;

            string[] heirarchy = path.Split('\\');
            XDocument doc = XDocument.Load("../../File/file_metaData.xml");
            XElement directoryNode = doc.Element("Directories");

            XElement userDirectory = directoryNode.Elements("Directory")
                    .Where(x => x.Attribute("name").Value.Equals(heirarchy[0])).FirstOrDefault();

            if (heirarchy.Length == 2)
            {
                XElement userFile = null;
                if (heirarchy[1].Contains(".")) {
                    userFile = userDirectory.Elements("File")
                    .Where(x => x.Attribute("name").Value.Equals(heirarchy[1])).FirstOrDefault();
                }else
                {
                    userFile = userDirectory.Elements("Directory")
                    .Where(x => x.Attribute("name").Value.Equals(heirarchy[1])).FirstOrDefault();
                }
                
                IEnumerable<XElement> comments = userFile.Elements();
                foreach(var element in comments)
                {
                    string nodeType = element.Name.ToString();
                    if(nodeType.Equals("Comment"))
                    {
                        string comment = element.Value;
                        commentList.Add(comment);
                    }
                }

            }
            else if(heirarchy.Length == 3)
            {
                XElement innerDirectory = userDirectory.Elements("Directory")
                    .Where(x => x.Attribute("name").Value.Equals(heirarchy[1])).FirstOrDefault();

                XElement userFile = innerDirectory.Elements("File")
                    .Where(x => x.Attribute("name").Value.Equals(heirarchy[2])).FirstOrDefault();

                IEnumerable<XElement> comments = userFile.Elements();
                foreach (var element in comments)
                {
                    string nodeType = element.Name.ToString();
                    if (nodeType.Equals("Comment"))
                    {
                        string comment = element.Value;
                        commentList.Add(comment);
                    }
                }
            }

            return commentList;
        }

        public static ArrayList getFileProperty(Message message)
        {
            ArrayList propertyList = new ArrayList();

            Console.Write(message.dateTime + " " + message.author + " " + message.messageType + "\n");
            ArrayList commentList = new ArrayList();
            string author = message.author;
            string path = message.body;
            string propertyFileName = "";

            string[] heirarchy = path.Split('\\');
            XDocument doc = XDocument.Load("../../File/file_metaData.xml");
            XElement directoryNode = doc.Element("Directories");

            XElement userDirectory = directoryNode.Elements("Directory")
                    .Where(x => x.Attribute("name").Value.Equals(heirarchy[0])).FirstOrDefault();

            if(heirarchy.Length == 2)
            {
                XElement userFile = userDirectory.Elements("File")
                    .Where(x => x.Attribute("name").Value.Equals(heirarchy[1])).FirstOrDefault();

                XElement propertyFile = userFile.Element("Property");
                propertyFileName = propertyFile.Value;

                //get all nodes in the property file
                XDocument propertyDoc = XDocument.Load("../../FileMaintainibility/" + propertyFileName + ".xml");
                XElement root = propertyDoc.Element("Properties");
                IEnumerable<XElement> fileProperties = root.Descendants();

                foreach (XElement node in fileProperties)
                {
                    propertyList.Add(node.Value);
                }

            }

            return propertyList;
        }
    }
}
