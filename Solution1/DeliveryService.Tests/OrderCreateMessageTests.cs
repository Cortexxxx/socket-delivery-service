using DeliveryService.Protocol.Messages;

namespace DeliveryService.Tests;

public class OrderCreateMessageTests
{
    [Theory]
    [InlineData(0x00, 0x00)]
    [InlineData(0xFF, 0xFF)]
    [InlineData(0x05, 0xAB)]
    public void Good_OrderCreateMessageRoundTrip_ParsesSerializesAndReturnTrue(byte x, byte y)
    {
        // Arrange

        var payload = new[] { x, y };
        var buffer = new byte[100];

        // Act

        var isSuccessfullyParsed = OrderCreateMessage.TryParse(payload, out var parsedMessage);
        var isSuccessfullySerialized = parsedMessage!.TrySerialize(buffer, out var written);

        // Assert

        Assert.True(isSuccessfullyParsed);
        Assert.Equal(x, parsedMessage.X);
        Assert.Equal(y, parsedMessage.Y);

        Assert.True(isSuccessfullySerialized);
        Assert.Equal(2, written);
        Assert.Equal(x, buffer[0]);
        Assert.Equal(y, buffer[1]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void Bad_PayloadLength_ReturnFalse(int payloadLength)
    {
        // Arrange

        var payload = new byte[payloadLength];

        // Act

        var isSuccessfullyParsed = OrderCreateMessage.TryParse(payload, out _);

        // Assert

        Assert.False(isSuccessfullyParsed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Bad_SmallBufferLength_ReturnFalse(int bufferLength)
    {
        // Arrange

        var message = new OrderCreateMessage
        {
            X = 0x05,
            Y = 0xAB
        };

        var buffer = new byte[bufferLength];

        // Act

        var isSuccessfullySerialized = message.TrySerialize(buffer, out var written);

        // Assert

        Assert.False(isSuccessfullySerialized);
        Assert.Equal(0, written);
    }
}
