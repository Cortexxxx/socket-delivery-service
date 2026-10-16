using System.Net;
using DeliveryService.Protocol.Interfaces;

namespace DeliveryService.Protocol;

public class MessageSender(UdpTransport udpTransport)
{
    private ushort Seq { get; set; }

    public void SendTo(IPEndPoint ipEndPoint, IMessage message)
    {
        udpTransport.TrySend(Seq++, ipEndPoint, message);
    }
}