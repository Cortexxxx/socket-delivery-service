using System.Net;
using System.Reflection;
using DeliveryService.Protocol;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Logger;
using DeliveryService.Protocol.Messages;

var assembly = Assembly.GetAssembly(typeof(DriveMessage));
var registry = new MessagesRegistry(assembly);
var packetCodec = new PacketCodec(registry);
var udpTransport = new UdpTransport(0, packetCodec, new HexLogger(LoggerFilters.None));
var endpoint = new IPEndPoint(IPAddress.Loopback, 9000);
udpTransport.Start();
ushort seq = 0;
var rand = new Random();
udpTransport.TrySend(++seq, endpoint, new HelloMessage
{
    Role = Role.Robot
});

while (true)
{
    udpTransport.TrySend(++seq, endpoint, new TelemetryMessage
    {
        Position = new Point<float>
        {
            X = rand.Next() % 255,
            Y = rand.Next() % 255
        }
    });
    Thread.Sleep(500);
}
Console.ReadLine();
