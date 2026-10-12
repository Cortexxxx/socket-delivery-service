using DeliveryService.Protocol;
using DeliveryService.Protocol.Attributes;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Interfaces;

namespace DeliveryService.Tests;

/// <summary>
/// Message without the Message attribute, so it is never registered.
/// </summary>
public class UnknownMessage : IMessage
{
    public static bool TryParse(ReadOnlySpan<byte> payload, out IMessage? message)
    {
        throw new NotImplementedException();
    }

    public bool TrySerialize(Span<byte> buffer, out int written)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Registered message that reports a configurable number of written payload bytes
/// without touching the buffer. Lets tests probe payload size limits.
/// </summary>
[Message(MessageType.Drive)]
public sealed class ConfigurablePayloadMessage(int payloadLength) : IMessage
{
    public static bool TryParse(ReadOnlySpan<byte> payload, out ConfigurablePayloadMessage? message)
    {
        message = null;
        return false;
    }

    public bool TrySerialize(Span<byte> buffer, out int written)
    {
        if (buffer.Length < payloadLength)
        {
            written = 0;
            return false;
        }
        
        written = payloadLength;
        return true;
    }
}
