using System.Net;
using System.Reflection;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol;

public delegate void HandleFunc(IPEndPoint sender, IMessage message);
public delegate void TypedHandle<in T>(IPEndPoint sender, T message) where T : class, IMessage;

/// <summary>
/// Routes received messages to instance handler methods of a handlers object by message type.
/// </summary>
public class MessagesDispatcher
{
    private readonly Dictionary<MessageType, HandleFunc> _messagesHandlers = new();
    
    /// <summary>
    /// Finds all public instance handlers declared in the handlers object's class,
    /// validates them and registers by message type.
    /// Uses reflection once on application start.
    /// </summary>
    /// <param name="handlersObject">Object whose methods will handle the messages</param>
    public MessagesDispatcher(object handlersObject)
    {
        var handlers = handlersObject.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttribute<MessageHandlerAttribute>() != null);

        foreach (var handler in handlers)
        {
            CheckHandlerMethod(handler);

            var messageType = handler.GetParameters()[1].ParameterType;
            var messageEnumType = messageType.GetCustomAttribute<MessageAttribute>()!.MessageType;
            var handleFunc = CreateHandleFunc(handlersObject, messageType, handler);

            var tryAdd = _messagesHandlers.TryAdd(messageEnumType, handleFunc);

            if (!tryAdd)
            {
                throw new Exception(ErrorsConstants.DuplicateMessageType);
            }
        }
    }

    public bool Dispatch(ReceivedPacket packet)
    {
        var handler = _messagesHandlers.GetValueOrDefault(packet.Packet.PacketHeader.MessageType);
        
        if (handler == null)
        {
            return false;
        }

        try
        {
            handler.Invoke(packet.Sender, packet.Packet.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// Throws an exception with a message if the handler is static,
    /// does not return void, does not take (IPEndPoint, T) parameters where T is a class message,
    /// or if T does not have the Message attribute.
    /// </summary>
    /// <param name="handler">Handler method</param>
    private static void CheckHandlerMethod(MethodInfo handler)
    {
        if (handler.IsStatic)
        {
            throw new Exception(ErrorsConstants.StaticHandler);
        }

        if (handler.ReturnType != typeof(void))
        {
            throw new Exception(ErrorsConstants.NonVoidHandlerReturnType);
        }

        var parameters = handler.GetParameters();

        if (parameters.Length != 2
            || parameters[0].ParameterType != typeof(IPEndPoint)
            || parameters[1].ParameterType.IsByRef
            || !parameters[1].ParameterType.IsClass
            || !typeof(IMessage).IsAssignableFrom(parameters[1].ParameterType))
        {
            throw new Exception(ErrorsConstants.IncorrectHandlerParameters);
        }

        if (parameters[1].ParameterType.GetCustomAttribute<MessageAttribute>() == null)
        {
            throw new Exception(ErrorsConstants.MissingMessageAttribute);
        }
    }

    /// <summary>
    /// Wraps a typed handler in a general HandleFunc delegate that casts the message to its concrete type.
    /// </summary>
    private static HandleFunc Wrap<TMessage>(TypedHandle<TMessage> typedHandle) where TMessage : class, IMessage
    {
        return (sender, message) =>
        {
            typedHandle.Invoke(sender, (TMessage)message);
        };
    }

    /// <summary>
    /// Builds a HandleFunc delegate bound to the handlers object for a handler method with a concrete message type.
    /// </summary>
    private static HandleFunc CreateHandleFunc(object handlersObject, Type messageType, MethodInfo handlerInfo)
    {
        var typedHandle = typeof(TypedHandle<>).MakeGenericType(messageType);
        var typedHandleDelegate = Delegate.CreateDelegate(typedHandle, handlersObject, handlerInfo);
        var wrapMethod = typeof(MessagesDispatcher).GetMethod(nameof(Wrap), BindingFlags.NonPublic | BindingFlags.Static)!;
        var wrapGenericMethod = wrapMethod.MakeGenericMethod(messageType);
        var handleFunc = wrapGenericMethod.Invoke(null, [typedHandleDelegate]);
        return (HandleFunc)handleFunc!;
    }   
}