using System.Net;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Server;

public class OrdersService
{
    public readonly Dictionary<ushort, Order> Orders = new();
    public Order? CurrentOrder { get; private set; }
    private Queue<Order> OrdersQueue { get; set; } = new();
    public readonly List<(ushort OrderId, OrderStatus status)> Changes = new();
    private ushort _ordersSeq;
    
    public Order CreateOrder(IPEndPoint customerEndpoint, Point<byte> deliveryPoint)
    {
        var order = new Order(customerEndpoint, deliveryPoint, _ordersSeq);
        OrdersQueue.Enqueue(order);
        Orders.Add(_ordersSeq, order);
        SetNewOrderInProcess();
        _ordersSeq++;
        return order;
    }

    public void SetNewOrderInProcess()
    {
        if ((CurrentOrder == null || CurrentOrder.OrderStatus == OrderStatus.Delivered))
        {
            if (OrdersQueue.TryDequeue(out var order))
            {
                CurrentOrder = order;
            }
            else
            {
                CurrentOrder = null;
            }
        }
    }

    public KeyValuePair<ushort, Order>[]? GetOrdersByIpEndpoint(IPEndPoint ipEndPoint)
    {
        return Orders.Where(o => o.Value.Customer.Equals(ipEndPoint)).ToArray();
    }

    public void AdvanceCurrentOrder()
    {
        if (CurrentOrder != null)
        {
            CurrentOrder.Advance();
        }
        else
        {
            Console.WriteLine("Error: No current order!");
        }
    }
}