using System.Net;
using DeliveryService.Client.Console;
using DeliveryService.Client.Core;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Network;

IPEndPoint serverEndpoint = new(IPAddress.Parse(NetworkConfig.ServerHost), NetworkConfig.ServerPort);

var tryParse = false;
var clientState = new ClientState();

while (!tryParse || clientState.Role == 0)
{
    ConsoleView.DisplayClientStartMenu();

    tryParse = int.TryParse(Console.ReadLine(), out var roleInt);
    
    if (tryParse && roleInt is 1 or 2)
    {
        clientState.Role = (Role)roleInt;
    }
}

var node = NetworkNodeFactory.Create(NetworkConfig.AnyPort, 
    _ => new ClientHandlers(clientState));

var clientApp = new ClientApp(node.Transport, 
    node.Sender, 
    node.ReceiveProcessor, 
    node.ReliabilityLayer,
    clientState, 
    serverEndpoint);

var consoleApp = new ConsoleClientApp(clientApp);
node.Transport.Start();

consoleApp.Run();
