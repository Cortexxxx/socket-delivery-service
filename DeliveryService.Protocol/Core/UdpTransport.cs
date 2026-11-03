using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Logger;
using DeliveryService.Protocol.Messages;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Protocol.Core;

public sealed class UdpTransport : IDisposable
{
    private readonly Socket _socket;
    private readonly Channel<ReceivedPacket> _receiveQueue;
    private readonly PacketCodec _packetCodec;
    private Task _receiveLoopTask = Task.CompletedTask;
    private CancellationTokenSource _receiveLoopCts = new();
    private readonly byte[] _sendBuffer = new byte[HeaderConstants.MaxPacketSize];
    private readonly IPacketLogger? _logger;
    
    public UdpTransport(int port, PacketCodec packetCodec, IPacketLogger? logger)
    {
        _packetCodec = packetCodec;
        _logger = logger;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Bind(new IPEndPoint(IPAddress.Any, port));
        
        _socket.IOControl(-1744830452, [0, 0, 0, 0], null);
        _receiveQueue = Channel.CreateUnbounded<ReceivedPacket>();
    }
    
    public void Start()
    {
        _receiveLoopCts = new CancellationTokenSource();
        _receiveLoopTask = Task.Run(() => ReceiveLoopAsync(_receiveLoopCts.Token));
    }

    public void Stop()
    {
        _receiveLoopCts.Cancel();
        _receiveQueue.Writer.TryComplete();
        _receiveLoopTask.Wait();
        _socket.Close();
    }
    
    public bool TrySend(ushort seq, IPEndPoint address, IMessage message, 
        [NotNullWhen(true)] out PacketHeader? header, out ReadOnlySpan<byte> messageBytes)
    {
        var trySerializePacket = _packetCodec.TrySerializePacket(
            _sendBuffer, seq, message, out var written, out header);
        messageBytes = _sendBuffer.AsSpan(0, written);
        
        return trySerializePacket 
               && TrySendRaw(address, _sendBuffer.AsSpan(0, written));
    }

    public bool TryReceive(out ReceivedPacket packet) => _receiveQueue.Reader.TryRead(out packet);
    
    public void Dispose()
    {
        Stop();
    }
    
    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        var buffer = new byte[ProtocolConstants.BufferSize];
        
        while (!token.IsCancellationRequested)
        {
            try
            {
                EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                var receiveResult = await _socket.ReceiveFromAsync(buffer, endPoint, token);

                _logger?.LogReceive((IPEndPoint)receiveResult.RemoteEndPoint, 
                    buffer.AsSpan(0, receiveResult.ReceivedBytes));

                var tryParseResult = _packetCodec.TryParsePacket(
                    buffer.AsSpan(0, receiveResult.ReceivedBytes),
                    out var packet, out var errorCode);

                if (!tryParseResult)
                {
                    if (errorCode != null)
                    {
                        // Inform that error
                    }

                    continue;
                }

                _receiveQueue.Writer.TryWrite(
                    new ReceivedPacket((IPEndPoint)receiveResult.RemoteEndPoint, packet!));
            }
            catch (SocketException e) when (e.SocketErrorCode == SocketError.MessageSize)
            {
                Console.WriteLine(e);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                break;
            }
        }
    }

    public bool TrySendRaw(IPEndPoint destination, ReadOnlySpan<byte> pendingMessageBytes)
    {
        try
        {
            _socket.SendTo(pendingMessageBytes, destination);
            _logger?.LogSend(destination, pendingMessageBytes);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }
}
