using DeliveryService.Protocol.Messages;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Tests;

public class MapMessageTests
{
    [Theory]
    [InlineData(1, 1, new byte[] { 0x01 })]
    [InlineData(3, 3, new byte[] { 0b10110101, 0b00000001 })]
    [InlineData(4, 2, new byte[] { 0xFF })]
    [InlineData(5, 3, new byte[] { 0xFF, 0x7F })]
    [InlineData(2, 5, new byte[] { 0b01010101, 0b00000010 })]
    public void TryGetBitmaskAndGetBoolMatrix_RoundTripValidBitmask_ReturnsSameBitmask(byte width, byte height, byte[] bitmask)
    {
        // Arrange

        var message = new MapMessage
        {
            Width = width,
            Height = height,
            Bitmask = bitmask,
            StoragePoint = null!
        };

        // Act

        var boolMap = message.GetBoolMatrixFromBitmask();
        var isSuccessfullyConverted = MapMessage.TryGetBitmaskFromBoolMatrix(boolMap, out var resultBitmask);

        // Assert

        Assert.True(isSuccessfullyConverted);
        Assert.Equal(width, boolMap.GetLength(0));
        Assert.Equal(height, boolMap.GetLength(1));
        Assert.Equal(bitmask, resultBitmask);
    }

    [Fact]
    public void TryGetBitmask_NullBoolMap_ReturnsFalse()
    {
        // Act

        var isSuccessfullyConverted = MapMessage.TryGetBitmaskFromBoolMatrix(null!, out var bitmask);

        // Assert

        Assert.False(isSuccessfullyConverted);
        Assert.Null(bitmask);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 3)]
    [InlineData(3, 0)]
    public void TryGetBitmask_BoolMapWithZeroSide_ReturnsFalse(int width, int height)
    {
        // Arrange

        var boolMap = new bool[width, height];

        // Act

        var isSuccessfullyConverted = MapMessage.TryGetBitmaskFromBoolMatrix(boolMap, out var bitmask);

        // Assert

        Assert.False(isSuccessfullyConverted);
        Assert.Null(bitmask);
    }

    [Theory]
    [InlineData(new byte[] { 0x01, 0x01, 0x01, 0x00, 0x00 })]
    [InlineData(new byte[] { 0x03, 0x03, 0b10110101, 0b00000001, 0x02, 0x01 })]
    [InlineData(new byte[] { 0x02, 0x04, 0xFF, 0xAB, 0xCD })]
    [InlineData(new byte[] { 0x03, 0x05, 0xFF, 0x7F, 0x04, 0x02 })]
    public void TrySerializeAndTryParse_RoundTripValidMap_ReturnsSameMessage(byte[] payload)
    {
        // Arrange

        var height = payload[0];
        var width = payload[1];
        var bitmaskLength = (width * height + 7) / 8;
        var buffer = new byte[100];

        // Act

        var isSuccessfullyParsed = MapMessage.TryParse(payload, out var parsedMessage);
        var isSuccessfullySerialized = parsedMessage!.TrySerialize(buffer, out var written);

        // Assert

        Assert.True(isSuccessfullyParsed);
        Assert.Equal(height, parsedMessage.Height);
        Assert.Equal(width, parsedMessage.Width);
        Assert.Equal(payload[2..(2 + bitmaskLength)], parsedMessage.Bitmask);
        Assert.Equal(payload[2 + bitmaskLength], parsedMessage.StoragePoint.X);
        Assert.Equal(payload[2 + bitmaskLength + 1], parsedMessage.StoragePoint.Y);

        Assert.True(isSuccessfullySerialized);
        Assert.Equal(payload.Length, written);
        Assert.Equal(payload, buffer[..written]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void TryParse_PayloadTooSmall_ReturnsFalse(int payloadLength)
    {
        // Arrange

        var payload = new byte[payloadLength];

        // Act

        var isSuccessfullyParsed = MapMessage.TryParse(payload, out var message);

        // Assert

        Assert.False(isSuccessfullyParsed);
        Assert.Null(message);
    }

    [Theory]
    [InlineData(0x00, 0x03)]
    [InlineData(0x03, 0x00)]
    [InlineData(0x00, 0x00)]
    public void TryParse_ZeroWidthOrHeight_ReturnsFalse(byte height, byte width)
    {
        // Arrange

        var payload = new byte[] { height, width, 0x00, 0x00 };

        // Act

        var isSuccessfullyParsed = MapMessage.TryParse(payload, out var message);

        // Assert

        Assert.False(isSuccessfullyParsed);
        Assert.Null(message);
    }

    [Theory]
    [InlineData(100, 96)] // 9600
    [InlineData(255, 255)]
    public void TryParse_MapSizeGreaterThanMax_ReturnsFalse(byte height, byte width)
    {
        // Arrange

        var mapSize = height * width;
        var bitmaskLength = (mapSize + 7) / 8;
        var payload = new byte[2 + bitmaskLength + 2];
        payload[0] = height;
        payload[1] = width;

        // Act

        var isSuccessfullyParsed = MapMessage.TryParse(payload, out var message);

        // Assert

        Assert.False(isSuccessfullyParsed);
        Assert.Null(message);
    }

    [Theory]
    [InlineData(3, 3, 5)] // expected 6
    [InlineData(3, 3, 7)]
    [InlineData(1, 1, 4)] // expected 5
    [InlineData(1, 1, 6)]
    public void TryParse_PayloadLengthMismatch_ReturnsFalse(byte height, byte width, int payloadLength)
    {
        // Arrange

        var payload = new byte[payloadLength];
        payload[0] = height;
        payload[1] = width;

        // Act

        var isSuccessfullyParsed = MapMessage.TryParse(payload, out var message);

        // Assert

        Assert.False(isSuccessfullyParsed);
        Assert.Null(message);
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(3, 0)]
    [InlineData(0, 0)]
    public void TrySerialize_ZeroWidthOrHeight_ReturnsFalse(byte width, byte height)
    {
        // Arrange

        var message = new MapMessage
        {
            Width = width,
            Height = height,
            Bitmask = [],
            StoragePoint = null!
        };

        var buffer = new byte[100];

        // Act

        var isSuccessfullySerialized = message.TrySerialize(buffer, out var written);

        // Assert

        Assert.False(isSuccessfullySerialized);
        Assert.Equal(0, written);
    }

    [Theory]
    [InlineData(100, 96)]
    [InlineData(255, 255)]
    public void TrySerialize_MapSizeGreaterThanMax_ReturnsFalse(byte width, byte height)
    {
        // Arrange

        var message = new MapMessage
        {
            Width = width,
            Height = height,
            Bitmask = new byte[(width * height + 7) / 8],
            StoragePoint = null!
        };

        var buffer = new byte[10000];

        // Act

        var isSuccessfullySerialized = message.TrySerialize(buffer, out var written);

        // Assert

        Assert.False(isSuccessfullySerialized);
        Assert.Equal(0, written);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)] // expected 6
    public void TrySerialize_BufferTooSmall_ReturnsFalse(int bufferLength)
    {
        // Arrange

        var message = new MapMessage
        {
            Width = 3,
            Height = 3,
            Bitmask = new byte[2],
            StoragePoint = new Point<byte> { X = 1, Y = 2 }
        };

        var buffer = new byte[bufferLength];

        // Act

        var isSuccessfullySerialized = message.TrySerialize(buffer, out var written);

        // Assert

        Assert.False(isSuccessfullySerialized);
        Assert.Equal(0, written);
    }

    [Theory]
    [InlineData(3, 3, 1)] // expected 2
    [InlineData(3, 3, 3)]
    [InlineData(1, 1, 0)] // expected 1
    [InlineData(1, 1, 2)]
    public void TrySerialize_BitmaskLengthMismatch_ReturnsFalse(byte width, byte height, int bitmaskLength)
    {
        // Arrange

        var message = new MapMessage
        {
            Width = width,
            Height = height,
            Bitmask = new byte[bitmaskLength],
            StoragePoint = null!
        };

        var buffer = new byte[100];

        // Act

        var isSuccessfullySerialized = message.TrySerialize(buffer, out var written);

        // Assert

        Assert.False(isSuccessfullySerialized);
        Assert.Equal(0, written);
    }

    [Fact]
    public void GetBoolMatrix_SingleBitSetInBitmask_ReturnsSingleTrueAtExpectedPosition()
    {
        // Arrange

        var message = new MapMessage
        {
            Width = 5,
            Height = 3,
            Bitmask =
            [
                0x00,
                0x02
            ],
            StoragePoint = null!
        };

        // Act

        var boolMap = message.GetBoolMatrixFromBitmask();

        // Assert

        Assert.Equal(5, boolMap.GetLength(0));
        Assert.Equal(3, boolMap.GetLength(1));

        for (var x = 0; x < 5; x++)
        {
            for (var y = 0; y < 3; y++)
            {
                Assert.Equal(x == 4 && y == 1, boolMap[x, y]);
            }
        }
    }

    [Fact]
    public void TryGetBitmask_SingleTrueInBoolMatrix_ReturnsExpectedBytes()
    {
        // Arrange

        var boolMap = new bool[5, 3];
        boolMap[4, 1] = true;

        // Act

        var isSuccessfullyConverted = MapMessage.TryGetBitmaskFromBoolMatrix(boolMap, out var bitmask);

        // Assert

        Assert.True(isSuccessfullyConverted);
        Assert.Equal(new byte[] { 0x00, 0x02 }, bitmask);
    }
}