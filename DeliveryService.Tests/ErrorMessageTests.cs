using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Messages;

namespace DeliveryService.Tests;

public class ErrorMessageTests
{
    [Theory]
    [InlineData(ErrorCode.UnknownMessage)]
    [InlineData(ErrorCode.BadPayload)]
    [InlineData(ErrorCode.NotRegistered)]
    [InlineData(ErrorCode.RoleTaken)]
    [InlineData(ErrorCode.Forbidden)]
    [InlineData(ErrorCode.InvalidPoint)]
    [InlineData(ErrorCode.RobotOffline)]
    public void TrySerializeAndTryParse_RoundTripAnyErrorCode_ReturnsSameMessage(ErrorCode errorCode)
    {
        // Arrange

        var message = new ErrorMessage
        {
            ErrorCode = errorCode
        };

        var buffer = new byte[100];
        var payload = new[] { (byte)errorCode };

        // Act

        var isSuccessfullySerialized = message.TrySerialize(buffer, out var written);
        var isSuccessfullyParsed = ErrorMessage.TryParse(payload, out var parsedMessage);

        // Assert

        Assert.True(isSuccessfullySerialized);
        Assert.True(isSuccessfullyParsed);
        Assert.Equal(1, written);
        Assert.Equal((byte)errorCode, buffer[0]);
        Assert.Equal(errorCode, parsedMessage!.ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void TryParse_InvalidPayloadLength_ReturnsFalse(int payloadLength)
    {
        // Arrange

        var payload = new byte[payloadLength];

        // Act

        var isSuccessfullyParsed = ErrorMessage.TryParse(payload, out _);

        // Assert

        Assert.False(isSuccessfullyParsed);
    }

    [Theory]
    [InlineData(0x00)]
    [InlineData(0x08)]
    public void TryParse_UndefinedErrorCode_ReturnsFalse(byte errorCode)
    {
        // Arrange

        var payload = new[] { errorCode };

        // Act

        var isSuccessfullyParsed = ErrorMessage.TryParse(payload, out _);

        // Assert

        Assert.False(isSuccessfullyParsed);
    }
}
