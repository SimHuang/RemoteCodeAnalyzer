using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCodeAnalyzer
{
    //Message Object use to send request to server
    public class Message
    {
        public string sourceAddress { get; set; }
        public string destinationAddress { get; set; }
        public string messageType { get; set; }
        public string author { get; set; }
        public string dateTime { get; set; }
        public string body { get; set; }
    }
}
