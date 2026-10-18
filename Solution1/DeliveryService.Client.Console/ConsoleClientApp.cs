using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using DeliveryService.Client.Core;
using DeliveryService.Client.Core.Operator;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Helpers;
using DeliveryService.Protocol.Models;
using DeliveryService.Protocol.Network;

namespace DeliveryService.Client.Console;

public class ConsoleClientApp(ClientApp clientApp)
{
    private readonly StringBuilder _customerInput = new();
    private readonly ConcurrentQueue<string?> _customerEntriesQueue = new();

    private readonly IntervalTimer _renderTimer = 
        new(TimeSpan.FromMilliseconds(ConsoleConfig.RenderIntervalMilliseconds));

    public void Run()
    {
        clientApp.OnStart();
        
        while (true)
        {
            Tick();
            
            var isAnyPacketProceed = clientApp.Step();
            
            if (!isAnyPacketProceed)
            {
                Thread.Sleep(NetworkConfig.ReceiveLoopIntervalMilliseconds);
            }
        }
    }
    
    private void Tick()
    {
        switch(clientApp.State.Role)
        {
            case Role.Operator:
                OperatorTick();
                break;
            case Role.Customer:
                CustomerTick();
                break;
        }
    }

    private void CustomerTick()
    {
        if (_renderTimer.IsDue())
        {
            ConsoleView.Render(clientApp.State.MapState, clientApp.State.Position, 
                clientApp.State.Angle, clientApp.State.Orders.Values, clientApp.State.CurrentOrder,
                clientApp.State.LastEvent, $"Enter delivery point (x y): {_customerInput}");
        }
        
        ReadCustomerKeys();
        
        if (!_customerEntriesQueue.TryDequeue(out var input) || input == null) return;
        if (TryParseInputToMapPoint(input, out var deliveryPoint)) clientApp.SendOrderCreate(deliveryPoint);
    }
    
    private void OperatorTick()
    {
        if (_renderTimer.IsDue())
        {
            ConsoleView.Render(clientApp.State.MapState, clientApp.State.Position, 
                clientApp.State.Angle, clientApp.State.Orders.Values, clientApp.State.CurrentOrder,
                clientApp.State.LastEvent);
        }

        if (!System.Console.KeyAvailable) return;
        
        var pressedKey = System.Console.ReadKey(true);
        switch (pressedKey.Key)
        {
            case ConsoleKey.W:
                clientApp.SetMoveDirection(MoveDirection.Forward);
                break;
            case ConsoleKey.S:
                clientApp.SetMoveDirection(MoveDirection.Backward);
                break;
            case ConsoleKey.D:
                clientApp.SetRotateDirection(RotateDirection.Right);
                break;
            case ConsoleKey.A:
                clientApp.SetRotateDirection(RotateDirection.Left);
                break;
            case ConsoleKey.Spacebar:
                clientApp.SetRotateDirection(RotateDirection.None);
                clientApp.SetRotateDirection(RotateDirection.None);
                break;
        }
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
            clientApp.State.LastEvent = "Incorrect point";
            return false;
        }

        point = new Point<byte>(x, y); 
        
        return true;
    }
    
    private void ReadCustomerKeys()
    {
        while (System.Console.KeyAvailable)
        {
            var key = System.Console.ReadKey(true);

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
}