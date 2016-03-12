using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using aspnet_debug.Shared.Client;
using aspnet_debug.Shared.Communication;
using aspnet_debug.Shared.Server;

namespace aspnet_debug.Client
{


    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press Enter to continue..");
            Console.ReadLine();

            Shared.Client.Client client = new Shared.Client.Client("127.0.0.1", Server.ServicePort);
            
            ExecutionParameters parameters = new ExecutionParameters();
            parameters.Payload = File.ReadAllBytes("ClassLibrary1.sln.zip");

            client.Send(parameters);
            
        }
    }
}
