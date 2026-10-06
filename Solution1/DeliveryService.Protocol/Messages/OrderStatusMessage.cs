using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Protocol.Messages;

[Message(MessageType.OrderStatus, Flags = PacketFlags.NeedAck )]
public sealed class OrderStatusMessage : IMessage
{
    public OrderStatus Status { get; init; }
    public ushort OrderId { get; init; }
    public Point<byte> Endpoint { get; init; }
    
    public static bool TryParse(ReadOnlySpan<byte> payload, [NotNullWhen(true)] out OrderStatusMessage? message)
    {
        message = null;

        if (payload.Length != MessagesConstants.OrderStatusPayloadSize)
        {
            return false;
        }

        var orderStatus = (OrderStatus)payload[0];

        if (!Enum.IsDefined(orderStatus))
        {
            return false;
            
        }


        var orderId = BinaryPrimitives.ReadUInt16LittleEndian(payload[MessagesConstants.OrderStatusIdOffset..MessagesConstants.OrderStatusEndpointOffset]);
        var x = payload[MessagesConstants.OrderStatusEndpointOffset];
        var y = payload[MessagesConstants.OrderStatusEndpointOffset+1];
            
        message = new OrderStatusMessage()
        {
            Status = orderStatus,
            OrderId = orderId,
            Endpoint = new Point<byte>() {X = x, Y = y}
        };
        
        return true;
    }
    
    public bool TrySerialize(Span<byte> buffer, out int written)
    {
        written = 0;
        
        if (buffer.Length < MessagesConstants.OrderStatusPayloadSize)
        {
            return false;
        }

        if (!Enum.IsDefined(Status))
        {
            return false;
        }
        
        buffer[0] = (byte)Status;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer[MessagesConstants.OrderStatusIdOffset..MessagesConstants.OrderStatusEndpointOffset], OrderId);
        buffer[MessagesConstants.OrderStatusEndpointOffset] = Endpoint.X;
        buffer[MessagesConstants.OrderStatusEndpointOffset + 1] = Endpoint.Y;
        
        written = MessagesConstants.OrderStatusPayloadSize;
        
        return true;
    }
}