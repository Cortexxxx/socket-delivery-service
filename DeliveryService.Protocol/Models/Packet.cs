using DeliveryService.Protocol.Messages;

namespace DeliveryService.Protocol.Models;

public class Packet(PacketHeader packetHeader, IMessage message)
{
    public PacketHeader PacketHeader { get; } = packetHeader;
    public IMessage Message { get; } = message;
}