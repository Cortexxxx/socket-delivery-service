using System.Text;
using DeliveryService.Protocol.Map;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Client.Console;

public static class ConsoleView
{
    private const char Wall = '#';
    private const char Free = '.';
    private const char Storage = 'S';
    private const char Delivery = 'D';
    private const char Robot = 'R';

    private static readonly object ConsoleLock = new();
    private static int _lastLineCount;

    public static void DisplayClientStartMenu()
    {
        System.Console.Clear();
        System.Console.WriteLine("Welcome to DeliveryService!");
        System.Console.WriteLine("Choose your role: ");
        System.Console.WriteLine("1 - Operator");
        System.Console.WriteLine("2 - Customer");
    }
    
    public static void Render(MapState? map, Point<float>? robot, 
        float? angle, IReadOnlyCollection<Order> orders,
        Order? currentOrder, string lastEvent, string footer = "")
    {
        var sb = new StringBuilder();

        if (map == null)
        {
            sb.AppendLine("Waiting for map...");
        }
        else
        {
            AppendMap(sb, map, robot, currentOrder);
            var robotText = robot == null ? "unknown" : $"({robot.X:F1}, " +
                                                        $"{robot.Y:F1}) angle {angle:F0}°";
            sb.AppendLine($"Robot: {robotText}   Storage: {map.StoragePoint}");
            sb.AppendLine($"Legend: {Robot} robot  {Storage} storage  {Delivery} delivery  {Wall} wall");
        }

        sb.AppendLine($"Current order: {(currentOrder == null ? "none" : FormatOrder(currentOrder))}");

        if (orders.Count > 0)
        {
            sb.AppendLine("Orders:");
            
            foreach (var order in orders)
            {
                sb.AppendLine($"  {(order == currentOrder ? '>' : ' ')} {FormatOrder(order)}");
            }
        }

        sb.AppendLine($"Last event: {lastEvent}");

        if (footer != "") sb.AppendLine(footer);

        Write(sb.ToString());
    }

    private static void AppendMap(StringBuilder sb, MapState map, 
        Point<float>? robot, Order? currentOrder)
    {
        var robotX = robot == null ? -1 : (int)Math.Floor(robot.X);
        var robotY = robot == null ? -1 : (int)Math.Floor(robot.Y);

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                sb.Append(x == robotX && y == robotY ? Robot
                    : currentOrder != null && x == currentOrder.DeliveryPoint.X && y == currentOrder.DeliveryPoint.Y ? Delivery
                    : x == map.StoragePoint.X && y == map.StoragePoint.Y ? Storage
                    : map.Map[x, y] ? Wall
                    : Free);
            }

            sb.AppendLine();
        }
    }

    private static string FormatOrder(Order order) =>
        $"#{order.Id} {order.OrderStatus} -> {order.DeliveryPoint}";
    
    private static void Write(string text)
    {
        lock (ConsoleLock)
        {
            var width = Math.Max(System.Console.WindowWidth - 1, 1);
            var lines = text.Split(["\r\n", "\n"], StringSplitOptions.None).ToList();
            lines.RemoveAt(lines.Count - 1);
            lines = lines.Take(Math.Max(System.Console.WindowHeight - 1, 1)).ToList();

            while (lines.Count < _lastLineCount)
            {
                lines.Add("");
            }

            _lastLineCount = lines.Count;

            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.AppendLine(line.Length > width ? line[..width] : line.PadRight(width));
            }

            System.Console.CursorVisible = false;
            System.Console.SetCursorPosition(0, 0);
            System.Console.Write(sb.ToString());
        }
    }
}
