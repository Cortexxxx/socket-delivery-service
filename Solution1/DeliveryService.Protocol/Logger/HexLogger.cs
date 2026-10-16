using System.Net;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol.Logger;

public class HexLogger(LoggerFilters filters = LoggerFilters.All) : IPacketLogger
{
    private readonly Dictionary<MessageType, ConsoleColor> _messageColors = new()
    {
        [MessageType.Error] = ConsoleColor.Red,
        [MessageType.Hello] = ConsoleColor.DarkCyan,
        [MessageType.Ack] = ConsoleColor.Yellow,
        [MessageType.Heartbeat] = ConsoleColor.Magenta,
    };

    public void LogReceive(IPEndPoint sender, ReadOnlySpan<byte> packetBytes)
        => WriteLog("RECEIVE FROM", sender, packetBytes);

    public void LogSend(IPEndPoint receiver, ReadOnlySpan<byte> packetBytes)
        => WriteLog("SENDING TO", receiver, packetBytes);

    private bool ShouldLog(bool headerParsed, MessageType messageType)
    {
        if (!headerParsed) return filters.HasFlag(LoggerFilters.ShowErrors);

        return messageType switch
        {
            MessageType.Telemetry => filters.HasFlag(LoggerFilters.ShowTelemetry),
            MessageType.Error => filters.HasFlag(LoggerFilters.ShowErrors),
            _ => true
        };
    }

    private void WriteLog(string direction, IPEndPoint endPoint, ReadOnlySpan<byte> packetBytes)
    {
        var parsed = PacketHeader.TryParse(packetBytes, out var header);
        
        if (!ShouldLog(parsed, header.MessageType)) return;

        var prefix = $"{DateTime.UtcNow} {direction} ({endPoint}): ";
        var packet = $"Packet: [{Convert.ToHexString(packetBytes)}]";

        if (!parsed)
        {
            Console.WriteLine($"{prefix}error while parsing header\n{packet}");
            return;
        }

        if (_messageColors.TryGetValue(header.MessageType, out var color)) 
            Console.ForegroundColor = color;
        
        Console.WriteLine($"{prefix}\n{packet}\n" +
                          $"Seq={header.Seq}, " +
                          $"Flags={header.Flags}, " +
                          $"MessageType={header.MessageType}, " +
                          $"PayloadLength={header.PayloadLength}");
        Console.ForegroundColor = ConsoleColor.White;
    }
}
