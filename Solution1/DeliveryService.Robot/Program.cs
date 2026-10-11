using System.Net;
using System.Reflection;
using DeliveryService.Protocol;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Messages;

var assembly = Assembly.GetAssembly(typeof(DriveMessage));
var registry = new MessagesRegistry(assembly);
var packetCodec = new PacketCodec(registry);
var udpTransport = new UdpTransport(0, packetCodec);

udpTransport.Start();

var trySendResult = udpTransport.TrySend(0, new IPEndPoint(IPAddress.Loopback, 9000)
    , new HelloMessage()
    {
        Role = Role.Robot
    });
Console.WriteLine(trySendResult);
Console.ReadLine();
