using DeliveryService.Protocol.Messages;
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
    public const string StaticHandler =
        "Handler method must be an instance method";
    public const string NonVoidHandlerReturnType =
        "Handler method must return void";
    public const string IncorrectHandlerParameters =
        "Handler method must have IPEndPoint, T parameters where T : class, IMessage";
    public const string MissingMessageAttribute =
        "Handler message type must have the Message attribute";
}