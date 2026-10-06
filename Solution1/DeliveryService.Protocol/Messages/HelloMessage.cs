using System.Diagnostics.CodeAnalysis;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol.Messages;

[Message(MessageType.Hello, Flags = PacketFlags.NeedAck )]
public sealed class HelloMessage : IMessage
{
    public Role Role { get; init; }
    
    public static bool TryParse(ReadOnlySpan<byte> payload, [NotNullWhen(true)] out HelloMessage? message)
    {
        message = null;
        
        if (payload.Length != MessagesConstants.HelloPayloadSize)
        {
            return false;
        }
        
        var role = payload[0];

        if (!Enum.IsDefined((Role)role))
        {
            return false;
        }
        
        message = new HelloMessage
        {
            Role = (Role)role
        };
        
        return true;
    }
    
    public bool TrySerialize(Span<byte> buffer, out int written)
    {
        written = 0;
        
        if (buffer.Length < MessagesConstants.HelloPayloadSize || !Enum.IsDefined(Role))
        {
            return false;
        }
        
        buffer[0] = (byte)Role;
        written = MessagesConstants.HelloPayloadSize;
        
        return true;
    }
}