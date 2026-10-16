using DeliveryService.Protocol.Models;

namespace DeliveryService.Protocol.Map;

public interface IMap
{
    bool TryLoad(out bool[,]? map, out Point<byte>? storagePoint);
}