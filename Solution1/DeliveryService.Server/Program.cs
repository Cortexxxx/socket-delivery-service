using System.Reflection;
using DeliveryService.Protocol;
using DeliveryService.Protocol.Messages;
using DeliveryService.Server;

var assembly = Assembly.GetAssembly(typeof(DriveMessage));
var registry = new MessagesRegistry(assembly);
var packetCodec = new PacketCodec(registry);
var udpTransport = new UdpTransport(9000, packetCodec);
var messagesDispatcher = new MessagesDispatcher(new ServerHandlers());

udpTransport.Start();

while (true)
{
    var tryReceiveResult = udpTransport.TryReceive(out var receivedPacket);
    
    if (!tryReceiveResult)
    {
        Thread.Sleep(10);
        continue;
    }

    messagesDispatcher.Dispatch(receivedPacket);
}