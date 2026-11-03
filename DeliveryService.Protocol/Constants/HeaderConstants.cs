namespace DeliveryService.Protocol.Constants;

public static class HeaderConstants
{
    public const int MaxPacketSize = HeaderSize + MaxPayloadSize;
    public const int MaxPayloadSize = 1192;
    public const int HeaderSize = 8;
    public const int SeqOffset = 2;
    public const int FlagsOffset = 4;
    public const int TypeOffset = 5;
    public const int PayloadLengthOffset = 6;
    public static readonly byte[] Magic = [0x52, 0x44];
}