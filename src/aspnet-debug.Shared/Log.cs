using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;

namespace aspnet_debug.Shared
{
    public class Log
    {
        public static ILog Logger;

        private static readonly string _logRootDirKey = "logrootdir";

        public static void Configure(FileInfo configFile, DirectoryInfo directory)
        {
            if (Logger == null)
            {
                if (!directory.Exists)
                    directory.Create();

                GlobalContext.Properties[_logRootDirKey] = directory.FullName.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

                XmlConfigurator.ConfigureAndWatch(configFile);
                Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
