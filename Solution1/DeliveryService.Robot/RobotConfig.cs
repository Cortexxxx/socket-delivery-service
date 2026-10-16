using DeliveryService.Protocol.Models;

namespace DeliveryService.Robot;

public class RobotConfig
{
    // Intervals
    
    public const int SendTelemetryIntervalMilliseconds = 50;
    public const int RenderIntervalMilliseconds = 200;
    public const int DriveIntervalMilliseconds = 50;
    public const int DeadManSwitchIntervalMilliseconds = 300;
    
    // Movement
    
    public const int MaxSpeed = 2; // Meters per second
    public const int MaxRotateSpeed = 20; // Angles per second
}
