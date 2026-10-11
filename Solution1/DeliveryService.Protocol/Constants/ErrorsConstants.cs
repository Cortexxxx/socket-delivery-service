namespace DeliveryService.Protocol.Constants;

public static class ErrorsConstants
{
    public const string IncorrectTryParseParameters =
        "TryParse method must have ReadOnlySpan, T? where T : IMessage parameters";
    public const string NonPublicTryParseSignature =
        "TryParse method must be public";
    public const string UnknownMessage =
        "Unknown message";
    public const string MissingTryParse =
        "Message must have TryParse method";
    public const string NonBoolTryParseReturnType =
        "TryParse method must return bool value";
    public const string NonStaticTryParse =
        "TryParse method must be static";
    public const string DuplicateMessageType =
        "Two or more messages with the same key exist";
}