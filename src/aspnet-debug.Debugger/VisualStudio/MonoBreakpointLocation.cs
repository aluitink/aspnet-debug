using Mono.Debugger.Soft;

namespace aspnet_debug.Debugger.VisualStudio
{
    internal class MonoBreakpointLocation
    {
        public MethodMirror Method { get; set; }
        public long Offset { get; set; }
    }
}