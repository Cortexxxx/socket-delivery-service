using System.Net;
using DeliveryService.Protocol;
using DeliveryService.Protocol.Interfaces;

namespace DeliveryService.Server;

public class ServerSender
{
    public ushort Seq { get; private set; }
    private readonly UdpTransport _udpTransport;

    public ServerSender(UdpTransport udpTransport)
    {
        _udpTransport = udpTransport;
    }

    public void SendTo(IPEndPoint ipEndPoint, IMessage message)
    {
        _udpTransport.TrySend(Seq++, ipEndPoint, message);
    }
}