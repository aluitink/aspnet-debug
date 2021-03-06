﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using aspnet_debug.Shared.Logging;

namespace aspnet_debug.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Configure(new FileInfo("log4net.xml"), new DirectoryInfo("logs"));

            Log.Logger.Info("Server starting...");
            

            using (var server = new Shared.Server.Server())
            {
                server.Start();
                //Send/Receive files
                //Build
                //Build MDPs
                //Execute with debug agent
                //Attach
                //Relay
                server.WaitForExit();
            }
        }
    }
}
