using System.Net;
using DeliveryService.Protocol.Attributes;
using DeliveryService.Protocol.Core;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Map;
using DeliveryService.Protocol.Messages;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Server;

public class ServerHandlers(ServerSessions serverSessions, 
    MessageSender serverSender, 
    MapState mapState, 
    OrdersService ordersService)
{
    [MessageHandler]
    public void OnHello(IPEndPoint sender, HelloMessage message)
    {
        Console.WriteLine($"Yahoo! Hello my little {sender}");
        
        var trySetResult = message.Role switch
        {
            Role.Operator => serverSessions.TrySetSingleton(sender, Role.Operator),
            Role.Customer => serverSessions.TrySetCustomer(sender),
            Role.Robot => serverSessions.TrySetSingleton(sender, Role.Robot),
            _ => throw new ArgumentOutOfRangeException(nameof(message.Role))
        };

        if (!trySetResult)
        {
            serverSender.SendTo(sender, new ErrorMessage()
            {
                ErrorCode = ErrorCode.RoleTaken
            });
        }
        else
        {
            serverSender.SendTo(sender, mapState.MapMessagePrefab);
            SendAllOrders(message.Role, sender);
        }
    }
    
    [MessageHandler]
    public void OnTelemetry(IPEndPoint sender, TelemetryMessage message)
    {

        if (ordersService.CurrentOrder == null) return;
        
        var currentOrder = ordersService.CurrentOrder;
            
        SendTelemetry(message, currentOrder);
        
        if (ordersService.TryChangeStatus(message.Position, mapState.StoragePoint))
        {
            SendActualStatus();
        }
    }

    [MessageHandler]
    public void OnDrive(IPEndPoint sender, DriveMessage message)
    {
        if (serverSessions.Operator == null || !serverSessions.Operator.Equals(sender)) return;
        
        if (serverSessions.Robot == null)
        {
            serverSender.SendTo(sender, new ErrorMessage
            {
                ErrorCode = ErrorCode.RobotOffline
            });
        }
        else
        {
            serverSender.SendTo(serverSessions.Robot, message);
        }
    }

    [MessageHandler]
    public void OnHeartbeat(IPEndPoint sender, HeartbeatMessage message) { }

    [MessageHandler]
    public void OnOrderCreate(IPEndPoint sender, OrderCreateMessage message)
    {
        if (!serverSessions.TryGetSessionByAddress(sender, out var session) 
            || session.Role != Role.Customer) return;
        
        var point = new Point<byte>(message.X, message.Y);
        
        if (mapState.CanMove(Point<float>.ByteToCentralFloatPoint(point)))
        {
            ordersService.CreateOrder(sender, point);
            SendActualStatus();
        }
        else
        {
            serverSender.SendTo(sender, new ErrorMessage
            {
                ErrorCode = ErrorCode.InvalidPoint
            });
        }
    }

    [MessageHandler]
    public void OnError(IPEndPoint sender, ErrorMessage message)
    {
        Console.WriteLine($"Error: {message.ErrorCode}");
    }
    
    private void SendTelemetry(TelemetryMessage message, Order currentOrder)
    {
        if (serverSessions.Operator != null) serverSender.SendTo(serverSessions.Operator, message);

        if (currentOrder.OrderStatus is OrderStatus.PickingUp or OrderStatus.InTransit)
            serverSender.SendTo(currentOrder.Customer, message);
    }

    private void SendActualStatus()
    {
        if (ordersService.CurrentOrder == null) return;
        SendOrder(ordersService.CurrentOrder, ordersService.CurrentOrder.Customer);
        SendOrder(ordersService.CurrentOrder, serverSessions.Operator);
    }
    
    private void SendOrder(Order order, IPEndPoint? receiver)
    {
        if (receiver == null) return;
        
        serverSender.SendTo(receiver, new OrderStatusMessage
        {
            Status = order.OrderStatus,
            OrderId = order.Id,
            Endpoint = order.DeliveryPoint
        });
    }
    
    private void SendAllOrders(Role role,  IPEndPoint? customerEndpoint = null)
    {
        var receiver = customerEndpoint ?? serverSessions.Operator;
        
        if (receiver == null) return;
        
        var orders = role == Role.Operator 
            ? ordersService.Orders : ordersService.GetOrdersByIpEndpoint(receiver);

        foreach (var order in orders)
        {
            if (receiver != null) SendOrder(order.Value, receiver);
        }
    }

}