using System.Net;
using System.Reflection;
using DeliveryService.Protocol;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Logger;
using DeliveryService.Protocol.Messages;
using DeliveryService.Robot;

var assembly = Assembly.GetAssembly(typeof(DriveMessage));
var registry = new MessagesRegistry(assembly);
var packetCodec = new PacketCodec(registry);
var udpTransport = new UdpTransport(0, packetCodec, new HexLogger(LoggerFilters.None));
var endpoint = new IPEndPoint(IPAddress.Loopback, 9000);
var robotHandlers = new RobotHandlers();
var messagesDispatcher = new MessagesDispatcher(robotHandlers);

udpTransport.Start();
ushort seq = 0;
var rand = new Random();

udpTransport.TrySend(++seq, endpoint, new HelloMessage
{
    Role = Role.Robot
});

var lastTelemetryTime = DateTime.UtcNow;
while (true)
{
    if (lastTelemetryTime + TimeSpan.FromSeconds(RobotConstants.SendTelemetryIntervalSeconds) < DateTime.Now)
    {
        udpTransport.TrySend(++seq, endpoint, new TelemetryMessage
        {
            Position = new Point<float>
            {
                X = rand.Next() % 255,
                Y = rand.Next() % 255
            }
        });
        lastTelemetryTime = DateTime.Now;
    }

    var tryReceiveResult = udpTransport.TryReceive(out var receivedPacket);

    
    if (!tryReceiveResult)
    {
        Thread.Sleep(10);
        continue;
    }

    messagesDispatcher.Dispatch(receivedPacket);
}

Console.ReadLine();