namespace DeliveryService.Protocol.Enums;

public enum ErrorCode : byte
{
    UnknownMessage = 0x01,
    BadPayload = 0x02,
    NotRegistered = 0x03,
    RoleTaken = 0x04,
    Forbidden = 0x05,
    InvalidPoint = 0x06,
    RobotOffline = 0x07,
}