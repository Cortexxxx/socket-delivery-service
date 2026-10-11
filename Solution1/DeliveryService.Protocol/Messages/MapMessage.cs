using System.Diagnostics.CodeAnalysis;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol.Messages;

[Message(MessageType.Map, Flags = PacketFlags.NeedAck )]
public sealed class MapMessage : IMessage
{
    public byte Width { get; init; }
    public byte Height { get; init; }
    public required byte[] Bitmask { get; init; }
    public Point<byte> StoragePoint { get; init; }

    public static bool TryParse(ReadOnlySpan<byte> payload, [NotNullWhen(true)] out MapMessage? message)
    {
        message = null;

        if (payload.Length < 2)
        {
            return false;
        }
        
        var height = payload[0];
        var width = payload[1];
        var mapSize = height * width;
        
        if (width <= 0 || height <= 0 || mapSize > MessagesConstants.MapMaxSize)
        {
            return false;
        }
        
        var bitmaskLength = (width * height + 7) / 8;
        var payloadLength = 2 + bitmaskLength + 2;

        if (payloadLength != payload.Length)
        {
            return false;
        }

        var bitmask = new byte[bitmaskLength];
        
        for (var i = 0; i < bitmaskLength; i++)
        {
            var mapByte = payload[2 + i];
            bitmask[i] = mapByte;
        }

        var storageX = payload[2 + bitmaskLength];
        var storageY = payload[2 + bitmaskLength + 1];

        message = new MapMessage
        {
            Width = width,
            Height = height,
            Bitmask = bitmask,
            StoragePoint = new Point<byte>()
            {
                X = storageX,
                Y = storageY
            }
        };

        return true;
    }
    
    public bool TrySerialize(Span<byte> buffer, out int written)
    {
        written = 0;

        var payloadLength = 2 + Bitmask.Length + 2;

        if (Width <= 0 
            || Height <= 0 
            || buffer.Length < payloadLength 
            || Width * Height > MessagesConstants.MapMaxSize
            || (Width * Height + 7) / 8 != Bitmask.Length)
        {
            return false;
        }

        buffer[0] = Height;
        buffer[1] = Width;

        for (var i = 0; i < Bitmask.Length; i++)
        {
            buffer[2 + i] = Bitmask[i];
        }

        buffer[2 + Bitmask.Length] = StoragePoint.X;
        buffer[2 + Bitmask.Length + 1] = StoragePoint.Y;
        
        written = payloadLength;
        
        return true;
    }

    public bool[,]? GetBoolMatrixFromBitmask()
    {
        var result = new bool[Width, Height];

        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                var index = Width * y + x;
                var mapByte = Bitmask[index / 8];
                var bitNumber = index % 8;
                var mask = 1 << bitNumber;
                var bitValue = (mapByte & mask) != 0;
                result[x, y] = bitValue;
            }
        }

        return result;
    }

    public static bool TryGetBitmaskFromBoolMatrix(bool[,]? boolMap, [NotNullWhen(true)] out byte[]? bitmask)
    {
        bitmask = null;

        if (boolMap == null)
        {
            return false;
        }
        
        var width = boolMap.GetLength(0);
        var height = boolMap.GetLength(1);

        if (width == 0 || height == 0)
        {
            return false;
        }

        bitmask = new byte[(width * height + 7) / 8];

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var index = width * y + x;
                var byteNumber = index / 8;
                var bitNumber = index % 8;
                var currentByte = bitmask[byteNumber];
                var bitValue = (boolMap[x, y] ? 1 : 0) << bitNumber;
                bitmask[byteNumber] = (byte)(currentByte | bitValue);
            }
        }

        return true;
    }
}