using System.Runtime.Serialization;

namespace aspnet_debug.Shared.Communication
{
    [DataContract]
    public enum Command : byte
    {
        [EnumMember]
        DebugContent,
        [EnumMember]
        Started,
        [EnumMember]
        Stopped
    }
}