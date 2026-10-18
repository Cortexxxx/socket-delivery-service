using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Client.GUI;

public partial class StartWindow : Window
{
    public event Action<Role>? OnRoleChosen;
    public StartWindow()
    {
        InitializeComponent();
    }

    private void OnOperatorClick(object? sender, RoutedEventArgs e)
    {
        SetButtonsEnabled(false);
        OnRoleChosen?.Invoke(Role.Operator);
    }

    private void OnCustomerClick(object? sender, RoutedEventArgs e) 
    {
        SetButtonsEnabled(false);
        OnRoleChosen?.Invoke(Role.Customer);
    }

    private void SetButtonsEnabled(bool enabled)
    {
        OperatorButton.IsEnabled = enabled;
        CustomerButton.IsEnabled = enabled;
    }
}
