namespace DeliveryService.Protocol.Interfaces;

public interface IMessage
{
    bool TrySerialize(Span<byte> buffer, out int written);
}