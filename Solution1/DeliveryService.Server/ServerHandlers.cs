using System.Drawing;
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
            _ => throw new ArgumentOutOfRangeException()
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

    private void SendAllOrders(Role role,  IPEndPoint? customerEndpoint = null)
    {
        var receiver = customerEndpoint ?? serverSessions.Operator;
        KeyValuePair<ushort, Order>[]? orders = null;
        orders = role == Role.Operator ? ordersService.Orders.ToArray() : ordersService.GetOrdersByIpEndpoint(receiver);

        if (orders == null) return;
        
        foreach (var order in orders)
        {
            if (receiver != null)
            {
                SendOrder(order.Value, receiver);
            }
        }
    }

    private void SendOrder(Order order, IPEndPoint? receiver)
    {
        Console.WriteLine($"Sending order status to {receiver}: {order.Id} with status {order.OrderStatus}");
        if (receiver == null) return;
        serverSender.SendTo(receiver, new OrderStatusMessage
        {
            Status = order.OrderStatus,
            OrderId = order.Id,
            Endpoint = order.DeliveryPoint
        });
    }

    [MessageHandler]
    public void OnTelemetry(IPEndPoint sender, TelemetryMessage message)
    {
        foreach (var session in serverSessions.GetAllSessions()
                     .Where(session => session.Role == Role.Operator))
        {
            serverSender.SendTo(session.IpEndPoint, message);
        }

        if (ordersService.CurrentOrder != null)
        {
            var currentOrder = ordersService.CurrentOrder;
            
            if (currentOrder.OrderStatus is OrderStatus.PickingUp or OrderStatus.InTransit)
            {
                serverSender.SendTo(currentOrder.Customer, message);
            }
            
            var wasStatusChanged = false;
            
            switch (currentOrder.OrderStatus)
            {
                case OrderStatus.Pending:
                    ordersService.AdvanceCurrentOrder();
                    wasStatusChanged = true;
                    break;
                case OrderStatus.PickingUp:
                    if (Point<float>.Distance(message.Position, 
                            Point<float>.ByteToCentralFloatPoint(mapState.StoragePoint)) <= 0.5)
                    {
                        ordersService.AdvanceCurrentOrder();
                        wasStatusChanged = true;
                    }
                    break;
                case OrderStatus.InTransit:
                    if (ordersService.CurrentOrder != null && Point<float>.Distance(message.Position, 
                            Point<float>.ByteToCentralFloatPoint(ordersService.CurrentOrder.DeliveryPoint)) <= 0.5)
                    {
                        ordersService.AdvanceCurrentOrder();
                        wasStatusChanged = true;
                    }
                    break;
                case OrderStatus.Delivered:
                    SendOrder(ordersService.CurrentOrder, ordersService.CurrentOrder.Customer);
                    SendOrder(ordersService.CurrentOrder, serverSessions.Operator);
                    
                    ordersService.SetNewOrderInProcess();
                    wasStatusChanged = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (wasStatusChanged && ordersService.CurrentOrder != null)
            {
                SendOrder(ordersService.CurrentOrder, ordersService.CurrentOrder.Customer);
                SendOrder(ordersService.CurrentOrder, serverSessions.Operator);
            }
        }
    }

    [MessageHandler]
    public void OnDrive(IPEndPoint sender, DriveMessage message)
    {
        if (serverSessions.TryGetSessionByAddress(sender, out var session) && session.Role == Role.Operator)
        {
            if (serverSessions.Robot == null)
            {
                serverSender.SendTo(sender, new ErrorMessage()
                {
                    ErrorCode = ErrorCode.RobotOffline
                });
            }
            else
            {
                serverSender.SendTo(serverSessions.Robot, message);
            }
        }
    }

    [MessageHandler]
    public void OnHeartbeat(IPEndPoint sender, HeartbeatMessage message)
    {
        if (serverSessions.TryGetSessionByAddress(sender, out var session))
        {
            session.LastAliveTime = DateTime.UtcNow;
        }
    }

    [MessageHandler]
    public void OnOrderCreate(IPEndPoint sender, OrderCreateMessage message)
    {
        if (serverSessions.TryGetSessionByAddress(sender, out var session) && session.Role == Role.Customer)
        {
            var point = new Point<byte>(message.X, message.Y);
            if (mapState.CanMove(Point<float>.ByteToCentralFloatPoint(point)))
            {
                var order = ordersService.CreateOrder(sender, point);
                SendOrder(order, sender);
                SendOrder(order, serverSessions.Operator);
            }
            else
            {
                serverSender.SendTo(sender, new ErrorMessage()
                {
                    ErrorCode = ErrorCode.InvalidPoint
                });
            }
        }
    }

    [MessageHandler]
    public void OnError(IPEndPoint sender, ErrorMessage message)
    {
        Console.WriteLine($"Error: {message.ErrorCode}");
    }
}