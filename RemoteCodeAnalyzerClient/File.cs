///////////////////////////////////////////////////////////////////////
///  File.cs -                                                      ///
///                                                                 ///
///  Language:     Visual C#                                        ///
///  Platform:     Windows 10                                       ///
///  Application:  Remote Code Analyzer                             ///
///  Author:       Simon Huang shuang43@syr.edu                     ///
///////////////////////////////////////////////////////////////////////
/// Note:                                                           ///
///                                                                 ///
/// This File Object is reponsible for contructing the objects      /// 
/// used for the Filed Combobox on the client side.                 ///
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCodeAnalyzerClient
{
    class File
    {
        public string name { get; set; }
        public string value { get; set; }
    }
}
