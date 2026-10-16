using System.Net;
using DeliveryService.Protocol.Attributes;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Map;
using DeliveryService.Protocol.Messages;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Client;

public class ClientHandlers(ClientState clientState)
{
    [MessageHandler]
    public void OnMap(IPEndPoint endPoint, MapMessage message)
    {
        clientState.MapState = new MapState(message.GetBoolMatrixFromBitmask()
            , message.Bitmask, message.StoragePoint);
        clientState.IsRegistered = true;
    }
    
    [MessageHandler]
    public void OnOrderStatus(IPEndPoint endPoint, OrderStatusMessage message)
    {
        if (clientState.Orders.TryGetValue(message.OrderId, out var value))
        {
            value.SetStatus(message.Status);
        }
        else
        {
            clientState.Orders.Add(message.OrderId, 
                new Order(null!, message.Endpoint, message.OrderId));
        }
    }
    
    [MessageHandler]
    public void OnError(IPEndPoint endPoint, ErrorMessage message)
    {
        clientState.LastEvent = "Error: " + message.ErrorCode;
        
        switch (message.ErrorCode)
        {
            case ErrorCode.UnknownMessage:
                break;
            case ErrorCode.BadPayload:
                break;
            case ErrorCode.NotRegistered:
                clientState.IsRegistered = false;
                break;
            case ErrorCode.RoleTaken:
                break;
            case ErrorCode.Forbidden:
                break;
            case ErrorCode.InvalidPoint:
                break;
            case ErrorCode.RobotOffline:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [MessageHandler]
    public void OnTelemetry(IPEndPoint endPoint, TelemetryMessage message)
    {
        clientState.Position = message.Position;
        clientState.Angle = message.Angle;
    }
}