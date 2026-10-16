using DeliveryService.Protocol.Logger;
using DeliveryService.Protocol.Messages;

namespace DeliveryService.Protocol;

public static class NetworkNodeFactory
{
    public static NetworkNode Create(int port, Func<MessageSender, object> createHandlers,
        IPacketLogger? logger = null)
    {
        var codec = new PacketCodec(new MessagesRegistry(typeof(DriveMessage).Assembly));
        var transport = new UdpTransport(port, codec, logger);
        var sender = new MessageSender(transport);

        var dispatcher = new MessagesDispatcher(createHandlers(sender));

        return new NetworkNode(transport, sender, new ReceiveProcessor(sender, dispatcher));
    }
}
