using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using aspnet_debug.Shared.Client;
using aspnet_debug.Shared.Server;

namespace aspnet_debug.Client
{


    class Program
    {
        static void Main(string[] args)
        {
            Shared.Client.Client client = new Shared.Client.Client("192.168.1.15", Server.ServicePort);
            

            
        }
    }
}
