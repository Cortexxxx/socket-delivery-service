using System.Net;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol.Models;

public class Order(IPEndPoint customer, Point<byte> deliveryPoint, ushort id)
{
    public ushort Id { get; } = id;
    public OrderStatus OrderStatus { get; private set; } = OrderStatus.Pending;
    public IPEndPoint Customer { get; private init; } = customer;
    public Point<byte> DeliveryPoint { get; private init; } = deliveryPoint;

    public void Advance()
    {
        if (OrderStatus != OrderStatus.Delivered) OrderStatus++;
    }

    public void SetStatus(OrderStatus status)
    {
        OrderStatus = status;
    }
}