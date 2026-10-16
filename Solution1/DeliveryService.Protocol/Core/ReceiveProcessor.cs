using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Messages;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Protocol.Core;

public class ReceiveProcessor(MessageSender sender, MessagesDispatcher dispatcher)
{
    public void ProcessPacket(ReceivedPacket receivedPacket)
    {
        SendAckIfNeeded(receivedPacket);

        var dispatchResult = dispatcher.Dispatch(receivedPacket);
        var messageType = receivedPacket.Packet.PacketHeader.MessageType;

        if (!dispatchResult && messageType != MessageType.Error 
                            && messageType != MessageType.Ack)
        {
            sender.SendTo(receivedPacket.Sender, new ErrorMessage
            {
                ErrorCode = ErrorCode.UnknownMessage
            });
        }
    }

    private void SendAckIfNeeded(ReceivedPacket receivedPacket)
    {
        if (receivedPacket.Packet.PacketHeader.Flags.HasFlag(PacketFlags.NeedAck))
        {
            sender.SendTo(receivedPacket.Sender, new AckMessage
            {
                Seq = receivedPacket.Packet.PacketHeader.Seq
            });
        }
    }
}
