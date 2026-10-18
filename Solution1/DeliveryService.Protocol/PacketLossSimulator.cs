namespace DeliveryService.Protocol;

public static class PacketLossSimulator
{
    private const int Seed = 150;
    private static readonly Random Random = new Random(Seed);
    public static int LostPacketsCounter { get; private set; }
    public static bool TryLosePacket(float chance)
    {
        var isPacketLost = Random.NextSingle() < chance;
        
        if (isPacketLost)
        {
            LostPacketsCounter++;
        }
        
        return isPacketLost;
    }
}