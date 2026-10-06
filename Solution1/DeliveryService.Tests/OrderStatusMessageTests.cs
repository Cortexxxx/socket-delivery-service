using DeliveryService.Protocol;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Messages;

namespace DeliveryService.Tests;

public class OrderStatusMessageTests
{
    [Theory]
    [InlineData(0x01, 0x00, 0x00, 0x00, 0x00, OrderStatus.Pending, (ushort)0x0000)]
    [InlineData(0x04, 0xFF, 0xFF, 0xFF, 0xFF, OrderStatus.Delivered, (ushort)0xFFFF)]
    [InlineData(0x03, 0x34, 0x12, 0x05, 0xAB, OrderStatus.InTransit, (ushort)0x1234)]
    public void Good_OrderStatusMessageRoundTrip_ParsesSerializesAndReturnTrue(
        byte status, byte orderIdLow, byte orderIdHigh, byte x, byte y,
        OrderStatus expectedStatus, ushort expectedOrderId)
    {
        // Arrange

        var payload = new[] { status, orderIdLow, orderIdHigh, x, y };
        var buffer = new byte[100];

        // Act

        var isSuccessfullyParsed = OrderStatusMessage.TryParse(payload, out var parsedMessage);
        var isSuccessfullySerialized = parsedMessage!.TrySerialize(buffer, out var written);

        // Assert

        Assert.True(isSuccessfullyParsed);
        Assert.Equal(expectedStatus, parsedMessage.Status);
        Assert.Equal(expectedOrderId, parsedMessage.OrderId);
        Assert.Equal(x, parsedMessage.Endpoint.X);
        Assert.Equal(y, parsedMessage.Endpoint.Y);

        Assert.True(isSuccessfullySerialized);
        Assert.Equal(5, written);
        Assert.Equal(status, buffer[0]);
        Assert.Equal(orderIdLow, buffer[1]);
        Assert.Equal(orderIdHigh, buffer[2]);
        Assert.Equal(x, buffer[3]);
        Assert.Equal(y, buffer[4]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(6)]
    public void Bad_ParseInvalidPayloadLength_ReturnFalse(int payloadLength)
    {
        // Arrange

        var payload = new byte[payloadLength];

        // Act

        var isSuccessfullyParsed = OrderStatusMessage.TryParse(payload, out _);

        // Assert

        Assert.False(isSuccessfullyParsed);
    }

    [Theory]
    [InlineData(0x00)]
    [InlineData(0x05)]
    public void Bad_ParseUndefinedStatus_ReturnFalse(byte status)
    {
        // Arrange

        var payload = new byte[] { status, 0x00, 0x00, 0x00, 0x00 };

        // Act

        var isSuccessfullyParsed = OrderStatusMessage.TryParse(payload, out _);

        // Assert

        Assert.False(isSuccessfullyParsed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void Bad_SerializeSmallBufferLength_ReturnFalse(int bufferLength)
    {
        // Arrange

        var message = new OrderStatusMessage
        {
            Status = OrderStatus.Pending,
            OrderId = 0x1234,
            Endpoint = new Point<byte> { X = 0x05, Y = 0xAB }
        };

        var buffer = new byte[bufferLength];

        // Act

        var isSuccessfullySerialized = message.TrySerialize(buffer, out var written);

        // Assert

        Assert.False(isSuccessfullySerialized);
        Assert.Equal(0, written);
    }

    [Fact]
    public void Bad_SerializeUndefinedStatus_ReturnFalse()
    {
        // Arrange

        var message = new OrderStatusMessage
        {
            Status = (OrderStatus)0x05,
            OrderId = 0x1234,
            Endpoint = new Point<byte> { X = 0x05, Y = 0xAB }
        };

        var buffer = new byte[100];

        // Act

        var isSuccessfullySerialized = message.TrySerialize(buffer, out var written);

        // Assert

        Assert.False(isSuccessfullySerialized);
        Assert.Equal(0, written);
    }
}
