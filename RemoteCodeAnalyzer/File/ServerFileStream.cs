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
        public static void uploadFile(FileTransferMessage msg)
        {
            Console.Write("UPLOAD FILE");
            HRTimer.HiResTimer hrt = new HRTimer.HiResTimer();
            hrt.Start();
            string filename = msg.filename;
            string rfilename = Path.Combine(savePath, filename);
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);
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
        }
    }
}
