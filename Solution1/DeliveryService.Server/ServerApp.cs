using DeliveryService.Protocol;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Messages;

namespace DeliveryService.Server;

public class ServerApp
{
    private DateTime _lastCheckTime;
    private readonly UdpTransport _udpTransport;
    private readonly ServerSender _serverSender;
    private readonly ServerSessions _serverSessions;
    private readonly MessagesDispatcher _messagesDispatcher;
    private bool[,] _map;
    
    public ServerApp(UdpTransport udpTransport,
        ServerSender serverSender, 
        ServerSessions serverSessions, 
        MessagesDispatcher messagesDispatcher, bool[,] map)
    {
        _udpTransport = udpTransport;
        _serverSender = serverSender;
        _serverSessions = serverSessions;
        _messagesDispatcher = messagesDispatcher;
        _map = map;
        _lastCheckTime = DateTime.UtcNow;
    }

    public void Run()
    {
        while (true)
        {
            CheckSessionTimeout();
    
            var tryReceiveResult = _udpTransport.TryReceive(out var receivedPacket);
    
            if (!tryReceiveResult)
            {
                Thread.Sleep(ServerConfig.ReceiveLoopIntervalMilliseconds);
                continue;
            }

            ProcessPacket(receivedPacket);
        }
    }

    private void ProcessPacket(ReceivedPacket receivedPacket)
    {
        if (TryRejectUnregistered(receivedPacket, out var session)) return;
            
        if(session != null)
        {
            session!.LastAliveTime = DateTime.UtcNow;
        }
            
        SendAckIfNeeded(receivedPacket);
    
        _messagesDispatcher.Dispatch(receivedPacket);
    }

    private bool TryRejectUnregistered(ReceivedPacket receivedPacket, out Session? session)
    {
        var isRegistered = _serverSessions.TryGetSessionByAddress(receivedPacket.Sender, out session);

        if (isRegistered) return false;
        if (receivedPacket.Packet.PacketHeader.MessageType == MessageType.Hello) return false;
        
        _serverSender.SendTo(receivedPacket.Sender, new ErrorMessage
        {
            ErrorCode = ErrorCode.NotRegistered
        });
        
        return true;

    }

    private void SendAckIfNeeded(ReceivedPacket receivedPacket)
    {
        if (receivedPacket.Packet.PacketHeader.Flags.HasFlag(PacketFlags.NeedAck))
        {
            _serverSender.SendTo(receivedPacket.Sender, new AckMessage
            {
                Seq = receivedPacket.Packet.PacketHeader.Seq
            });
        }
    }

    private void CheckSessionTimeout()
    {
        if (_lastCheckTime + TimeSpan.FromSeconds(ServerConfig.CheckTimeoutIntervalSeconds) 
            >= DateTime.UtcNow) return;
        _serverSessions.UpdateSessionsAliveStatus();
        _lastCheckTime = DateTime.UtcNow;
    }
}