using System.Numerics;

namespace DeliveryService.Protocol;

public struct Point<T> where T : INumber<T>
{
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
}