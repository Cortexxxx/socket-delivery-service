using System.Net;

namespace DeliveryService.Protocol;

public readonly record struct ReceivedPacket(IPEndPoint Sender, Packet Packet);
