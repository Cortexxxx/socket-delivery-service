using System.Net;

namespace DeliveryService.Protocol.Models;

public readonly record struct PacketKey(IPEndPoint EndPoint, ushort Seq);
