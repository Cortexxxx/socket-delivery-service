
namespace DeliveryService.Robot;

public static class RobotConfig
{
    // Intervals
    
    public const int SendTelemetryIntervalMilliseconds = 16;
    public const int DriveIntervalMilliseconds = 50;
    public const int DeadManSwitchIntervalMilliseconds = 300;
    
    // Movement
    
    public const int MaxSpeed = 2; // Meters per second
    public const int MaxRotateSpeed = 90; // Angles per second
}
