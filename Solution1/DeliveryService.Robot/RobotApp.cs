using System.Diagnostics;
using System.Net;
using DeliveryService.Protocol.Core;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Helpers;
using DeliveryService.Protocol.Messages;
using DeliveryService.Protocol.Models;
using DeliveryService.Protocol.Network;

namespace DeliveryService.Robot;

public class RobotApp(
    UdpTransport udpTransport,
    MessageSender robotSender,
    RobotState state,
    ReceiveProcessor receiveProcessor, 
    IPEndPoint serverEndpoint) : AppLoop(udpTransport, receiveProcessor)
{
    private readonly IntervalTimer _telemetryTimer = 
        new(TimeSpan.FromMilliseconds(RobotConfig.SendTelemetryIntervalMilliseconds));
    private readonly IntervalTimer _helloTimer = 
        new(TimeSpan.FromSeconds(NetworkConfig.HelloRetryIntervalSeconds));
    private readonly IntervalTimer _driveTimer = 
        new(TimeSpan.FromMilliseconds(RobotConfig.DriveIntervalMilliseconds));
    private readonly IntervalTimer _renderTimer = 
        new(TimeSpan.FromMilliseconds(RobotConfig.RenderIntervalMilliseconds));

    protected override void OnStart()
    {
        Console.Clear();
    }

    protected override void Tick()
    {
        if (_renderTimer.IsDue())
        {
            ConsoleView.Render(state.MapState, state.Position, state.Angle, 
                [], null, state.LastEvent);
        }

        if (state.IsRegistered)
        {
            SendTelemetryIfDue();

            if (!state.CanDrive) return;
            
            if (CheckDeadManSwitch())
            {
                state.CanDrive = false;
                state.LastEvent = "Operator is dead!";
            }
            else
            {
                DriveIfDue();
            }
        }
        else
        {
            SendHelloIfDue();
        }
    }
    
    protected override bool Accept(ReceivedPacket receivedPacket)
    {
        return receivedPacket.Sender.Equals(serverEndpoint);
    }

    private bool CheckDeadManSwitch()
    {
        return Stopwatch.GetElapsedTime(state.LastDriveReceived, Stopwatch.GetTimestamp()) 
               > TimeSpan.FromMilliseconds(RobotConfig.DeadManSwitchIntervalMilliseconds);
    }
    
    private void SendHelloIfDue()
    {
        if (_helloTimer.IsDue()) robotSender.SendTo(serverEndpoint, new HelloMessage
        {
            Role = Role.Robot
        });
    }

    private void DriveIfDue()
    {
        if (_driveTimer.IsDue()) state.Drive();
    }
    
    private void SendTelemetryIfDue()
    {
        if (_telemetryTimer.IsDue()) robotSender.SendTo(serverEndpoint, new TelemetryMessage
        {
            Position = state.Position,
            Angle = state.Angle
        });
    }
}
