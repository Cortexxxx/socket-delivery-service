using System.Net;
using DeliveryService.Protocol.Messages;

namespace DeliveryService.Protocol.Core;

public class MessageSender(UdpTransport udpTransport)
{
    private ushort Seq { get; set; }

    public void SendTo(IPEndPoint ipEndPoint, IMessage message)
    {
        udpTransport.TrySend(Seq++, ipEndPoint, message);
    }
}