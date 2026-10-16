using System.Text;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Protocol.Map;

public class FileMap : IMap
{
    private readonly string _path;
    private readonly char _obstacleChar;
    private readonly char _storageChar;

    public FileMap(string path, char obstacleChar, char storageChar)
    {
        _path = path;
        _obstacleChar = obstacleChar;
        _storageChar = storageChar;
    }
    
    public bool TryLoad(out bool[,]? map, out Point<byte>? storagePoint)
    {
        map = null;
        storagePoint = null;
        string[] mapStrings;
        try
        {
            mapStrings = File.ReadAllLines(_path, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        if (mapStrings.Length == 0)
        {
            return false;
        }

        var height = mapStrings.Length;
        var width = mapStrings[0].Length;
        map = new bool[width, height];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (mapStrings[y][x] == _obstacleChar)
                {
                    map[x,y] = true;
                }
                else if (mapStrings[y][x] == _storageChar)
                {
                    storagePoint = new Point<byte>()
                    {
                        X = (byte)x,
                        Y = (byte)y
                    };
                }
            }
        }

        return true;
    }
}