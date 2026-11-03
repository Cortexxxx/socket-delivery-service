using System.Net;

namespace DeliveryService.Protocol.Models;

public readonly record struct ReceivedPacket(IPEndPoint Sender, Packet Packet);
