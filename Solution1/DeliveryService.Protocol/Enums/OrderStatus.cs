namespace DeliveryService.Protocol.Enums;

public enum OrderStatus : byte
{
    Pending = 0x01,
    PickingUp = 0x02,
    InTransit = 0x03,
    Delivered = 0x04,
}