using DeliveryService.Protocol.Core;
using DeliveryService.Protocol.Logger;
using DeliveryService.Protocol.Messages;

namespace DeliveryService.Protocol.Network;

public static class NetworkNodeFactory
{
    public static NetworkNode Create(int port, Func<MessageSender, object> createHandlers,
        IPacketLogger? logger = null)
    {
        var codec = new PacketCodec(new MessagesRegistry(typeof(DriveMessage).Assembly));
        var transport = new UdpTransport(port, codec, logger);
        var reliability = new ReliabilityLayer(transport);
        var sender = new MessageSender(transport, reliability);

        var dispatcher = new MessagesDispatcher(createHandlers(sender));

        return new NetworkNode(transport, sender, new ReceiveProcessor(sender, dispatcher, reliability), reliability);
    }
}
