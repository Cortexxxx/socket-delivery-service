using System.Diagnostics.CodeAnalysis;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol.Messages;

[Message(MessageType.OrderCreate, Flags = PacketFlags.NeedAck )]
public sealed class OrderCreateMessage : IMessage
{
    public byte X { get; init; }
    public byte Y { get; init; }
    
    public static bool TryParse(ReadOnlySpan<byte> payload, [NotNullWhen(true)] out OrderCreateMessage? message)
    {
        if (payload.Length != MessagesConstants.OrderCreatePayloadSize)
        {
            message = null;
            return false;
        }

        var x = payload[0];
        var y = payload[1];
            
        message = new OrderCreateMessage
        {
            X = x,
            Y = y
        };
        
        return true;
    }
    
    public bool TrySerialize(Span<byte> buffer, out int written)
    {
        written = 0;
        
        if (buffer.Length < MessagesConstants.OrderCreatePayloadSize)
        {
            return false;
        }

        buffer[0] = X;
        buffer[1] = Y;
        written = MessagesConstants.OrderCreatePayloadSize;
        
        return true;
    }
}