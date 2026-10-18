using System.Net;
using DeliveryService.Client.Core.Operator;
using DeliveryService.Protocol.Core;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Helpers;
using DeliveryService.Protocol.Messages;
using DeliveryService.Protocol.Models;
using DeliveryService.Protocol.Network;

namespace DeliveryService.Client.Core;

public class ClientApp(
    UdpTransport udpTransport,
    MessageSender clientSender,
    ReceiveProcessor receiveProcessor, 
    ReliabilityLayer reliabilityLayer,
    ClientState state,
    IPEndPoint serverEndpoint) : AppLoop(udpTransport, receiveProcessor, reliabilityLayer)
{
    public readonly ClientState State = state;
    private readonly IntervalTimer _helloTimer 
        = new(TimeSpan.FromSeconds(NetworkConfig.HelloRetryIntervalSeconds));
    private readonly IntervalTimer _heartbeatTimer 
        = new(TimeSpan.FromSeconds(ClientConfig.HeartbeatIntervalSeconds));
    private readonly IntervalTimer _driveTickTimer 
        = new(TimeSpan.FromMilliseconds(ClientConfig.DriveTickIntervalMilliseconds));

    
    private MoveDirection _moveDirection = MoveDirection.None;
    private RotateDirection _rotateDirection = RotateDirection.None;

    protected override void Tick()
    {
        if (!State.IsRegistered)
        {
            SendHelloIfDue();
        }
        else
        {
            switch (State.Role)
            {
                case Role.Operator:
                    if (_driveTickTimer.IsDue())
                    {
                        SendDrive();
                    }
                    break;
                case Role.Customer:
                    ClientTick();
                    break;
            }
        }
    }

    protected override bool Accept(ReceivedPacket receivedPacket) 
        => receivedPacket.Sender.Equals(serverEndpoint);

    public void SetMoveDirection(MoveDirection moveDirection) => _moveDirection = moveDirection;
    
    public void SetRotateDirection(RotateDirection rotateDirection) => _rotateDirection = rotateDirection;

    private void SendDrive()
    {
        clientSender.SendTo(serverEndpoint, new DriveMessage
        {
            Speed = (sbyte)((sbyte)_moveDirection * ClientConfig.SpeedPercentage),
            Rotate = (sbyte)((sbyte)_rotateDirection * ClientConfig.RotatePercentage)
        });
    }
    
    private void ClientTick()
    {
        if (_heartbeatTimer.IsDue()) clientSender.SendTo(serverEndpoint, new HeartbeatMessage());
    }

    public void SendOrderCreate(Point<byte> deliveryPoint)
    {
        clientSender.SendTo(serverEndpoint, new OrderCreateMessage
        {
            X = deliveryPoint.X,
            Y = deliveryPoint.Y
        });
    }

    private void SendHelloIfDue()
    {
        if (_helloTimer.IsDue())
        {
            clientSender.SendTo(serverEndpoint, new HelloMessage { Role = State.Role });
        }
    }
}