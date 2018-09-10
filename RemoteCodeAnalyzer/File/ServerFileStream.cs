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

using MessageService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if(msg.uploadAsDirectory)
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(savePath + "\\" + msg.username);
                int count = dir.GetDirectories().Length + 1;
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
            Console.Write("\n  Received file \"{0}\"", filename);

            return "File successfully uploaded.";
        }
    }
}
