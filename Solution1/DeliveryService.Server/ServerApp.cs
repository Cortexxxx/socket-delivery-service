using DeliveryService.Protocol;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Messages;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Server;

public class ServerApp(
    UdpTransport udpTransport,
    MessageSender serverSender,
    ServerSessions serverSessions,
    ReceiveProcessor receiveProcessor) : AppLoop(udpTransport, receiveProcessor)
{
    private readonly IntervalTimer _checkTimer = new(TimeSpan.FromSeconds(ServerConfig.CheckTimeoutIntervalSeconds));

    protected override void Tick()
    {
        CheckSessionTimeout();
    }

    protected override bool Accept(ReceivedPacket receivedPacket)
    {
        if (TryRejectUnregistered(receivedPacket, out var session)) return false;

        if (session != null)
        {
            session.LastAliveTime = DateTime.UtcNow;
        }

        return true;
    }

    private bool TryRejectUnregistered(ReceivedPacket receivedPacket, out Session? session)
    {
        var isRegistered = serverSessions.TryGetSessionByAddress(receivedPacket.Sender, out session);

        if (isRegistered) return false;
        if (receivedPacket.Packet.PacketHeader.MessageType == MessageType.Hello) return false;
        
        serverSender.SendTo(receivedPacket.Sender, new ErrorMessage
        {
            ErrorCode = ErrorCode.NotRegistered
        });
        
        return true;
    }

    private void CheckSessionTimeout()
    {
        if (_checkTimer.IsDue())
        {
            serverSessions.UpdateSessionsAliveStatus();
        }
    }
}