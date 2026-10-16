using DeliveryService.Protocol.Core;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Messages;

namespace DeliveryService.Tests;

public class MessagesRegistryTests
{
    private readonly MessagesRegistry _registry = new(typeof(DriveMessage).Assembly);
    
    [Fact] 
    public void Constructor_AllMessageTypes_RegistersEveryMessageWithoutUnknownMessageError()
    {
        // Arrange
        
        var emptyPayload = ReadOnlySpan<byte>.Empty;

        // Act
        
        foreach (var messageType in Enum.GetValues<MessageType>())
        {
            // Act

            _registry.TryParsePayload(messageType, emptyPayload, out _, out var error);
            
            // Assert
            Assert.NotEqual(ErrorCode.UnknownMessage, error);
        }
    }

    [Fact]
    public void TryGetMessageInfo_MessageWithoutAttribute_ReturnsFalse()
    {
        // Arrange 
        
        var message = new UnknownMessage();
        
        // Act

        var tryGetResult = _registry.TryGetMessageInfo(message, out _);
        
        // Assert
        
        Assert.False(tryGetResult);
    }
    
    [Fact]
    public void TryParsePayload_UnknownMessageCode_ReturnsUnknownMessageError()
    {
        // Arrange 
        
        var message = new UnknownMessage();
        
        // Act

        var tryGetResult = _registry.TryParsePayload(0, [], out _, out var errorCode);
        
        // Assert
        
        Assert.Equal(ErrorCode.UnknownMessage, errorCode);
    }

    [Theory]
    [InlineData(MessageType.Drive, new byte[] {0x10, 0x20}, typeof(DriveMessage))]
    [InlineData(MessageType.Hello, new byte[] {0x01}, typeof(HelloMessage))]
    public void TryParsePayload_GoodPayload_ReturnsParsedMessage(MessageType messageType, byte[] payload, Type expectedType)
    {
        // Arrange

        // Act

        var parseResult = _registry.TryParsePayload(messageType,
            payload, out var message, out var errorCode);

        // Assert
        Assert.True(parseResult);
        Assert.Null(errorCode);
        Assert.NotNull(message);
        Assert.IsType(expectedType, message);

        var buffer = new byte[payload.Length];
        Assert.True(message.TrySerialize(buffer, out var written));
        Assert.Equal(payload.Length, written);
        Assert.Equal(payload, buffer);
    }
    
    // Bad parse return BadPayload error
    
    [Fact]
    public void TryParsePayload_BadPayload_ReturnsParsedMessage()
    {
        // Arrange
        var messageType = MessageType.Drive;
        byte[] payload = [0x10, 0x20, 0x30];

        // Act

        var parseResult = _registry.TryParsePayload(messageType, 
            payload, out var message, out var errorCode);
        
        // Assert
        
        Assert.False(parseResult);
        Assert.Equal(ErrorCode.BadPayload, errorCode);
    }
    // Good TryGetMessageInfo return correct messagetype and flags
    
    [Theory]
    [InlineData(typeof(DriveMessage), MessageType.Drive, PacketFlags.None)]
    [InlineData(typeof(HelloMessage), MessageType.Hello, PacketFlags.NeedAck)]
    public void TryGetMessageInfo_ValidMessage_ReturnsCorrectTypeAndFlags(
        Type messageClass, MessageType expectedType, PacketFlags expectedFlags)
    {
        // Arrange

        var message = (IMessage)Activator.CreateInstance(messageClass)!;

        // Act

        var tryGetResult = _registry.TryGetMessageInfo(message, out var messageInfo);

        // Assert

        Assert.True(tryGetResult);
        Assert.Equal(expectedType, messageInfo.messageType);
        Assert.Equal(expectedFlags, messageInfo.messageFlags);
    }
}