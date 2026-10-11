using DeliveryService.Protocol;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Messages;

namespace DeliveryService.Tests;

public class PacketCodecTests
{
    private readonly PacketCodec _codec = new(new MessagesRegistry(typeof(DriveMessage).Assembly));

    [Fact]
    public void TrySerializePacket_ValidMessage_ReturnsTrueAndExpectedBytes()
    {
        // Arrange

        var message = new DriveMessage { Speed = 10, Rotate = -5 };
        var buffer = new byte[HeaderConstants.HeaderSize + 2];
        byte[] expected = [0x52, 0x44, 0x34, 0x12, 0x00, 0x20, 0x02, 0x00, 0x0A, 0xFB];

        // Act

        var result = _codec.TrySerializePacket(buffer, 0x1234, message, out var written);

        // Assert

        Assert.True(result);
        Assert.Equal(expected.Length, written);
        Assert.Equal(expected, buffer);
    }

    [Fact]
    public void TrySerializePacket_BufferShorterThanHeader_ReturnsFalse()
    {
        // Arrange

        var message = new DriveMessage { Speed = 10, Rotate = -5 };
        var buffer = new byte[HeaderConstants.HeaderSize - 1];

        // Act

        var result = _codec.TrySerializePacket(buffer, 1, message, out var written);

        // Assert

        Assert.False(result);
        Assert.Equal(0, written);
    }

    [Fact]
    public void TrySerializePacket_MessageNotInRegistry_ReturnsFalse()
    {
        // Arrange

        var message = new UnknownMessage();
        var buffer = new byte[HeaderConstants.MaxPacketSize];

        // Act

        var result = _codec.TrySerializePacket(buffer, 1, message, out var written);

        // Assert

        Assert.False(result);
        Assert.Equal(0, written);
    }

    [Fact]
    public void TrySerializePacket_PayloadExceedsMaxPayloadSize_ReturnsFalse()
    {
        // Arrange

        var codec = new PacketCodec(new MessagesRegistry(typeof(ConfigurablePayloadMessage).Assembly));
        var message = new ConfigurablePayloadMessage(HeaderConstants.MaxPayloadSize + 1);
        var buffer = new byte[2048];

        // Act

        var result = codec.TrySerializePacket(buffer, 1, message, out var written);

        // Assert

        Assert.False(result);
        Assert.Equal(0, written);
    }

    [Fact]
    public void TrySerializePacket_PayloadEqualsMaxPayloadSize_ReturnsTrueAndMaxPacketSize()
    {
        // Arrange

        var codec = new PacketCodec(new MessagesRegistry(typeof(ConfigurablePayloadMessage).Assembly));
        var message = new ConfigurablePayloadMessage(HeaderConstants.MaxPayloadSize);
        var buffer = new byte[2048];

        // Act

        var result = codec.TrySerializePacket(buffer, 1, message, out var written);

        // Assert

        Assert.True(result);
        Assert.Equal(HeaderConstants.MaxPacketSize, written);
        Assert.Equal(1200, written);
    }

    [Fact]
    public void TryParsePacket_ValidPacket_ReturnsTrueAndParsedPacket()
    {
        // Arrange

        byte[] packetBytes = [0x52, 0x44, 0x34, 0x12, 0x00, 0x20, 0x02, 0x00, 0x0A, 0xFB];

        // Act

        var result = _codec.TryParsePacket(packetBytes, out var packet, out var errorCode);

        // Assert

        Assert.True(result);
        Assert.Null(errorCode);
        Assert.NotNull(packet);
        Assert.Equal(0x1234, packet.PacketHeader.Seq);
        Assert.Equal(PacketFlags.None, packet.PacketHeader.Flags);
        Assert.Equal(MessageType.Drive, packet.PacketHeader.MessageType);
        Assert.Equal(2, packet.PacketHeader.PayloadLength);
        var message = Assert.IsType<DriveMessage>(packet.Message);
        Assert.Equal(10, message.Speed);
        Assert.Equal(-5, message.Rotate);
    }

    [Fact]
    public void TryParsePacket_InvalidHeader_ReturnsFalse()
    {
        // Arrange

        // Wrong magic bytes
        byte[] packetBytes = [0x00, 0x00, 0x34, 0x12, 0x00, 0x20, 0x02, 0x00, 0x0A, 0xFB];

        // Act

        var result = _codec.TryParsePacket(packetBytes, out var packet, out var errorCode);

        // Assert
        Assert.NotNull(errorCode);
        Assert.False(result);
        Assert.Null(packet);
    }

    [Fact]
    public void TryParsePacket_InvalidPayload_ReturnsFalseWithBadPayloadError()
    {
        // Arrange

        // Drive payload must be 2 bytes, here it is 3
        byte[] packetBytes = [0x52, 0x44, 0x34, 0x12, 0x00, 0x20, 0x03, 0x00, 0x0A, 0xFB, 0x01];

        // Act

        var result = _codec.TryParsePacket(packetBytes, out var packet, out var errorCode);

        // Assert

        Assert.False(result);
        Assert.Null(packet);
        Assert.Equal(ErrorCode.BadPayload, errorCode);
    }
}
