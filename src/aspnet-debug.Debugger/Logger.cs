using System.IO;

namespace aspnet_debug.Debugger
{
    public class Logger
    {
        internal static TextWriter Writer
        {
            get { return _logger ?? (_logger = new StreamWriter("C:\\temp\\debugger.log")); }
        }

        private static TextWriter _logger; 

        public static void Log(string message)
        {
            Writer.WriteLine(message);
            Writer.Flush();
        }
    }
}
