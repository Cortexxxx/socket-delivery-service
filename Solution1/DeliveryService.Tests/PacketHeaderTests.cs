using DeliveryService.Protocol;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Tests;

public class PacketHeaderTests
{
    private PacketHeader GetTestHeader(
        ushort seq = 0x1234, 
        PacketFlags flags = PacketFlags.NeedAck, 
        MessageType messageType = MessageType.OrderStatus, 
        ushort payloadLength = 0x0201)
    {
        return new PacketHeader
        {
            Seq = seq,
            Flags = flags,
            MessageType = messageType,
            PayloadLength = payloadLength
        };
    }

    private Span<byte> GetHeaderBytes(PacketHeader header)
    {
        var buffer = new byte[HeaderConstants.HeaderSize + header.PayloadLength];
        if (!PacketHeader.TrySerialize(header, buffer)) throw new Exception();
        return buffer;
    }

    [Fact]
    public void Good_PacketHeader_SerializesCorrectly()
    {   
        // Arrange
        
        var header = GetTestHeader();
        var buffer = new byte[2048];
        var awaitedBytesArray = new byte[] { 0x52, 0x44, 0x34, 0x12, 0x01, 0x41, 0x01, 0x02 };
        // Act

        var isSuccessful = PacketHeader.TrySerialize(header, buffer);
        
        // Assert
        
        Assert.True(isSuccessful);
        Assert.Equal(awaitedBytesArray, buffer[..8]);
    }
    
    [Theory]
    [InlineData(0x1234, PacketFlags.NeedAck, MessageType.Hello, 0x0201)]
    [InlineData(0x1234, PacketFlags.NeedAck, MessageType.Telemetry, HeaderConstants.MaxPayloadSize)]
    [InlineData(ushort.MaxValue, PacketFlags.NeedAck, MessageType.Telemetry, 0x0201)]
    [InlineData(ushort.MinValue, PacketFlags.NeedAck, MessageType.Telemetry, 0x0201)]
    [InlineData(0x1234, PacketFlags.NeedAck, MessageType.Telemetry, 0)]
    [InlineData(0x1234, PacketFlags.None, MessageType.Telemetry, 0x0201)]
    public void Good_PacketHeader_SerializesAndParsesCorrectly(ushort seq, PacketFlags flags, MessageType messageType, ushort payloadLength)
    {
        // Arrange
        
        var header = GetTestHeader(seq, flags, messageType, payloadLength);
        var buffer = new byte[2048];
        // Act
        
        var isSuccessfulSerialize = PacketHeader.TrySerialize(header, buffer);
        var isSuccessfulParse = PacketHeader.TryParse(buffer.AsSpan()[..(8 + header.PayloadLength)], out var headerResult);
        
        // Assert
        
        Assert.True(isSuccessfulSerialize);
        Assert.True(isSuccessfulParse);
        Assert.Equal(header, headerResult);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(HeaderConstants.HeaderSize - 1, 100)]
    [InlineData(2048, ushort.MaxValue)]
    [InlineData(2048, HeaderConstants.MaxPayloadSize + 1)]
    [InlineData(0, ushort.MaxValue)]
    public void Bad_PacketHeaderSerialization_ReturnFalse(ushort bufferSize, ushort payloadLength)
    {
        // Arrange
        
        var header = GetTestHeader(payloadLength: payloadLength);
        var buffer = new byte[bufferSize];

        // Act
        
        var isSuccessfulSerialize = PacketHeader.TrySerialize(header, buffer);
        
        // Assert
        
        Assert.False(isSuccessfulSerialize);
    }


    [Fact]
    public void Bad_PacketHeaderParseWithWrongMagic_ReturnFalse()
    {
        // Arrange
        
        var header = GetTestHeader();
        var buffer = GetHeaderBytes(header);
        
        buffer[0] = 0x00;
        
        // Act

        var isParseSuccessful = PacketHeader.TryParse(buffer, out _);
        
        // Assert
        
        Assert.False(isParseSuccessful);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(7)]
    public void Bad_PacketHeaderParseWithSmallLength_ReturnFalse(ushort bufferLength)
    {
        // Arrange
        
        var header = GetTestHeader();
        var buffer = GetHeaderBytes(header)[..bufferLength];
        
        // Act

        var isParseSuccessful = PacketHeader.TryParse(buffer, out _);
        
        // Assert
        
        Assert.False(isParseSuccessful);
    }
    
    [Fact]
    public void Bad_PacketHeaderParseWithLengthBiggerThanContent_ReturnFalse()
    {
        // Arrange
        var header = GetTestHeader();
        var buffer = GetHeaderBytes(header);
        buffer[6] = byte.MaxValue; // 767 > 513

        // Act
        var isParseSuccessful = PacketHeader.TryParse(buffer, out _);

        // Assert
        Assert.False(isParseSuccessful);
    }

    [Fact]
    public void Bad_PacketHeaderParseWithLengthSmallerThanContent_ReturnFalse()
    {
        // Arrange
        var header = GetTestHeader();
        var buffer = GetHeaderBytes(header);
        buffer[7] = 0x01; // 257 < 513

        // Act
        var isParseSuccessful = PacketHeader.TryParse(buffer, out _);

        // Assert
        Assert.False(isParseSuccessful);
    }
}