using System.Net;
using DeliveryService.Protocol.Attributes;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Messages;

namespace DeliveryService.Server;

public class ServerHandlers
{
    private readonly ServerSessions _serverSessions;
    private readonly ServerSender _serverSender;
    private readonly MapMessage _mapMessage;
    public ServerHandlers(ServerSessions serverSessions, ServerSender serverSender, MapMessage mapMessage)
    {
        _mapMessage = mapMessage;
        _serverSessions = serverSessions;
        _serverSender = serverSender;
    }
    
    [MessageHandler]
    public void OnHello(IPEndPoint sender, HelloMessage message)
    {
        Console.WriteLine($"Yahoo! Hello my little {sender}");
        
        var trySetResult = message.Role switch
        {
            Role.Operator => _serverSessions.TrySetSingleton(sender, Role.Operator),
            Role.Customer => _serverSessions.TrySetCustomer(sender),
            Role.Robot => _serverSessions.TrySetSingleton(sender, Role.Robot),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        if (!trySetResult)
        {
            _serverSender.SendTo(sender, new ErrorMessage()
            {
                ErrorCode = ErrorCode.RoleTaken
            });
        }
        else
        {
            _serverSender.SendTo(sender, _mapMessage);
        }
        
        Console.WriteLine($"My little {sender} your connection status: {trySetResult}");
    }
    
    [MessageHandler]
    public void OnTelemetry(IPEndPoint sender, TelemetryMessage message)
    {
        Console.WriteLine($"My little {sender}, you are moving really cool! {message.Position}");
        var tryGetResult = _serverSessions.TryGetSessionByAddress(sender, out var session);
    }
}