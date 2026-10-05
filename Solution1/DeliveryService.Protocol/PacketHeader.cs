using System.Buffers.Binary;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol;

public readonly record struct PacketHeader
{
    public required ushort Seq { get; init; }
    public required PacketFlags Flags { get; init; }
    public required MessageType MessageType { get; init; }

    public required ushort PayloadLength { get; init; }

    public static bool TryParse(ReadOnlySpan<byte> packet, out PacketHeader packetHeader)
    {
        packetHeader = default;
        
        if (packet.Length < HeaderConstants.HeaderSize) return false;
        
        var isCorrectMagic = HeaderConstants.Magic[0] == packet[0] && HeaderConstants.Magic[1] == packet[1];
        
        if (!isCorrectMagic) return false;

        var contentLength = BinaryPrimitives.ReadUInt16LittleEndian(packet[HeaderConstants.PayloadLengthOffset
            ..(HeaderConstants.PayloadLengthOffset + 2)]);
        
        if (contentLength > HeaderConstants.MaxPayloadSize || contentLength + HeaderConstants.HeaderSize != packet.Length) return false;
        
        var seq = BinaryPrimitives.ReadUInt16LittleEndian(packet[HeaderConstants.SeqOffset..(HeaderConstants.SeqOffset + 2)]);
        var flags = (PacketFlags)packet[HeaderConstants.FlagsOffset];
        var type = (MessageType)packet[HeaderConstants.TypeOffset];
        
        packetHeader = new PacketHeader
        {
            Seq = seq,
            Flags = flags,
            MessageType = type,
            PayloadLength = contentLength
        };
        
        return true;
    }

    public static bool TrySerialize(PacketHeader packetHeader, Span<byte> buffer)
    {
        if (buffer.Length < HeaderConstants.HeaderSize || packetHeader.PayloadLength > HeaderConstants.MaxPayloadSize) return false;
        
        buffer[0] = HeaderConstants.Magic[0];
        buffer[1] = HeaderConstants.Magic[1];
        
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(HeaderConstants.SeqOffset, 2), packetHeader.Seq);
        buffer[HeaderConstants.FlagsOffset] = (byte)packetHeader.Flags;        
        buffer[HeaderConstants.TypeOffset] = (byte)packetHeader.MessageType;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(HeaderConstants.PayloadLengthOffset, 2), packetHeader.PayloadLength);
        
        return true;
    }
}