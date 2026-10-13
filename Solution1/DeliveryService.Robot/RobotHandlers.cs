using System.Net;
using DeliveryService.Protocol.Attributes;
using DeliveryService.Protocol.Messages;

namespace DeliveryService.Robot;

public class RobotHandlers
{
    [MessageHandler]
    public void OnAck(IPEndPoint sender, AckMessage message)
    {
        Console.WriteLine("ACK");
    }

    [MessageHandler]
    public void OnMap(IPEndPoint sender, MapMessage mapMessage)
    {
        Console.WriteLine($"Map W:{mapMessage.Width} H:{mapMessage.Height} {mapMessage.StoragePoint}");
    }
}