using System.Runtime.Serialization;

namespace aspnet_debug.Shared.Communication
{
    [DataContract]
    public class ExecutionParameters: MessageBase
    {
        [DataMember]
        public string ProjectPath { get; set; }
        [DataMember]
        public string ExecutionCommand { get; set; }
    }

    [DataContract]
    public class MessageBase
    {
        [DataMember]
        public Command Command { get; set; }
        [DataMember]
        public object Payload { get; set; }
    }
}