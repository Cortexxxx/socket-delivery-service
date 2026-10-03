namespace DeliveryService.Protocol.Enums;

[Flags]
public enum PacketFlags : byte
{
    None = 0x00,
    NeedAck = 0x01,
}