using DeliveryService.Protocol.Models;
using DeliveryService.Protocol.Network;

namespace DeliveryService.Protocol.Core;

/// <summary>
/// Common main loop of an application: do the periodic work, then take one received packet
/// (if any) and give it to <see cref="ReceiveProcessor"/>. Applications fill in the periodic work
/// and decide which packets they accept.
/// </summary>
public abstract class AppLoop(UdpTransport udpTransport, ReceiveProcessor receiveProcessor)
{
    public void Run()
    {
        OnStart();

        while (true)
        {
            Tick();

            if (!udpTransport.TryReceive(out var receivedPacket))
            {
                Thread.Sleep(NetworkConfig.ReceiveLoopIntervalMilliseconds);
                continue;
            }

            if (Accept(receivedPacket))
            {
                receiveProcessor.ProcessPacket(receivedPacket);
            }
        }
    }
    
    protected virtual void OnStart() { }
    
    protected abstract void Tick();
    
    protected virtual bool Accept(ReceivedPacket receivedPacket) => true;
}
