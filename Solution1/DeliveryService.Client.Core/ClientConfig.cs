namespace DeliveryService.Client.Core;

public static class ClientConfig
{
    // Intervals
    
    public const float HeartbeatIntervalSeconds = 0.2f;
    public const int DriveTickIntervalMilliseconds = 50;
    
    // Operator
    
    public const int SpeedPercentage = 100;
    public const int RotatePercentage = 100;
}