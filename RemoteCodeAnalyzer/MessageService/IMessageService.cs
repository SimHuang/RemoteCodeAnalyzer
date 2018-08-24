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
        bool authenticateUser(Message message);

        [OperationContract]
        Stream downloadFile(string filename);

        [OperationContract]
        void uploadFile(FileTransferMessage msg);

        [OperationContract]
        ArrayList retrieveFiles(string directory);
    }
}
