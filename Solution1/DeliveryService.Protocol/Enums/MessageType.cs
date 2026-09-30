namespace DeliveryService.Protocol.Enums;

public enum MessageType : byte
{
    Hello = 0x01,
    Heartbeat = 0x02,
    Ack = 0x03,
    Error = 0x04,
    Map = 0x10,
    Drive = 0x20,
    Stop = 0x21,
    Telemetry = 0x30,
    OrderCreate = 0x40,
    OrderStatus = 0x41
}