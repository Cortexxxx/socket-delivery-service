namespace DeliveryService.Protocol;

public static class Constants
{
    public static readonly byte[] Magic = [0x52, 0x44];

    public const int MaxPacketSize = 1200;
    public const int MaxPayloadSize = 1192;
    public const int HeaderSize = 8;
    // Offsets

    public const int MagicOffset = 0;
    public const int SeqOffset = 2;
    public const int FlagsOffset = 4;
    public const int TypeOffset = 5;
    public const int PayloadLengthOffset = 6;

}