using System.Diagnostics.CodeAnalysis;
using System.Net;
using DeliveryService.Protocol.Models;
using DeliveryService.Protocol.Network;

namespace DeliveryService.Protocol.Core;

public sealed class ReliabilityLayer(UdpTransport udpTransport)
{
    private readonly Dictionary<PacketKey, PendingPacket> _packetsAwaitingAck = new();
    private readonly Dictionary<PacketKey, DateTime> _receivedPacketTimes = new();
    private readonly Dictionary<IPEndPoint, ushort> _latestSeqByEndPoint = new();

    public bool TryAddPendingPacket(ushort seq, IPEndPoint destination, byte[] packetBytes)
    {
        return _packetsAwaitingAck.TryAdd(new PacketKey(destination, seq),
            new PendingPacket(destination, seq, packetBytes, DateTime.UtcNow, 0));
    }

    public bool TryRemovePendingPacket(ushort seq, IPEndPoint destination)
    {
        return _packetsAwaitingAck.Remove(new PacketKey(destination, seq));
    }

    public bool TryRegisterResendAttempt(ushort seq, IPEndPoint destination)
    {
        if (!_packetsAwaitingAck.TryGetValue(new PacketKey(destination, seq), out var pendingPacket))
        {
            return false;
        }

        if (pendingPacket.Attempts >= NetworkConfig.MaxPendingAttempts)
        {
            TryRemovePendingPacket(seq, destination);
            return false;
        }

        pendingPacket.Attempts++;
        pendingPacket.LastSentAt = DateTime.UtcNow;

        return true;
    }

    public void Tick(DateTime now)
    {
        var resendInterval = TimeSpan.FromMilliseconds(NetworkConfig.TryPendIntervalMilliseconds);
        var packetsToResend = _packetsAwaitingAck.Values
            .Where(p => now - p.LastSentAt > resendInterval)
            .ToList();

        foreach (var packet in packetsToResend)
        {
            if (TryRegisterResendAttempt(packet.Seq, packet.Destination))
                udpTransport.TrySendRaw(packet.Destination, packet.Bytes);
        }

        var receivedLifetime = TimeSpan.FromSeconds(NetworkConfig.ReceivedSeqLifetimeSeconds);
        var expiredKeys = _receivedPacketTimes
            .Where(p => now - p.Value > receivedLifetime)
            .Select(p => p.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _receivedPacketTimes.Remove(key);
        }
    }

    public bool TryMarkPacketAsReceived(ushort seq, IPEndPoint sender, DateTime receivedAt)
    {
        return _receivedPacketTimes.TryAdd(new PacketKey(sender, seq), receivedAt);
    }

    public bool IsPacketAlreadyReceived(ushort seq, IPEndPoint sender)
    {
        return _receivedPacketTimes.ContainsKey(new PacketKey(sender, seq));
    }

    public bool IsStaleSeq(IPEndPoint sender, ushort seq)
    {
        return _latestSeqByEndPoint.TryGetValue(sender, out var latestSeq)
               && (short)(latestSeq - seq) > 0;
    }

    public void UpdateLatestSeq(IPEndPoint sender, ushort seq)
    {
        _latestSeqByEndPoint[sender] = seq;
    }
}
