using System.Net;
using DeliveryService.Client;
using DeliveryService.Protocol.Core;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Network;

IPEndPoint serverEndpoint = new(IPAddress.Parse(NetworkConfig.ServerHost), NetworkConfig.ServerPort);

var tryParse = false;
ClientState clientState = null;

while (!tryParse)
{
    Console.Clear();
    Console.WriteLine("Welcome to DeliveryService!");
    Console.WriteLine("Choose your role: ");
    Console.WriteLine("1 - Operator");
    Console.WriteLine("2 - Customer");
    tryParse = int.TryParse(Console.ReadLine(), out var roleInt);
    
    if (tryParse && roleInt is 1 or 2)
    {
        clientState = new ClientState
        {
            Role = (Role)roleInt
        };
    }
}

var node = NetworkNodeFactory.Create(NetworkConfig.AnyPort, _ => new ClientHandlers(clientState));
var clientApp = new ClientApp(node.Transport, 
    node.Sender, 
    node.ReceiveProcessor, 
    clientState, 
    serverEndpoint);

node.Transport.Start();
clientApp.Run();