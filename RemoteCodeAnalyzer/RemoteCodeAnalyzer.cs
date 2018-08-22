using MessageService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

/**
 * This is the starting point of the server side application for the remote code analyzer.
 */
namespace RemoteCodeAnalyzer
{
    class RemoteCodeAnalyzer
    {
        static ServiceHost CreateChannel(string url)
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            Uri address = new Uri(url);
            Type service = typeof(MessageService.MessageService);
            ServiceHost host = new ServiceHost(service, address);
            host.AddServiceEndpoint(typeof(IMessageService), binding, address);
            return host;
        }

        static void Main(string[] args)
        {
            Console.Title = "Remote Code Analyzer Service Host";
            Console.Write("\n Starting Remote Code Analyzer Service");
            Console.Write("\n ======================================");

            ServiceHost host = null;
            try
            {
                host = CreateChannel("http://localhost:8080/MessageService");
                host.Open();
                Console.Write("\n Message Service Started - Press any key to exit: \n");
                Console.ReadKey();

            }catch(Exception ex)
            {
                Console.Write("\n\n {0}\n\n", ex.Message);
                return;
            }

            host.Close();
        }
    }
}
