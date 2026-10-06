using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol.Messages;

[Message(MessageType.Ack, Flags = PacketFlags.None )]
public sealed class AckMessage : IMessage
{
    public ushort Seq { get; init; }
    
    public static bool TryParse(ReadOnlySpan<byte> payload, [NotNullWhen(true)] out AckMessage? message)
    {
        if (payload.Length != MessagesConstants.AckPayloadSize)
        {
            message = null;
            return false;
        }

        var seq = BinaryPrimitives.ReadUInt16LittleEndian(payload);
            
        message = new AckMessage
        {
            Seq = seq
        };
        
        return true;
    }
    
    public bool TrySerialize(Span<byte> buffer, out int written)
    {
        written = 0;
        
        if (buffer.Length < MessagesConstants.AckPayloadSize)
        {
            return false;
        }

        BinaryPrimitives.WriteUInt16LittleEndian(buffer[..MessagesConstants.AckPayloadSize], Seq);
        written = MessagesConstants.AckPayloadSize;
        
        return true;
    }
}