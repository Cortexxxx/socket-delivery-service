using System.Diagnostics.CodeAnalysis;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol.Messages;

[Message(MessageType.Drive, Flags = PacketFlags.None)]
public sealed class DriveMessage : IMessage
{
    public sbyte Speed { get; init; }
    public sbyte Rotate { get; init; }
    
    public static bool TryParse(ReadOnlySpan<byte> payload, [NotNullWhen(true)] out DriveMessage? message)
    {
        message = null;
        if (payload.Length != MessagesConstants.DrivePayloadSize)
        {
            return false;
        }

        var speed = (sbyte)payload[0];
        var rotate = (sbyte)payload[1];

        if (!IsValid(speed, rotate))
        {
            return false;
        }
        
        message = new DriveMessage
        {
            Speed = speed,
            Rotate = rotate
        };

        return true;
    }

    public bool TrySerialize(Span<byte> buffer,  out int written)
    {
        written = 0;
        
        if (buffer.Length < MessagesConstants.DrivePayloadSize || !IsValid(Speed, Rotate))
        {
            return false;
        }

        buffer[0] = (byte)Speed;
        buffer[1] = (byte)Rotate;
        written = MessagesConstants.DrivePayloadSize;
        
        return true;
    }

    private static bool IsValid(sbyte speed, sbyte rotate)
    {
        return speed is >= -MessagesConstants.DriveMaxValue and <= MessagesConstants.DriveMaxValue 
               && rotate is >= -MessagesConstants.DriveMaxValue and <= MessagesConstants.DriveMaxValue;
    }
}