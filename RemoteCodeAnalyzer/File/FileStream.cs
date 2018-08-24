using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCodeAnalyzer.File
{
    public class FileStream
    {
        public static string filename;
        public static string savePath = "..\\SavedFiles";
        public static string ToSendPath = "..\\ToSend";
        public static int BlockSize = 1024;
        public static byte[] block = new byte[BlockSize];

        
    }
}
