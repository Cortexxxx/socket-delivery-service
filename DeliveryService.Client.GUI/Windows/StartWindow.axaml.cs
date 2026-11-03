using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DeliveryService.Protocol.Enums;

namespace DeliveryService.Client.GUI.Windows;

public partial class StartWindow : Window
{
    public event Action<Role>? OnRoleChosen;
    public string ServerHost { get; set; }
    public int ServerPort { get; set; }
    
    public StartWindow()
    {
        InitializeComponent();
    }

    private void OnOperatorClick(object? sender, RoutedEventArgs e)
    {
        OnRoleClick(Role.Operator);
    }

    private void OnCustomerClick(object? sender, RoutedEventArgs e)
    {
        OnRoleClick(Role.Customer);
    }

    private void OnRoleClick(Role role)
    {
        SetButtonsEnabled(false);
        ServerHost = string.IsNullOrEmpty(HostBox.Text) ? "127.0.0.1" : HostBox.Text;
        ServerPort = int.TryParse(PortBox.Text.ToString(), out var port) ? port : 9000;
        
        OnRoleChosen?.Invoke(role);
    }

    private void SetButtonsEnabled(bool enabled)
    {
        OperatorButton.IsEnabled = enabled;
        CustomerButton.IsEnabled = enabled;
    }
}
