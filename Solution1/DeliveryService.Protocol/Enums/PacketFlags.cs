namespace DeliveryService.Protocol.Enums;

[Flags]
public enum PacketFlags : byte
{
    Ack = 0x01,
}