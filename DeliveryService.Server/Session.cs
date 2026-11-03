using System.Net;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Server;

public class Session(IPEndPoint ipEndPoint, Role role)
{
    public IPEndPoint IpEndPoint { get; } = ipEndPoint;
    public Role Role { get; } = role;
    public DateTime LastAliveTime { get; set; } = DateTime.UtcNow;
}