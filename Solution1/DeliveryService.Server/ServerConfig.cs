namespace DeliveryService.Server;

public class ServerConfig
{
    // Network
    
    public const int ServerPort = 9000;
    
    // Intervals
    
    public const int ReceiveLoopIntervalMilliseconds = 10;
    public const int CheckTimeoutIntervalSeconds = 1;
    
    
    // Map
    
    public const string MapPath = "map.txt";
    public const char MapObstacleChar = '#';
    public const char MapStorageChar = '*';
}