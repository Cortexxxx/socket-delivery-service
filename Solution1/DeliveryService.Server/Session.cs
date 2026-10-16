using System.Net;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Server;

public class Session(IPEndPoint ipEndPoint, Role role)
{
    public IPEndPoint IpEndPoint { get; init; } = ipEndPoint;
    public Role Role { get; init; } = role;
    public DateTime LastAliveTime { get; set; } = DateTime.UtcNow;
}