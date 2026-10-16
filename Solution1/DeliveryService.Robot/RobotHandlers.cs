using System.Diagnostics;
using System.Net;
using DeliveryService.Protocol.Attributes;
using DeliveryService.Protocol.Map;
using DeliveryService.Protocol.Messages;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Robot;

public class RobotHandlers(RobotState state)
{

    [MessageHandler]
    public void OnAck(IPEndPoint sender, AckMessage message)
    {
    }

    [MessageHandler]
    public void OnMap(IPEndPoint sender, MapMessage message)
    {
        state.MapState = new MapState(message.GetBoolMatrixFromBitmask(), message.Bitmask, message.StoragePoint);
        var startPoint = Point<float>.ByteToCentralFloatPoint(message.StoragePoint);
        
        if (state.MapState.CanMove(startPoint))
        {
            state.MoveTo(startPoint);
            state.IsRegistered = true;
            state.LastEvent = $"Map received {message.Width}x{message.Height}";
        }
        else
        {
            throw new Exception("Robot start point is in wall");
        }
    }
    
    [MessageHandler]
    public void OnDrive(IPEndPoint sender, DriveMessage message)
    {
        state.LastDriveReceived = Stopwatch.GetTimestamp();
        state.CanDrive = true;
        state.ApplyMove(message.Speed, message.Rotate);
    }
    
    [MessageHandler]
    public void OnError(IPEndPoint sender, ErrorMessage message)
    {
        state.LastEvent = $"Error: {message.ErrorCode}";
    }
}
