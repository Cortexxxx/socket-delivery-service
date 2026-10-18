namespace DeliveryService.Protocol.Network;

public static class NetworkConfig
{
    // Network
    
    public const string ServerHost = "127.0.0.1";
    public const int ServerPort = 9000;
    public const int AnyPort = 0;
    
    // Intervals
    
    public const int ReceiveLoopIntervalMilliseconds = 10;
    public const int HelloRetryIntervalSeconds = 2;
    public const int TryPendIntervalMilliseconds = 200;
    
    // Reliability
    
    public const float LosePacketChance = 0.05f;
    public const int MaxPendingAttempts = 5;
    public const int ReceivedSeqLifetimeSeconds = TryPendIntervalMilliseconds * (MaxPendingAttempts + 1) * 2;
    public const int MaxPacketsOnTick = 50;
}