using RemoteCodeAnalyzer;
using System;
using System.Collections.Generic;
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
    }
}
