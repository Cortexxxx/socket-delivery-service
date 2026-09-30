using System.Buffers.Binary;
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
        
        if (packet.Length < Constants.HeaderSize) return false;
        
        var isCorrectMagic = Constants.Magic[0] == packet[0] && Constants.Magic[1] == packet[1];
        
        if (!isCorrectMagic) return false;

        var contentLength = BinaryPrimitives.ReadUInt16LittleEndian(packet[Constants.PayloadLengthOffset
            ..(Constants.PayloadLengthOffset + 2)]);
        
        if (contentLength > Constants.MaxPayloadSize || contentLength + Constants.HeaderSize != packet.Length) return false;
        
        var seq = BinaryPrimitives.ReadUInt16LittleEndian(packet[Constants.SeqOffset..(Constants.SeqOffset + 2)]);
        var flags = (PacketFlags)packet[Constants.FlagsOffset];
        var type = (MessageType)packet[Constants.TypeOffset];
        
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
        if (buffer.Length < Constants.HeaderSize || packetHeader.PayloadLength > Constants.MaxPayloadSize) return false;
        
        buffer[0] = Constants.Magic[0];
        buffer[1] = Constants.Magic[1];
        
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(Constants.SeqOffset, 2), packetHeader.Seq);
        buffer[Constants.FlagsOffset] = (byte)packetHeader.Flags;        
        buffer[Constants.TypeOffset] = (byte)packetHeader.MessageType;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(Constants.PayloadLengthOffset, 2), packetHeader.PayloadLength);
        
        return true;
    }
}