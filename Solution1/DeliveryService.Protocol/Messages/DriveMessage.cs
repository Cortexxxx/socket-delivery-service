using System.Diagnostics.CodeAnalysis;

namespace DeliveryService.Protocol.Messages;

public sealed class DriveMessage : IMessage
{
    private const int PayloadSize = 2;
    private const sbyte MaxDrivePercent = 100;
    
    public sbyte Speed { get; set; }
    public sbyte Rotate { get; set; }
    
    public static bool TryParse(ReadOnlySpan<byte> payload, [NotNullWhen(true)] out DriveMessage? message)
    {
        message = null;
        if (payload.Length != PayloadSize)
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
        
        if (buffer.Length < PayloadSize || !IsValid(Speed, Rotate))
        {
            return false;
        }

        buffer[0] = (byte)Speed;
        buffer[1] = (byte)Rotate;
        written = PayloadSize;
        
        return true;
    }

    private static bool IsValid(sbyte speed, sbyte rotate)
    {
        return (speed is >= -MaxDrivePercent and <= MaxDrivePercent) && (rotate is >= -MaxDrivePercent and <= MaxDrivePercent);
    }
}