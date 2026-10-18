using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Map;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Client.Core;

public sealed class ClientState
{
    public bool IsRegistered { get; set; }
    public Role Role { get; set; }
    public Dictionary<ushort, Order> Orders { get; } = new();
    public MapState? MapState { get; set; }
    public Point<float>? Position { get; set; }
    public float? Angle { get; set; }
    public string LastEvent { get; set; } = "-";
    public Order? CurrentOrder => Orders.Values
        .FirstOrDefault(o => o.OrderStatus != OrderStatus.Delivered);
}
