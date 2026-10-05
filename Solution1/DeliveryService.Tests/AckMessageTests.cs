using DeliveryService.Protocol.Messages;

namespace DeliveryService.Tests;

public class AckMessageTests
{
    [Fact]
    public void Good_AckMessageRoundTrip_ParsesSerializesAndReturnTrue()
    {
        // Arrange

        var message = new AckMessage()
        {
            Seq = 0xFDE8
        };
        
        var buffer = new byte[100];
        var payload = new byte[] {0xE8, 0xFD};
        
        // Act

        var isSuccessfullySerialized = message.TrySerialize(buffer, out var written);

        var isSuccessfullyParsed = AckMessage.TryParse(payload, out var parsedMessage);
        
        // Assert

        Assert.True(isSuccessfullySerialized);
        Assert.True(isSuccessfullyParsed);
        Assert.Equal(2, written);
        Assert.Equal(0xE8, buffer[0]);
        Assert.Equal(0xFD, buffer[1]);
        Assert.Equal(message.Seq,parsedMessage!.Seq);
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

        var isSuccessfullyParsed = AckMessage.TryParse(payload, out _);
        
        // Assert
        
        Assert.False(isSuccessfullyParsed);
    }
}