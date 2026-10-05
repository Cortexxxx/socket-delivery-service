using System.Diagnostics.CodeAnalysis;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol.Messages;

[Message(MessageType.Heartbeat, Flags = PacketFlags.None )]
public class HeartbeatMessage : IMessage
{
    public static bool TryParse(ReadOnlySpan<byte> payload, [NotNullWhen(true)] out HeartbeatMessage? message)
    {
        if (payload.Length != 0)
        {
            message = null;
            return false;
        }
        
        message = new HeartbeatMessage();
        
        return true;
    }
    
    public bool TrySerialize(Span<byte> buffer, out int written)
    {
        written = 0;
        return true;
    }
}