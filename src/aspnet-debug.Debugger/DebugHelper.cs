using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace aspnet_debug.Debugger
{
    public static class DebugHelper
    {
        //private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal static void TraceEnteringMethod([CallerMemberName] string callerMember = "")
        {
            MethodBase mth = new StackTrace().GetFrame(1).GetMethod();
            if (mth.ReflectedType != null)
            {
                string className = mth.ReflectedType.Name;
                Logger.Log(className + " (entering) :  " + callerMember);
            }
        }
    }
}