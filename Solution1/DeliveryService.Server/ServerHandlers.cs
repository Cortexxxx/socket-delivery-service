using System.Net;
using DeliveryService.Protocol;
using DeliveryService.Protocol.Messages;

namespace DeliveryService.Server;

public class ServerHandlers
{
    [MessageHandler]
    public void OnHello(IPEndPoint sender, HelloMessage message)
    {
        Console.WriteLine($"Yahoo! Hello my little {sender}");
    }
    
    [MessageHandler]
    public void OnTelemetry(IPEndPoint sender, TelemetryMessage message)
    {
        Console.WriteLine($"My little {sender}, you are moving really cool! {message.Position}");
    }
}