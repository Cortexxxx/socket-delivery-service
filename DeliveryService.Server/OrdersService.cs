using System.Net;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Server;

public class OrdersService
{
    public readonly Dictionary<ushort, Order> Orders = new();
    public Order? CurrentOrder { get; private set; }
    private Queue<Order> OrdersQueue { get; } = new();
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
    
    public bool TryChangeStatus(Point<float> robotPosition, Point<byte> storagePoint)
    {
        var wasStatusChanged = false;
        
        if (CurrentOrder == null) return false;
        
        switch (CurrentOrder.OrderStatus)
        {
            case OrderStatus.Pending:
                AdvanceCurrentOrder();
                wasStatusChanged = true;
                break;
            case OrderStatus.PickingUp:
                if (Point<float>.Distance(robotPosition, 
                        Point<float>.ByteToCentralFloatPoint(storagePoint)) <= 0.5)
                {
                    AdvanceCurrentOrder();
                    wasStatusChanged = true;
                }
                break;
            case OrderStatus.InTransit:
                if (CurrentOrder != null && Point<float>.Distance(robotPosition, 
                        Point<float>.ByteToCentralFloatPoint(CurrentOrder.DeliveryPoint)) <= 0.5)
                {
                    AdvanceCurrentOrder();
                    wasStatusChanged = true;
                }
                break;
            case OrderStatus.Delivered:
                SetNewOrderInProcess();
                wasStatusChanged = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return wasStatusChanged;
    }

    private void SetNewOrderInProcess()
    {
        if (CurrentOrder == null || CurrentOrder.OrderStatus == OrderStatus.Delivered) 
            CurrentOrder = OrdersQueue.TryDequeue(out var order) ? order : null;
    }

    public IEnumerable<KeyValuePair<ushort, Order>> GetOrdersByIpEndpoint(IPEndPoint ipEndPoint) 
        => Orders.Where(o => o.Value.Customer.Equals(ipEndPoint));

    private void AdvanceCurrentOrder()
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