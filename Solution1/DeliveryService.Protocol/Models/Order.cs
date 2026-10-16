using System.Net;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol.Models;

public class Order
{
    public Order(IPEndPoint customer, Point<byte> deliveryPoint, ushort id)
    {
        Customer = customer;
        Id = id;
        DeliveryPoint = deliveryPoint;
    }

    public ushort Id { get; init; }
    public OrderStatus OrderStatus { get; private set; } = OrderStatus.Pending;
    public IPEndPoint Customer { get; private init; }
    public Point<byte> DeliveryPoint { get; private init; }

    public void Advance()
    {
        if (OrderStatus != OrderStatus.Delivered)
        {
            OrderStatus++;
        }
    }

    public void SetStatus(OrderStatus status)
    {
        OrderStatus = status;
    }
}