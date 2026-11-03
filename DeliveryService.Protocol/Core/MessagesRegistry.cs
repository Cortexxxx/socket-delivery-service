using System.Reflection;
using DeliveryService.Protocol.Attributes;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Messages;

namespace DeliveryService.Protocol.Core;

// We need two delegates because a dictionary cannot store TryParse delegates
// as values of different generic types. The dictionary stores a general ParseFunc delegate that uses a closure
// to hold the concrete TryParse method
public delegate bool TypedParse<T>(ReadOnlySpan<byte> payload, out T? message) where T: class, IMessage; 
public delegate bool ParseFunc(ReadOnlySpan<byte> payload, out IMessage? message);

public sealed class MessagesRegistry
{
    private readonly Dictionary<MessageType, ParseFunc> _messageParses;
    private readonly Dictionary<Type, (MessageType messageType, PacketFlags flags)> _messageInfos;
    
    public MessagesRegistry(Assembly assembly)
    {
        _messageParses = new Dictionary<MessageType, ParseFunc>();
        _messageInfos = new Dictionary<Type, (MessageType messageType, PacketFlags flags)>();
        var messagesAttributes = assembly
            .GetTypes()
            .Select(t =>
            {
                var attribute = t.GetCustomAttribute<MessageAttribute>();
                return (type: t, attribute);
            })
            .Where(t => t.attribute != null);
        
        foreach (var messageInfo in messagesAttributes)
        {
            if (!Enum.IsDefined(messageInfo.attribute!.MessageType))
            {
                throw new Exception(ErrorsConstants.UnknownMessage);
            }

            var tryParseMethod = messageInfo.type.GetMethod("TryParse");
            
            CheckTryParseMethod(tryParseMethod, messageInfo);
            
            var messageType = messageInfo.attribute.MessageType;
            
            if (_messageParses.ContainsKey(messageType) || _messageInfos.ContainsKey(messageInfo.type))
            {
                throw new Exception(ErrorsConstants.DuplicateMessageType);
            }
            
            _messageParses[messageType] = CreateParseFunc(messageInfo.type, tryParseMethod!);
            _messageInfos[messageInfo.type] = (messageType, messageInfo.attribute.Flags);
        }
    }
    
    public bool TryParsePayload(MessageType messageType, 
        ReadOnlySpan<byte> payload, 
        out IMessage? message, 
        out ErrorCode? errorCode)
    {
        message = default;
        errorCode = default;
        
        if (!_messageParses.TryGetValue(messageType, out var tryParseDelegate))
        {
            errorCode = ErrorCode.UnknownMessage;
            return false;
        }
        
        if (tryParseDelegate.Invoke(payload, out message)) return true;
        
        errorCode = ErrorCode.BadPayload;
        
        return false;
    }
    
    public bool TryGetMessageInfo(IMessage message, 
        out (MessageType messageType, PacketFlags messageFlags) messageInfo)
    {
        messageInfo = default;
        var messageType = message.GetType();
        
        if (!_messageInfos.TryGetValue(messageType, out var info)) return false;
        
        messageInfo = info;
        
        return true;
    }

    /// <summary>
    /// Wraps a TypedParse method with a concrete message type as generic in a general ParseFunc delegate.
    /// The dictionary that stores all messages requires ParseFunc without a generic type.
    /// This method uses a closure, so it stores the concrete TryParse method inside.
    /// </summary>
    private static ParseFunc Wrap<T>(TypedParse<T> typedParse) where T : class, IMessage
    {
        return (ReadOnlySpan<byte> payload, out IMessage? message) =>
        {
            var isTryParseSuccessful = typedParse.Invoke(payload, out var typed);
            message = typed;
            return isTryParseSuccessful;
        };
    }
    
    /// <summary>
    /// Returns a ParseFunc delegate for a TryParse method.
    /// Builds a TypedParse delegate and wraps it in a ParseFunc delegate.
    /// Uses reflection once on application start for each message type in the project.
    /// </summary>
    /// <param name="messageType">Type of the message</param>
    /// <param name="tryParse">TryParse method from message class</param>
    /// <returns>General ParseFunc delegate with TryParse inside</returns>
    private static ParseFunc CreateParseFunc(Type messageType, MethodInfo tryParse)
    {
        var delegateType = typeof(TypedParse<>).MakeGenericType(messageType);
        var typedParseDelegate = Delegate.CreateDelegate(delegateType, tryParse);
        var wrapMethodInfo = typeof(MessagesRegistry).GetMethod(nameof(Wrap), 
            BindingFlags.NonPublic | BindingFlags.Static)!;
        var wrapGenericMethod = wrapMethodInfo.MakeGenericMethod(messageType);
        var parseFuncObject = wrapGenericMethod.Invoke(null, [typedParseDelegate]);
        return (ParseFunc)parseFuncObject;
    }
    
    private static void CheckTryParseMethod(MethodInfo? tryParseMethod, 
        (Type type, MessageAttribute? attribute) messageInfo)
    {
        if (tryParseMethod == null)
        {
            throw new Exception(ErrorsConstants.MissingTryParse);
        }
            
        if (tryParseMethod.ReturnType != typeof(bool))
        {
            throw new Exception(ErrorsConstants.NonBoolTryParseReturnType);
        }
            
        if (!tryParseMethod.IsStatic)
        {
            throw new Exception(ErrorsConstants.NonStaticTryParse);
        }
            
        if (!tryParseMethod.IsPublic)
        {
            throw new Exception(ErrorsConstants.NonPublicTryParseSignature);
        }
            
        var methodParams = tryParseMethod.GetParameters();

        if (methodParams.Length != 2)
        {
            throw new Exception(ErrorsConstants.IncorrectTryParseParameters);
        }
            
        var payloadParamType = methodParams[0].ParameterType;
        var messageParamType = methodParams[1].ParameterType;
            
        if (payloadParamType != typeof(ReadOnlySpan<byte>) 
            || !messageParamType.IsByRef || !methodParams[1].IsOut 
            || typeof(IMessage).IsAssignableFrom(messageParamType.GetElementType()) == false 
            || messageParamType.GetElementType() != messageInfo.type)
        {
            throw new Exception(ErrorsConstants.IncorrectTryParseParameters);
        }
    }
}