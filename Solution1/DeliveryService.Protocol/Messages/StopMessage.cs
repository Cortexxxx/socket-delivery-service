using System.Diagnostics.CodeAnalysis;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol.Messages;

[Message(MessageType.Stop, Flags = PacketFlags.NeedAck )]
public class StopMessage : IMessage
{
    public static bool TryParse(ReadOnlySpan<byte> payload, [NotNullWhen(true)] out StopMessage? message)
    {
        if (payload.Length != 0)
        {
            message = null;
            return false;
        }
        
        message = new StopMessage();
        
        return true;
    }
    
    public bool TrySerialize(Span<byte> buffer, out int written)
    {
        written = 0;
        return true;
    }
}