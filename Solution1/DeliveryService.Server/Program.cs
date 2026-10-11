using System.Reflection;
using DeliveryService.Protocol;
using DeliveryService.Protocol.Messages;

var assembly = Assembly.GetAssembly(typeof(DriveMessage));
var registry = new MessagesRegistry(assembly);
var packetCodec = new PacketCodec(registry);
var udpTransport = new UdpTransport(9000, packetCodec);

udpTransport.Start();

while (true)
{
    var tryReceiveResult = udpTransport.TryReceive(out var receivedPacket);
    
    if (!tryReceiveResult)
    {
        Thread.Sleep(10);
        continue;
    }
    
    Console.WriteLine(receivedPacket.Packet.PacketHeader.MessageType + " " + receivedPacket.Sender);
}