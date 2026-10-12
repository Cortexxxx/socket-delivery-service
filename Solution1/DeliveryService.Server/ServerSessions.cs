using System.Diagnostics.CodeAnalysis;
using System.Net;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Server;

public class ServerSessions
{
    public IPEndPoint? Robot { get; private set; }
    public IPEndPoint? Operator { get; private set; }
    private readonly Dictionary<IPEndPoint, Session> _sessions = new();

    public bool TrySetRobot(IPEndPoint endPoint)
    {
        if (Robot != null)
        {   
            return Robot.Equals(endPoint);
        }

        if (_sessions.ContainsKey(endPoint))
        {
            return false;
        }


        Robot = endPoint;
        _sessions[endPoint] = new Session(endPoint, Role.Robot);
        return true;
    }
    
    public bool TrySetOperator(IPEndPoint endPoint)
    {
        if (Operator != null)
        {
            return Operator.Equals(endPoint);
        }
        
        if (_sessions.ContainsKey(endPoint))
        {
            return false;
        }
        
        Operator = endPoint;
        _sessions[endPoint] = new Session(endPoint, Role.Operator);
        return true;
    }

    public bool TrySetCustomer(IPEndPoint endPoint)
    {
        var sessionExists = _sessions.TryGetValue(endPoint, out var session);

        if (sessionExists)
        {
            return session!.Role == Role.Customer;
        }
        
        _sessions[endPoint] = new Session(endPoint, Role.Customer);
        return true;
    }

    public bool TryGetSessionByAddress(IPEndPoint endPoint, 
        [NotNullWhen(true)] out Session? session)
    {
        return _sessions.TryGetValue(endPoint, out session);
    }
    
    public void UpdateSessionsAliveStatus()
    {
        if (Robot != null)
        {
            UpdateSessionAliveStatus(Robot);
        }

        if (Operator != null)
        {
            UpdateSessionAliveStatus(Operator);
        }

        foreach (var session in _sessions
                     .Where(session => session.Value.Role == Role.Customer))
        {
            UpdateSessionAliveStatus(session.Value.IpEndPoint);
        }
    }

    private void UpdateSessionAliveStatus(IPEndPoint endPoint)
    {
        var isIpAddressExists = _sessions.TryGetValue(endPoint, out var session);
        
        if (!isIpAddressExists)
        {
            Console.WriteLine("Ip address is not exists");
            return;
        }
        
        var isSessionAlive = DateTime.UtcNow - session!.LastAliveTime < TimeSpan.FromSeconds(5);

        if (isSessionAlive)
        {
            return;
        }
        
        switch (session.Role)
        {
            case Role.Operator:
                Operator = null;
                break;
            case Role.Robot:
                Robot = null;
                break;
            case Role.Customer:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(session.Role), session.Role, null);
        }
        
        _sessions.Remove(session.IpEndPoint);
    }
 
}