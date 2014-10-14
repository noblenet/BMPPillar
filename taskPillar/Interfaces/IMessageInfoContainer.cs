using System;

namespace PillarAPI.Interfaces
{
    public interface IMessageInfoContainer
    {

        bool IsSerializedMessageValid { get; }
        string CorrelationId { get; }
        string Destination { get; }
        Type MessageType { get; }
        string MessageTypeName { get; }
        string SerializedMessage { get; }
        object MessageObject { get; }
        bool IsFromValidCollection { get; }
        string SignedMessage { get; }
    }
}