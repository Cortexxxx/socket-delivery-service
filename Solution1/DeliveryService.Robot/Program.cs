using System.Net;
using DeliveryService.Protocol.Core;
using DeliveryService.Protocol.Network;
using DeliveryService.Robot;

IPEndPoint serverEndpoint = new(IPAddress.Parse(NetworkConfig.ServerHost), NetworkConfig.ServerPort);

var robotState = new RobotState();
var node = NetworkNodeFactory.Create(NetworkConfig.AnyPort, _ => new RobotHandlers(robotState));
var robotApp = new RobotApp(node.Transport, node.Sender, robotState, node.ReceiveProcessor, serverEndpoint);

node.Transport.Start();
robotApp.Run();
