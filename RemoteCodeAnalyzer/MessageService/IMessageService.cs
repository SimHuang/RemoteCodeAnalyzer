///////////////////////////////////////////////////////////////////////
///  IMessageService.cs -                                           ///
///                                                                 ///
///  Language:     Visual C#                                        ///
///  Platform:     Windows 10                                       ///
///  Application:  Remote Code Analyzer                             ///
///  Author:       Simon Huang shuang43@syr.edu                     ///
///////////////////////////////////////////////////////////////////////
/// Note:                                                           ///
///                                                                 ///
/// IMessageService                                                 ///
///     This contains all the operation definitions that needs to   ///
///     be implemented by MessageService Class.                     ///
///                                                                 ///
/// FileTransferMessage                                             ///
///     This repersents the object file for upload.                 ///
///////////////////////////////////////////////////////////////////////

using RemoteCodeAnalyzer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MessageService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IMessageService" in both code and config file together.
    [ServiceContract(Namespace ="MessageService")]
    public interface IMessageService
    {
        [OperationContract]
        void sendMessage(string msg);

        [OperationContract]
        string getMessage();

        [OperationContract]
        string authenticateUser(Message message);

        [OperationContract]
        Stream downloadFile(string filename);

        [OperationContract]
        void uploadFile(FileTransferMessage msg);

        [OperationContract]
        ArrayList retrieveFiles(Message messgae);

        [OperationContract]
        ArrayList retrieveSharedFiles(Message message);

        [OperationContract]
        string grantFilePermission(Message message);

        [OperationContract]
        string createNewUser(Message message);

        [OperationContract]
        string addComment(Message message);

        [OperationContract]
        ArrayList getComment(Message message);

        //void calculateMaintainibility(string[] filePath);
    }

    [MessageContract]
    public class FileTransferMessage
    {
        [MessageHeader(MustUnderstand = true)]
        public string filename { get; set; }

        [MessageHeader(MustUnderstand =true)]
        public string username { get; set; }

        [MessageHeader(MustUnderstand = true)]
        public bool uploadAsDirectory { get; set; }

        [MessageBodyMember(Order = 1)]
        public Stream transferStream { get; set; }
    }
}
