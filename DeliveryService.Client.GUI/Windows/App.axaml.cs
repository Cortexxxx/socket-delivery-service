using System;
using System.Net;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DeliveryService.Client.Core;
using DeliveryService.Client.GUI.Windows;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Network;

namespace DeliveryService.Client.GUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var startWindow = new StartWindow();
            
            startWindow.OnRoleChosen += role =>
            {
                var mainWindow = new MainWindow(CreateApp(role, startWindow.ServerHost, startWindow.ServerPort));
                mainWindow.KeyDown += mainWindow.KeyDownHandle;
                mainWindow.KeyUp += mainWindow.KeyUpHandle;
                mainWindow.Show();
                desktop.MainWindow = mainWindow;
                startWindow.Close();
            };
            
            desktop.MainWindow = startWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private ClientApp CreateApp(Role role, 
        string serverHost = NetworkConfig.LocalServerHost, int serverPort = NetworkConfig.ServerPort)
    {
        IPEndPoint serverEndpoint = new(IPAddress.Parse(serverHost), serverPort);
        
        var clientState = new ClientState
        {
            Role = role
        };

        var node = NetworkNodeFactory.Create(NetworkConfig.AnyPort, 
            _ => new ClientHandlers(clientState));

        var clientApp = new ClientApp(node.Transport, 
            node.Sender, 
            node.ReceiveProcessor, 
            node.ReliabilityLayer,
            clientState, 
            serverEndpoint);
        
        node.Transport.Start();
        clientApp.OnStart();
        return clientApp;
    }
    
    
}