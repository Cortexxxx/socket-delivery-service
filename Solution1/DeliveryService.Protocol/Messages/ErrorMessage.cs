using System.Diagnostics.CodeAnalysis;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol.Messages;

[Message(MessageType.Error, Flags = PacketFlags.None )]
public class ErrorMessage : IMessage
{
    public ErrorCode ErrorCode { get; init; }
    
    public static bool TryParse(ReadOnlySpan<byte> payload, [NotNullWhen(true)] out ErrorMessage? message)
    {
        message = null;
        
        if (payload.Length != MessagesConstants.ErrorPayloadSize)
        {
            return false;
        }
        
        var errorCode = payload[0];

        if (!Enum.IsDefined((ErrorCode)errorCode))
        {
            return false;
        }
        
        message = new ErrorMessage
        {
            ErrorCode = (ErrorCode)errorCode
        };
        
        return true;
    }
    
    public bool TrySerialize(Span<byte> buffer, out int written)
    {
        written = 0;
        
        if (buffer.Length < MessagesConstants.ErrorPayloadSize || !Enum.IsDefined(ErrorCode))
        {
            return false;
        }
        
        buffer[0] = (byte)ErrorCode;
        written = MessagesConstants.ErrorPayloadSize;
        
        return true;
    }
}