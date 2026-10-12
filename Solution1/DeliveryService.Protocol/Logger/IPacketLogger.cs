using System.Net;

namespace DeliveryService.Protocol.Logger;

public interface IPacketLogger
{
    public void LogReceive(IPEndPoint sender, ReadOnlySpan<byte> packetBytes);
    public void LogSend(IPEndPoint receiver, ReadOnlySpan<byte> packetBytes);
}