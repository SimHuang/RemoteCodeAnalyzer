///////////////////////////////////////////////////////////////////////
///  ServiceFileStream.cs -                                         ///
///                                                                 ///
///  Language:     Visual C#                                        ///
///  Platform:     Windows 10                                       ///
///  Application:  Remote Code Analyzer                             ///
///  Author:       Simon Huang shuang43@syr.edu                     ///
///////////////////////////////////////////////////////////////////////
/// Note:                                                           ///
/// Methods: downloadFile(), uploadFile()                           ///
///                                                                 ///
/// This class contains the service logic to handle the download and///    
/// upload of files.                                                ///
///////////////////////////////////////////////////////////////////////

using CodeAnalysis;
using MessageService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RemoteCodeAnalyzer.File
{
    public class ServerFileStream
    {
        //public static string filename;
        public static string savePath = "..\\..\\FileStorage";
        public static string fileStoragePath = "..\\..\\FileStorage"; 
        public static int BlockSize = 1024;
        public static byte[] block = new byte[BlockSize];

        /*
         * Service method to allow user to download a file
         */
        public static Stream downloadFile(string filename) {
            Console.Write("DOWNLOAD FILE");
            string sfilename = Path.Combine(fileStoragePath, filename);
            FileStream outStream = null;
            if (System.IO.File.Exists(sfilename))
            {
                outStream = new FileStream(sfilename, FileMode.Open);
                Console.Write("\n  Sending File \"{0}\"", filename);
            }
            else
                throw new Exception("open failed for \"" + filename + "\"");
            return outStream;
        }

        /*
         * Service method to allow user to upload a file
         */
        public static string uploadFile(FileTransferMessage msg)
        {
            Console.Write("UPLOAD FILE");
            HRTimer.HiResTimer hrt = new HRTimer.HiResTimer();
            hrt.Start();
            //savePath += "\\" + msg.username;
            string uploadPath = "";
            int count = -1;
            if(msg.uploadAsDirectory)
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(savePath + "\\" + msg.username);
                count = dir.GetDirectories().Length + 1;
                uploadPath = savePath + "\\" + msg.username + "\\" + count.ToString();
            }else
            {
                uploadPath = savePath + "\\" + msg.username;
            }
            string filename = msg.filename;
            string rfilename = Path.Combine(uploadPath, filename);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
            using (var outputStream = new FileStream(rfilename, FileMode.Create))
            {
                while (true)
                {
                    int bytesRead = msg.transferStream.Read(block, 0, BlockSize);
                    if (bytesRead > 0)
                        outputStream.Write(block, 0, bytesRead);
                    else
                        break;
                }
            }
            hrt.Stop();
            string xmlName = addToFileMetaData(msg.username, filename, count);
            Console.Write("\n  Received file \"{0}\"", filename);

            //can upload one one file at once
            string[] files = new string[2];
            string correctPath = "../../FileStorage/" + msg.username;
            if(msg.uploadAsDirectory)
            {
                correctPath += "/" + count;
            }
            files[0] = correctPath;
            files[1] = filename;
            calculateMaintainibility(files, xmlName);

            return "File successfully uploaded.";
        }

        /*
         * Add The file to the file meta data xml to keep track of it. Traverse 
         * the metadata file through upload path
         */
        private static string addToFileMetaData(string username, string filename, int isDirectory)
        {
            XDocument doc = XDocument.Load("../../File/file_metaData.xml");

            //store the property file
            Guid g = Guid.NewGuid();
            string GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("=", "");
            GuidString = GuidString.Replace("+", "");

            XElement userDir = doc.Element("Directories")
                            .Elements("Directory")
                            .Where(x => x.Attribute("name").Value.Equals(username)).FirstOrDefault();

            if(isDirectory > 0)
            {
                userDir.AddFirst(new XElement("Directory", new XAttribute("name", isDirectory.ToString()),
                                new XElement("File", new XAttribute("name", filename), new XElement("Property", GuidString))));
            }
            else
            {
                userDir.Add(new XElement("File", new XAttribute("name", filename), new XElement("Property", GuidString)));
            }
            doc.Save("../../File/file_metaData.xml");

            //create xml file in FileMaintainibility folder
            XDocument newPropertyFile = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes")
                );
            newPropertyFile.Add(new XElement("Properties"));
            newPropertyFile.Save("../../FileMaintainibility/" + GuidString + ".xml");

            return GuidString;
        }

        //helper method to calculate maintainibility index
        public static void calculateMaintainibility(string[] filePaths, string xmlName)
        {
            TestParser.calculateMaintainibilityIndex(filePaths, xmlName);
        }
    }
}
