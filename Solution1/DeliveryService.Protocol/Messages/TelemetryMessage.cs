using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol.Messages;

[Message(MessageType.Telemetry, Flags = PacketFlags.None)]
public sealed class TelemetryMessage : IMessage
{
    public Point<float> Position { get; init; }
    public float Angle { get; init; }
    
    public static bool TryParse(ReadOnlySpan<byte> payload, [NotNullWhen(true)] out TelemetryMessage? message)
    {
        message = null;
        if (payload.Length != MessagesConstants.TelemetryPayloadSize)
        {
            return false;
        }

        var position = new Point<float>
        {
            X = BinaryPrimitives.ReadSingleLittleEndian(payload[..MessagesConstants.TelemetryYPositionOffset]),
            Y = BinaryPrimitives.ReadSingleLittleEndian(payload[MessagesConstants.TelemetryYPositionOffset..MessagesConstants.TelemetryAngleOffset])
        };
        
        var angle = BinaryPrimitives.ReadSingleLittleEndian(payload[MessagesConstants.TelemetryAngleOffset..]);

        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y) || !float.IsFinite(angle))
        {
            return false;
        }
        
        message = new TelemetryMessage
        {
            Position = position,
            Angle = angle
        };

        return true;
    }

    public bool TrySerialize(Span<byte> buffer,  out int written)
    {
        written = 0;
        
        if (buffer.Length < MessagesConstants.TelemetryPayloadSize)
        {
            return false;
        }
        
        if (!float.IsFinite(Position.X) || !float.IsFinite(Position.Y) || !float.IsFinite(Angle))
        {
            return false;
        }
        
        BinaryPrimitives.WriteSingleLittleEndian(buffer[..MessagesConstants.TelemetryYPositionOffset], Position.X);
        BinaryPrimitives.WriteSingleLittleEndian(buffer[MessagesConstants.TelemetryYPositionOffset
            ..MessagesConstants.TelemetryAngleOffset], Position.Y);
        BinaryPrimitives.WriteSingleLittleEndian(buffer[MessagesConstants.TelemetryAngleOffset
            ..MessagesConstants.TelemetryPayloadSize], Angle);
        
        written = MessagesConstants.TelemetryPayloadSize;
        
        return true;
    }
}