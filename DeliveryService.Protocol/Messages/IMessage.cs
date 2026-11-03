namespace DeliveryService.Protocol.Messages;

public interface IMessage
{
    bool TrySerialize(Span<byte> buffer, out int written);
}