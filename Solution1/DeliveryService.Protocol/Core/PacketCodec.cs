using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Messages;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Protocol.Core;

public class PacketCodec(MessagesRegistry registry)
{
    public bool TryParsePacket(ReadOnlySpan<byte> packetBytes, 
        out Packet? packet, out ErrorCode? errorCode)
    {
        errorCode = null;
        packet = null;
        
        
        if (!PacketHeader.TryParse(packetBytes, out var packetHeader))
        {
            return false;
        }
        
        if (!registry.TryParsePayload(packetHeader.MessageType, packetBytes[HeaderConstants.HeaderSize..], 
                out var message, out errorCode) 
            || message == null) return false;
        
        packet = new Packet(packetHeader, message);
        
        return true;
    }
    
    public bool TrySerializePacket(Span<byte> buffer, ushort seq, IMessage message, out int written)
    {
        written = 0;
        
        if (buffer.Length < HeaderConstants.HeaderSize) return false;
        
        if (!registry.TryGetMessageInfo(message, 
                out (MessageType messageType, PacketFlags flags) messageInfo)) return false;
        
        var bufferLimitLength = Math.Min(HeaderConstants.MaxPacketSize, buffer.Length);
        
        var trySerializePayload = message.TrySerialize(
            buffer[HeaderConstants.HeaderSize..bufferLimitLength], 
            out var payloadWritten);
        
        if (!trySerializePayload) return false;
        
        var trySerializeHeader = PacketHeader.TrySerialize(new PacketHeader
        {
            Seq = seq,
            Flags = messageInfo.flags,
            MessageType = messageInfo.messageType,
            PayloadLength = (ushort)payloadWritten
        }, buffer[..HeaderConstants.HeaderSize]);

        if (!trySerializeHeader) return false;
        
        written = HeaderConstants.HeaderSize + payloadWritten;
        return true;
    }
}