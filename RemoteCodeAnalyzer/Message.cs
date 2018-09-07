///////////////////////////////////////////////////////////////////////
///  Message.cs -                                           ///
///                                                                 ///
///  Language:     Visual C#                                        ///
///  Platform:     Windows 10                                       ///
///  Application:  Remote Code Analyzer                             ///
///  Author:       Simon Huang shuang43@syr.edu                     ///
///////////////////////////////////////////////////////////////////////
/// Note:                                                           ///
///                                                                 ///
/// This contains the message class along with all of it's          ///
/// properties. The Message object is used to send objects to the   ///
/// server.                                                         ///
///////////////////////////////////////////////////////////////////////

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
