using DeliveryService.Protocol.Messages;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Protocol.Map;

public class MapState
{
    public MapState(bool[,] map, byte[] bitmask, Point<byte> storagePoint)
    {

        StoragePoint = storagePoint;
        Width = (byte)map.GetLength(0);
        Height = (byte)map.GetLength(1);
        Map = map;

        MapMessagePrefab = new MapMessage
        {
            Width = Width,
            Height = Height,
            Bitmask = bitmask,
            StoragePoint = storagePoint
        };
    }

    public bool[,] Map { get; private init; }
    public byte Width { get; private init; }
    public byte Height { get; private init; }
    public Point<byte> StoragePoint { get; private init; }
    public MapMessage MapMessagePrefab { get; init; }
    
    public bool CanMove(Point<float> position)
    {
        var cell = new Point<int>()
        {
            X = (int)Math.Floor(position.X),
            Y = (int)Math.Floor(position.Y)
        };

        if (cell.X >= Width || cell.X < 0 || cell.Y >= Height || cell.Y < 0)
        {
            return false;
        }
        
        return !Map[cell.X, cell.Y];
    }
}