using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class MessageAttribute(MessageType messageType) : Attribute
{
    public PacketFlags Flags { get; set; }
    public MessageType MessageType { get; } = messageType;
}