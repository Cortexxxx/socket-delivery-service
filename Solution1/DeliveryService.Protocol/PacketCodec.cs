using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Interfaces;

namespace DeliveryService.Protocol;

public class PacketCodec
{
    private readonly MessagesRegistry _registry;
    
    public PacketCodec(MessagesRegistry registry)
    {
        _registry = registry;
    }

    public bool TryParsePacket(ReadOnlySpan<byte> packetBytes, out Packet? packet, out ErrorCode? errorCode)
    {
        errorCode = null;
        packet = null;
        
        var tryParseHeader = PacketHeader.TryParse(packetBytes, out var packetHeader);
        
        if (!tryParseHeader)
        {
            return false;
        }
        
        var tryParsePayload = _registry.TryParsePayload(packetHeader.MessageType,
            packetBytes[HeaderConstants.HeaderSize..], 
            out var message, out errorCode);

        if (!tryParseHeader || !tryParsePayload || message == null)
        {
            return false;
        }
        
        packet = new Packet(packetHeader, message);
        
        return true;
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