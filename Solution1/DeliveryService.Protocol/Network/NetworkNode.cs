using DeliveryService.Protocol.Core;
namespace DeliveryService.Protocol.Network;

/// <summary>
/// Ready-to-use network stack of an application: socket, sending, and received packets processing.
/// </summary>
public record NetworkNode(
    UdpTransport Transport,
    MessageSender Sender,
    ReceiveProcessor ReceiveProcessor, ReliabilityLayer ReliabilityLayer);
