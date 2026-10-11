using DeliveryService.Protocol.Messages;

namespace DeliveryService.Tests;

public class DriveMessageTests
{
    [Theory]
    [InlineData(0x00, 0x01, 0, 1)]
    [InlineData(0x64, 0x01, 100, 1)]
    [InlineData(0x00, 0x64, 0, 100)]
    [InlineData(0x9C, 0x01, -100, 1)]
    [InlineData(0x00, 0x9C, 0, -100)]
    public void TryParse_ValidPayload_ReturnsTrueAndExpectedValues(byte speed, byte rotate, sbyte expectedSpeed, sbyte expectedRotate)
    {
        // Arrange

        var payload = new[] { speed, rotate };
        
        // Act

        var isParsedSuccessfully = DriveMessage.TryParse(payload, out var message);
        
        // Assert
        
        Assert.True(isParsedSuccessfully); 
        Assert.Equal(expectedSpeed, message.Speed);
        Assert.Equal(expectedRotate, message.Rotate);
    }
    
    [Theory]
    [InlineData(0x65, 0x01)]
    [InlineData(0x01, 0x65)]
    [InlineData(0x9B, 0x01)]
    [InlineData(0x01, 0x9B)]
    public void TryParse_OutOfRangeValues_ReturnsFalse(byte speed, byte rotate)
    {
        // Arrange

        var payload = new[] { speed, rotate };
        
        // Act

        var isParsedSuccessfully = DriveMessage.TryParse(payload, out _);
        
        // Assert
        
        Assert.False(isParsedSuccessfully); 
    }

    [Fact]
    public void TryParse_IncorrectPayloadLength_ReturnsFalse()
    {
        // Arrange

        var payload = new byte [] { 0x01 };
        
        // Act

        var isParsedSuccessfully = DriveMessage.TryParse(payload, out _);
        
        // Assert
        
        Assert.False(isParsedSuccessfully); 
    }

    [Fact]
    public void TrySerialize_ValidMessage_ReturnsTrueAndExpectedBytes()
    {
        // Arrange
        
        var message = new DriveMessage()
        {
            Speed = -100,
            Rotate = 1
        };
        var buffer = new byte[100];
        
        // Act

        var isSuccessfulSerialize = message.TrySerialize(buffer, out var written);
        
        // Assert
        
        Assert.Equal(2, written);
        Assert.True(isSuccessfulSerialize);
        Assert.Equal(0x9C, buffer[0]);
        Assert.Equal(0x01, buffer[1]);
    } 
}