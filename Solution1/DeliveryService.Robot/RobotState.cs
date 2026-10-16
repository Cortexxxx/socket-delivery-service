using System.Diagnostics;
using DeliveryService.Protocol.Map;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Robot;

public class RobotState
{
    public bool IsRegistered { get; set; }
    public bool CanDrive { get; set; }
    public MapState? MapState { get; set; }
    public Point<float> Position { get; private set; } = new();
    public float Angle { get; private set; }
    public string LastEvent { get; set; } = "-";
    public long LastDriveReceived { get; set; }
    private int _speed;
    private int _rotate;
    private long _lastDriveStamp = Stopwatch.GetTimestamp();
    
    public void MoveTo(Point<float> target)
    {
        if (MapState != null && MapState.CanMove(target))
        {
            Position = target;
        }
        else
        {
            LastEvent = "Can't move in wall";
        }
    }

    public void Drive()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var dt = (float)Stopwatch.GetElapsedTime(_lastDriveStamp, currentTimestamp).TotalSeconds;
        
        if (TimeSpan.FromSeconds(dt) 
            > TimeSpan.FromMilliseconds(RobotConfig.DeadManSwitchIntervalMilliseconds))
        {
            _lastDriveStamp = currentTimestamp;
        }
        
        dt = Math.Min(dt, 0.1f);

        _lastDriveStamp = currentTimestamp;
        Angle += _rotate / 100f * RobotConfig.MaxRotateSpeed * dt;

        var angleRadians = double.DegreesToRadians(Angle);
        var speed = _speed / 100f * RobotConfig.MaxSpeed;
        var newXPosition = Position.X + (float)(speed * Math.Cos(angleRadians)) * dt;
        var newYPosition = Position.Y + (float)(speed * Math.Sin(angleRadians)) * dt;
        
        if (MapState != null && MapState.CanMove(new Point<float>(newXPosition, newYPosition)))
        {
            Position.X = newXPosition;
            Position.Y = newYPosition;
        }
    }

    public void ApplyMove(int speed, int rotate)
    {
        _speed = speed;
        _rotate = rotate;
    }
}
