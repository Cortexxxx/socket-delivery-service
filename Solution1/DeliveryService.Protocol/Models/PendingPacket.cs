using System.Net;

namespace DeliveryService.Protocol.Models;

public class PendingPacket(
    IPEndPoint destination,
    ushort seq,
    byte[] bytes,
    DateTime lastSentAt,
    byte attempts)
{
    public IPEndPoint Destination { get; set; } = destination;
    public ushort Seq { get; set; } = seq;
    public byte[] Bytes { get; set; } = bytes;
    public DateTime LastSentAt { get; set; } = lastSentAt;
    public byte Attempts { get; set; } = attempts;
}