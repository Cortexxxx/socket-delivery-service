namespace DeliveryService.Server;

public static class ServerConfig
{
    // Intervals
    
    public const int CheckTimeoutIntervalSeconds = 1;
    public const int SessionTimeoutSeconds = 5;
    
    // Map
    
    public const string MapPath = "map.txt";
    public const char MapObstacleChar = '#';
    public const char MapStorageChar = '*';
}