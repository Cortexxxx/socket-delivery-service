using System.Net;
using DeliveryService.Protocol.Network;
using DeliveryService.Robot;

var serverHost = NetworkConfig.LocalServerHost;
var serverPort = NetworkConfig.ServerPort;
if (args.Length > 1)
{
    serverHost = args[0];
    int.TryParse(args[1], out serverPort);
}
IPEndPoint serverEndpoint = new(IPAddress.Parse(serverHost), serverPort);

var robotState = new RobotState();
var node = NetworkNodeFactory.Create(NetworkConfig.AnyPort, _ => new RobotHandlers(robotState));
var robotApp = new RobotApp(node.Transport, node.Sender, robotState, node.ReceiveProcessor, node.ReliabilityLayer, serverEndpoint);

node.Transport.Start();
robotApp.RunLoop();
