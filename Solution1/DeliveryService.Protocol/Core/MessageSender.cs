using System.Net;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Messages;

namespace DeliveryService.Protocol.Core;

public class MessageSender(UdpTransport udpTransport, ReliabilityLayer reliabilityLayer)
{
    private ushort Seq { get; set; }
    
    public void SendTo(IPEndPoint ipEndPoint, IMessage message)
    {
        var seq = Seq++;
        if (udpTransport.TrySend(seq, ipEndPoint, message, out var header,
                out var messageBytes) && header.Value.Flags.HasFlag(PacketFlags.NeedAck))
        {
            reliabilityLayer.TryAddPendingPacket(seq, ipEndPoint, messageBytes.ToArray());
        }    
    }


}