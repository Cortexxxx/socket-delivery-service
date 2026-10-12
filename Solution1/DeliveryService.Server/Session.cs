using System.Net;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Server;

public class Session
{
    public Session(IPEndPoint ipEndPoint, Role role)
    {
        IpEndPoint = ipEndPoint;
        Role = role;
        LastAliveTime = DateTime.UtcNow;
    }

    public IPEndPoint IpEndPoint { get; init; }
    public Role Role { get; init; }
    public DateTime LastAliveTime { get; set; }
}