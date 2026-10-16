using DeliveryService.Protocol.Interfaces;

namespace DeliveryService.Protocol.Models;

public class Packet
{
    public Packet(PacketHeader packetHeader, IMessage message)
    {
        Message = message;
        PacketHeader = packetHeader;
    }

    public PacketHeader PacketHeader { get; init; }
    public IMessage Message { get; init; }
}