using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol;

public class PacketCodec
{
    private readonly MessagesRegistry _registry;
    
    public PacketCodec(MessagesRegistry registry)
    {
        _registry = registry;
    }

    public bool TryParsePacket()
    {
        return false;
    }
    
    /// <summary>
    /// Serializes header and message to packet buffer
    /// </summary>
    public bool TrySerializePacket(Span<byte> buffer, ushort seq, IMessage message, out int written)
    {
        written = 0;
        
        if (buffer.Length < HeaderConstants.HeaderSize)
        {
            return false;
        }
        
        var tryGetMessageInfo = _registry.TryGetMessageInfo(message, 
            out (MessageType messageType, PacketFlags flags) messageInfo);

        if (!tryGetMessageInfo)
        {
            return false;
        }
        
        var bufferLimitLength = Math.Min(HeaderConstants.MaxPacketSize, buffer.Length);
        var trySerializePayload = message.TrySerialize(buffer[HeaderConstants.HeaderSize..bufferLimitLength], 
            out var payloadWritten);
        
        if (!trySerializePayload)
        {
            return false;
        }
        
        var trySerializeHeader = PacketHeader.TrySerialize(new PacketHeader
        {
            Seq = seq,
            Flags = messageInfo.flags,
            MessageType = messageInfo.messageType,
            PayloadLength = (ushort)payloadWritten
        }, buffer[..HeaderConstants.HeaderSize]);

        if (!trySerializeHeader)
        {
            return false;
        }
        
        written = HeaderConstants.HeaderSize + payloadWritten;
        return true;
    }
}