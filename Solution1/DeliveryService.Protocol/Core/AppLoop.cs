using DeliveryService.Protocol.Models;
using DeliveryService.Protocol.Network;

namespace DeliveryService.Protocol.Core;

public abstract class AppLoop(UdpTransport udpTransport, 
    ReceiveProcessor receiveProcessor, 
    ReliabilityLayer reliabilityLayer)
{
    public void Run()
    {
        OnStart();

        while (true)
        {
            Tick();
            reliabilityLayer.Tick(DateTime.UtcNow);
            
            if (!udpTransport.TryReceive(out var receivedPacket))
            {
                Thread.Sleep(NetworkConfig.ReceiveLoopIntervalMilliseconds);
                continue;
            }
            
            if (!PacketLossSimulator.TryLosePacket(NetworkConfig.LosePacketChance) 
                && Accept(receivedPacket))
            {
                    receiveProcessor.ProcessPacket(receivedPacket);
            }
        }
    }
    
    protected virtual void OnStart() { }
    
    protected abstract void Tick();
    
    protected virtual bool Accept(ReceivedPacket receivedPacket) => true;
}
