namespace DeliveryService.Protocol.Constants;

public static class MessagesConstants
{
    // Drive

    public const int DrivePayloadSize = 2;
    public const sbyte DriveMaxValue = 100;
    
    // Ack

    public const int AckPayloadSize = 2;
    
    // Hello
    
    public const int HelloPayloadSize = 1;
    
    // Error

    public const int ErrorPayloadSize = 1;
    
    // OrderCreate

    public const int OrderCreatePayloadSize = 2;
    
    // OrderStatus

    public const int OrderStatusPayloadSize = 5;
    public const int OrderStatusIdOffset = 1;
    public const int OrderStatusEndpointOffset = 3;
    
    // Telemetry
    
    public const int TelemetryPayloadSize = 12; 
    public const int TelemetryYPositionOffset = 4; 
    public const int TelemetryAngleOffset = 8; 
    
    // Map
}