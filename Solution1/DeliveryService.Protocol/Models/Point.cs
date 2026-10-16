using System.Numerics;

namespace DeliveryService.Protocol.Models;

public class Point<T> where T : INumber<T>
{
    public Point(T x, T y)
    {
        X = x;
        Y = y;
    }
    
    public Point() { }


    public T X { get; set; }
    public T Y { get; set; }

    public static Point<T> operator +(Point<T> a, Point<T> b) =>
        new() { X = a.X + b.X, Y = a.Y + b.Y };

    public static Point<T> operator -(Point<T> a, Point<T> b) =>
        new() { X = a.X - b.X, Y = a.Y - b.Y };

    public static Point<T> operator *(Point<T> point, T scalar) =>
        new() { X = point.X * scalar, Y = point.Y * scalar };

    public static Point<T> operator *(T scalar, Point<T> point) =>
        point * scalar;
    
    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public static float Distance(Point<float> first, Point<float> second)
    {
        return (float)Math.Sqrt((first.X - second.X) * (first.X - second.X) + (first.Y - second.Y) * ( first.Y - second.Y));
    }

    public static Point<float> ByteToCentralFloatPoint(Point<byte> bytePoint)
    {
        return new Point<float>(bytePoint.X + 0.5f, bytePoint.Y + 0.5f);
    }
}