using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Messages;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Protocol.Core;

public class ReceiveProcessor(MessageSender sender, 
    MessagesDispatcher dispatcher, 
    ReliabilityLayer reliabilityLayer)
{
    public void ProcessPacket(ReceivedPacket receivedPacket)
    {
        var packet = receivedPacket.Packet;
        var packetHeader = packet.PacketHeader;
        var messageType = packetHeader.MessageType;
        var packetSender = receivedPacket.Sender;
        var seq = packetHeader.Seq;

        switch (messageType)
        {
            case MessageType.Drive or MessageType.Telemetry
                when reliabilityLayer.IsStaleSeq(packetSender, seq):
                return;
            case MessageType.Drive or MessageType.Telemetry:
                reliabilityLayer.UpdateLatestSeq(packetSender,
                    seq);
                break;
            case MessageType.Ack:
                reliabilityLayer.TryRemovePendingPacket(((AckMessage)packet.Message).Seq,
                    packetSender);
                break;
            case MessageType.Hello when reliabilityLayer.IsPacketAlreadyReceived(seq, packetSender):
                reliabilityLayer.UpdateLatestSeq(packetSender, seq);
                break;
        }

        if (packetHeader.Flags.HasFlag(PacketFlags.NeedAck))
        {
            SendAck(receivedPacket);

            if (reliabilityLayer.IsPacketAlreadyReceived(seq,
                    packetSender)) return;

            reliabilityLayer.TryMarkPacketAsReceived(seq,
                packetSender, DateTime.UtcNow);
        }

        var dispatchResult = dispatcher.Dispatch(receivedPacket);

        if (!dispatchResult && messageType != MessageType.Error
                            && messageType != MessageType.Ack)
        {
            sender.SendTo(packetSender, new ErrorMessage
            {
                ErrorCode = ErrorCode.UnknownMessage
            });
        }
    }

    private void SendAck(ReceivedPacket receivedPacket)
    {
        sender.SendTo(receivedPacket.Sender, new AckMessage
        {
            Seq = receivedPacket.Packet.PacketHeader.Seq
        });
    }
}
