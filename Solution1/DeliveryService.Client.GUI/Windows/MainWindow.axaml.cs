using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using DeliveryService.Client.Core;
using DeliveryService.Client.Core.Operator;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Helpers;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Client.GUI;

public partial class MainWindow : Window
{
    private readonly ClientApp _clientApp;
    private readonly DispatcherTimer _dispatcherTimer;
    private readonly IntervalTimer _updateSidebarTimer = new(TimeSpan.FromMilliseconds(100));
    private readonly HashSet<Key> _pressedKeys = new();
    
    public MainWindow(ClientApp clientApp)
    {
        InitializeComponent();
        _clientApp = clientApp;
        RoleText.Text = clientApp.State.Role.ToString();
        Map.ClientState = _clientApp.State;
        
        _dispatcherTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        
        _dispatcherTimer.Tick += ClientTick;
        _dispatcherTimer.Tick += DisplayStatus;
        _dispatcherTimer.Tick += (_, _) => Map.UpdateSmoothing();
        
        
        if (clientApp.State.Role == Role.Operator)
        {
            _dispatcherTimer.Tick += (_, _) => SetDirection();
            HintText.Text = "W/S: drive, A/D: rotate";
            OrderPanel.IsVisible = false;
        }
        else
        {
            HintText.IsVisible = false;
        }
        
        _dispatcherTimer.Start();
        
        Closed += (_, _) => _dispatcherTimer.Stop();
        Closed += (_, _) => clientApp.Stop();
    }

    private void ClientTick(object? o, EventArgs eventArgs)
    {
        _clientApp.Step();
        if (_updateSidebarTimer.IsDue()) UpdateSidebar();
        Map.InvalidateVisual();
    }

    private void DisplayStatus(object? sender, EventArgs eventArgs) 
        => ConnectionStatus.Text = _clientApp.State.IsRegistered ? "Connected" : "Not connected";

    public void KeyDownHandle(object? sender, KeyEventArgs e)
    {
        e.Handled = true;
        _pressedKeys.Add(e.Key);
        Trace.WriteLine("Down " + e.Key);
    }
    
    public void KeyUpHandle(object? sender, KeyEventArgs e)
    {
        e.Handled = true;
        Trace.WriteLine("Up " + e.Key);
        _pressedKeys.Remove(e.Key);
    }

    private void SetDirection()
    {
        var moveDirection = (MoveDirection)((_pressedKeys.Contains(Key.W) ? 1 : 0) 
                            + (_pressedKeys.Contains(Key.S) ? -1 : 0));
        
        var rotateDirection = (RotateDirection)((_pressedKeys.Contains(Key.D) ? 1 : 0) 
                                                + (_pressedKeys.Contains(Key.A) ? -1 : 0));
        _clientApp.SetMoveDirection(moveDirection);
        _clientApp.SetRotateDirection(rotateDirection);
    }

    public void UpdateSidebar()
    {
        var state = _clientApp.State;
        RoleText.Text = $"{state.Role}";
        ConnectionStatus.Text = state.IsRegistered ? "Online" : "Offline";
        ConnectionDot.Fill = state.IsRegistered ? Brushes.MediumSeaGreen : Brushes.IndianRed;

        PositionText.Text = state.Position == null 
            ? "-" : $"{state.Position.X:F1}, {state.Position.Y:F1}";
        var normalAngle = state.Angle;
        while (normalAngle > 360) normalAngle -= 360;
        while (normalAngle <= -360) normalAngle += 360;
        AngleText.Text = state.Angle == null 
            ? "-" : $"{normalAngle:F0}";

        if (state.CurrentOrder != null)
        {
            var currentOrder = state.CurrentOrder;
            CurrentOrderText.Text = 
                $"#{currentOrder.Id} - {currentOrder.OrderStatus} | {currentOrder.DeliveryPoint}";
        }
        else
        {
            CurrentOrderText.Text = "-";
        }
        
        OrdersText.Text = string.Join('\n', state.Orders.Values.Select(o => 
            $"#{o.Id} - {o.OrderStatus} | {o.DeliveryPoint}"));
    }

    private void CreateOrderButton_Click(object? sender, RoutedEventArgs e)
    {
        if (OrderX.Value == null || OrderY.Value == null)
        {
            return;
        }
        
        _clientApp.SendOrderCreate(new Point<byte>((byte)(OrderX.Value!), (byte)(OrderY.Value!)));
        
        OrderX.Value = null;
        OrderY.Value = null;
    }
}