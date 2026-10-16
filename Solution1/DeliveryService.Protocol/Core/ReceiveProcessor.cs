using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Messages;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Protocol;

public class ReceiveProcessor
{
    private readonly MessageSender _sender;
    private readonly MessagesDispatcher _dispatcher;

    public ReceiveProcessor(MessageSender sender, MessagesDispatcher dispatcher)
    {
        _sender = sender;
        _dispatcher = dispatcher;
    }

    public void ProcessPacket(ReceivedPacket receivedPacket)
    {
        SendAckIfNeeded(receivedPacket);

        var dispatchResult = _dispatcher.Dispatch(receivedPacket);
        var messageType = receivedPacket.Packet.PacketHeader.MessageType;

        if (!dispatchResult && messageType != MessageType.Error && messageType != MessageType.Ack)
        {
            _sender.SendTo(receivedPacket.Sender, new ErrorMessage
            {
                ErrorCode = ErrorCode.UnknownMessage
            });
        }
    }

    private void SendAckIfNeeded(ReceivedPacket receivedPacket)
    {
        if (receivedPacket.Packet.PacketHeader.Flags.HasFlag(PacketFlags.NeedAck))
        {
            _sender.SendTo(receivedPacket.Sender, new AckMessage
            {
                Seq = receivedPacket.Packet.PacketHeader.Seq
            });
        }
    }
}
