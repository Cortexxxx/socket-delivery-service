using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using DeliveryService.Client.Operator;
using DeliveryService.Protocol.Core;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Helpers;
using DeliveryService.Protocol.Messages;
using DeliveryService.Protocol.Models;
using DeliveryService.Protocol.Network;

namespace DeliveryService.Client;

public class ClientApp(
    UdpTransport udpTransport,
    MessageSender clientSender,
    ReceiveProcessor receiveProcessor, 
    ReliabilityLayer reliabilityLayer,
    ClientState state,
    IPEndPoint serverEndpoint) : AppLoop(udpTransport, receiveProcessor, reliabilityLayer)
{
    private readonly IntervalTimer _helloTimer 
        = new(TimeSpan.FromSeconds(NetworkConfig.HelloRetryIntervalSeconds));
    private readonly IntervalTimer _heartbeatTimer 
        = new(TimeSpan.FromSeconds(ClientConfig.HeartbeatIntervalSeconds));
    private readonly IntervalTimer _driveTickTimer 
        = new(TimeSpan.FromMilliseconds(ClientConfig.DriveTickIntervalMilliseconds));
    private readonly IntervalTimer _renderTimer = 
        new(TimeSpan.FromMilliseconds(ClientConfig.RenderIntervalMilliseconds));
    
    private MoveDirection _moveDirection = MoveDirection.None;
    private RotateDirection _rotateDirection = RotateDirection.None;

    private readonly StringBuilder _customerInput = new();
    private readonly ConcurrentQueue<string?> _customerEntriesQueue = new();
    
    protected override void OnStart()
    {
        Console.Clear();
    }

    protected override void Tick()
    {
        if (!state.IsRegistered)
        {
            SendHelloIfDue();
        }
        else
        {
            switch (state.Role)
            {
                case Role.Operator:
                    if (_driveTickTimer.IsDue()) OperatorTick();
                    break;
                case Role.Customer:
                    ClientTick();
                    break;
            }
        }
    }

    protected override bool Accept(ReceivedPacket receivedPacket) 
        => receivedPacket.Sender.Equals(serverEndpoint);

    private void OperatorTick()
    {
        if (Console.KeyAvailable)
        {
            var pressedKey = Console.ReadKey(true);
            
            switch (pressedKey.Key)
            {
                case ConsoleKey.W:
                    _moveDirection = MoveDirection.Forward;
                    break;
                case ConsoleKey.S:
                    _moveDirection = MoveDirection.Backward;
                    break;
                case ConsoleKey.D:
                    _rotateDirection = RotateDirection.Right;
                    break;
                case ConsoleKey.A:
                    _rotateDirection = RotateDirection.Left;
                    break;
                case ConsoleKey.Spacebar:
                    _moveDirection = MoveDirection.None;
                    _rotateDirection = RotateDirection.None;
                    break;
            }
        }

        clientSender.SendTo(serverEndpoint, new DriveMessage
        {
            Speed = (sbyte)((sbyte)_moveDirection * ClientConfig.SpeedPercentage),
            Rotate = (sbyte)((sbyte)_rotateDirection * ClientConfig.RotatePercentage)
        });
    }

    private void ClientTick()
    {
        ReadCustomerKeys();

        if (_renderTimer.IsDue())
        {
            ConsoleView.Render(state.MapState, state.Position, 
                state.Angle, state.Orders.Values, state.CurrentOrder,
                state.LastEvent, $"Enter delivery point (x y): {_customerInput}");
        }

        if (_heartbeatTimer.IsDue()) clientSender.SendTo(serverEndpoint, new HeartbeatMessage());

        if (!_customerEntriesQueue.TryDequeue(out var input) || input == null) return;
        
        if (TryParseInputToMapPoint(input, out var deliveryPoint)) SendOrderCreate(deliveryPoint);
    }

    private void ReadCustomerKeys()
    {
        while (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Enter)
            {
                _customerEntriesQueue.Enqueue(_customerInput.ToString());
                _customerInput.Clear();
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (_customerInput.Length > 0) _customerInput.Length--;
            }
            else if (!char.IsControl(key.KeyChar))
            {
                _customerInput.Append(key.KeyChar);
            }
        }
    }

    private void SendOrderCreate(Point<byte> deliveryPoint)
    {
        clientSender.SendTo(serverEndpoint, new OrderCreateMessage
        {
            X = deliveryPoint.X,
            Y = deliveryPoint.Y
        });
    }

    private bool TryParseInputToMapPoint(string input, 
        [NotNullWhen(true)] out Point<byte>? point)
    {
        point = null;
        var parts = input.Split(' ');
        
        if (parts.Length != 2) return false;
        
        var parseXResult = byte.TryParse(parts[0], out var x); 
        var parseYResult = byte.TryParse(parts[1], out var y);
        
        if (!parseXResult || !parseYResult)
        {
            state.LastEvent = "Incorrect point";
            return false;
        }

        point = new Point<byte>(x, y); 
        
        return true;
    }

    private void SendHelloIfDue()
    {
        if (_helloTimer.IsDue())
        {
            clientSender.SendTo(serverEndpoint, new HelloMessage { Role = state.Role });
        }
    }
}