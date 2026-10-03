namespace DeliveryService.Protocol;

public interface IMessage
{
    bool TrySerialize(Span<byte> buffer, out int written);
}