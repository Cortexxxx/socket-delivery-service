using DeliveryService.Protocol;
using DeliveryService.Protocol.Messages;

namespace DeliveryService.Tests;

public class TelemetryMessageTests
{
    [Theory]
    [InlineData(
        new byte[] { 0x00, 0x00, 0x00, 0x00 },
        new byte[] { 0x00, 0x00, 0x00, 0x00 },
        new byte[] { 0x00, 0x00, 0x00, 0x00 },
        0f, 0f, 0f)]
    [InlineData(
        new byte[] { 0x00, 0x00, 0x80, 0x3F },
        new byte[] { 0x00, 0x00, 0x80, 0xBF },
        new byte[] { 0x00, 0x00, 0x00, 0x40 },
        1f, -1f, 2f)]
    [InlineData(
        new byte[] { 0x00, 0x00, 0x00, 0x3F },
        new byte[] { 0x00, 0x00, 0x00, 0x40 },
        new byte[] { 0x00, 0x00, 0x80, 0xBF },
        0.5f, 2f, -1f)]
    public void Good_TelemetryMessageRoundTrip_ParsesSerializesAndReturnTrue(
        byte[] xBytes, byte[] yBytes, byte[] angleBytes,
        float expectedX, float expectedY, float expectedAngle)
    {
        // Arrange

        var payload = new byte[12];
        xBytes.CopyTo(payload, 0);
        yBytes.CopyTo(payload, 4);
        angleBytes.CopyTo(payload, 8);  

        var buffer = new byte[100];

        // Act

        var isSuccessfullyParsed = TelemetryMessage.TryParse(payload, out var parsedMessage);
        var isSuccessfullySerialized = parsedMessage!.TrySerialize(buffer, out var written);

        // Assert

        Assert.True(isSuccessfullyParsed);
        Assert.Equal(expectedX, parsedMessage.Position.X);
        Assert.Equal(expectedY, parsedMessage.Position.Y);
        Assert.Equal(expectedAngle, parsedMessage.Angle);

        Assert.True(isSuccessfullySerialized);
        Assert.Equal(12, written);
        Assert.Equal(xBytes, buffer[..4]);
        Assert.Equal(yBytes, buffer[4..8]);
        Assert.Equal(angleBytes, buffer[8..12]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(13)]
    public void Bad_ParseInvalidPayloadLength_ReturnFalse(int payloadLength)
    {
        // Arrange

        var payload = new byte[payloadLength];

        // Act

        var isSuccessfullyParsed = TelemetryMessage.TryParse(payload, out _);

        // Assert

        Assert.False(isSuccessfullyParsed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Bad_SerializeSmallBufferLength_ReturnFalse(int bufferLength)
    {
        // Arrange

        var message = new TelemetryMessage
        {
            Position = new Point<float> { X = 1f, Y = -1f },
            Angle = 2f
        };

        var buffer = new byte[bufferLength];

        // Act

        var isSuccessfullySerialized = message.TrySerialize(buffer, out var written);

        // Assert

        Assert.False(isSuccessfullySerialized);
        Assert.Equal(0, written);
    }

    [Theory]
    [InlineData(new byte[] { 0x00, 0x00, 0xC0, 0xFF }, new byte[] { 0x00, 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x00, 0x00 })] // X = NaN
    [InlineData(new byte[] { 0x00, 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x80, 0x7F }, new byte[] { 0x00, 0x00, 0x00, 0x00 })] // Y = +Infinity
    [InlineData(new byte[] { 0x00, 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x80, 0xFF })] // Angle = -Infinity
    public void Bad_ParseNonFiniteValue_ReturnFalse(byte[] xBytes, byte[] yBytes, byte[] angleBytes)
    {
        // Arrange

        var payload = new byte[12];
        xBytes.CopyTo(payload, 0);
        yBytes.CopyTo(payload, 4);
        angleBytes.CopyTo(payload, 8);

        // Act

        var isSuccessfullyParsed = TelemetryMessage.TryParse(payload, out _);

        // Assert

        Assert.False(isSuccessfullyParsed);
    }

    [Theory]
    [InlineData(float.NaN, 0f, 0f)]
    [InlineData(0f, float.PositiveInfinity, 0f)]
    [InlineData(0f, 0f, float.NegativeInfinity)]
    public void Bad_SerializeNonFiniteValue_ReturnFalse(float x, float y, float angle)
    {
        // Arrange

        var message = new TelemetryMessage
        {
            Position = new Point<float> { X = x, Y = y },
            Angle = angle
        };

        var buffer = new byte[100];

        // Act

        var isSuccessfullySerialized = message.TrySerialize(buffer, out var written);

        // Assert

        Assert.False(isSuccessfullySerialized);
        Assert.Equal(0, written);
    }
}
