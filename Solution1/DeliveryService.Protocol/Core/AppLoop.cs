using DeliveryService.Protocol.Models;
using DeliveryService.Protocol.Network;

namespace DeliveryService.Protocol.Core;

public abstract class AppLoop(UdpTransport udpTransport, 
    ReceiveProcessor receiveProcessor, 
    ReliabilityLayer reliabilityLayer)
{
    public bool Started { get; private set; }
    
    /// <summary>
    /// Runs loop in main thread with Thread.Sleep() inside
    /// </summary>
    public void RunLoop()
    {
        OnStart();
        
        while (true)
        {
            var isAnyPacketProceed = Step();
            
            if (!isAnyPacketProceed)
            {
                Thread.Sleep(NetworkConfig.ReceiveLoopIntervalMilliseconds);
            }
        }
    }

    public bool Step()
    {
        Tick();
        reliabilityLayer.Tick(DateTime.UtcNow);

        var wasPacketProceed = false;
        
        for (int i = 0; i < NetworkConfig.MaxPacketsOnTick; i++)
        {
            if (!udpTransport.TryReceive(out var receivedPacket))
            {
                return wasPacketProceed;
            }
            
            if (!PacketLossSimulator.TryLosePacket(NetworkConfig.LosePacketChance) 
                && Accept(receivedPacket))
            {
                receiveProcessor.ProcessPacket(receivedPacket);
                wasPacketProceed = true;
            }
        }

        return wasPacketProceed;
    }

    public virtual void OnStart()
    {
        Started = true;
    }
    
    protected abstract void Tick();
    
    protected virtual bool Accept(ReceivedPacket receivedPacket) => true;

    public void Stop() => udpTransport.Stop();
}
