using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DeliveryService.Client.Core;
using DeliveryService.Protocol.Constants;
using DeliveryService.Protocol.Enums;
using DeliveryService.Protocol.Models;

namespace DeliveryService.Client.GUI;

public class MapControl : Control
{
    // Gap in pixels: the same between tiles and from the panel edge to the first tile.
    private const double Gap = 3;
    private const double MaxCornerRadius = 8;
    private const double RobotImageSizeInCells = 1.2;
    private const double RobotImageAngleOffsetDegrees = 90;
    
    private static readonly IBrush PanelBrush = CreateBrush("#14808080");
    private static readonly IPen PanelPen = new Pen(CreateBrush("#26808080"), 1);
    private static readonly IBrush FloorBrush = CreateBrush("#1F808080");
    private static readonly IBrush WallBrush = CreateBrush("#64748B");
    private static readonly IBrush StorageFillBrush = CreateBrush("#3330A46C");
    private static readonly IPen StoragePen = new Pen(CreateBrush("#30A46C"), 2);
    private static readonly IBrush PendingBrush = CreateBrush("#94A3B8");
    private static readonly IBrush PendingSoftBrush = CreateBrush("#4094A3B8");
    private static readonly IBrush PickingUpBrush = CreateBrush("#F5A524");
    private static readonly IBrush PickingUpSoftBrush = CreateBrush("#40F5A524");
    private static readonly IBrush InTransitBrush = CreateBrush("#3B82F6");
    private static readonly IBrush InTransitSoftBrush = CreateBrush("#403B82F6");
    private static readonly IBrush ShadowBrush = CreateBrush("#33000000");

    private bool _isInitialized;
    private float _x;
    private float _y;
    private float _angle;
    private long _lastRenderTimestamp;
    
    private readonly Bitmap _robotImage =
        new(AssetLoader.Open(new Uri("avares://DeliveryService.Client.GUI/Assets/robot.png")));

    public MapControl()
    {
        RenderOptions.SetBitmapInterpolationMode(this, BitmapInterpolationMode.HighQuality);
    }

    public ClientState? ClientState { get; set; }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var mapState = ClientState?.MapState;
        if (mapState == null)
        {
            return;
        }

        var cell = Math.Min((Bounds.Width - Gap) / mapState.Width,
            (Bounds.Height - Gap) / mapState.Height);
        if (cell <= Gap)
        {
            return;
        }

        var panelWidth = cell * mapState.Width + Gap;
        var panelHeight = cell * mapState.Height + Gap;
        var panelX = (Bounds.Width - panelWidth) / 2;
        var panelY = (Bounds.Height - panelHeight) / 2;

        var originX = panelX + Gap / 2;
        var originY = panelY + Gap / 2;

        var cornerRadius = Math.Min(MaxCornerRadius, cell * 0.14);

        context.DrawRectangle(PanelBrush, PanelPen,
            new Rect(panelX, panelY, panelWidth, panelHeight),
            cornerRadius + 4, cornerRadius + 4);

        for (var i = 0; i < mapState.Width; i++)
        {
            for (var j = 0; j < mapState.Height; j++)
            {
                var brush = mapState.Map[i, j] ? WallBrush : FloorBrush;
                context.DrawRectangle(brush, null, GetTileRect(originX, originY, cell, i, j),
                    cornerRadius, cornerRadius);
            }
        }

        DrawStorage(context, mapState.StoragePoint, originX, originY, cell, cornerRadius);
        DrawOrders(context, originX, originY, cell);

        if (_isInitialized)
        {
            var robotCenter = new Point(originX + _x * cell, originY + _y * cell);

            // Soft shadow under the robot.
            context.DrawEllipse(ShadowBrush, null,
                new Point(robotCenter.X, robotCenter.Y + cell * 0.05),
                cell * 0.34, cell * 0.34);

            var angleDegrees = _angle + RobotImageAngleOffsetDegrees;
            var rotation = Matrix.CreateRotation(double.DegreesToRadians(angleDegrees));
            var transform = rotation * Matrix.CreateTranslation(robotCenter.X, robotCenter.Y);

            var size = cell * RobotImageSizeInCells;

            using (context.PushTransform(transform))
            {
                context.DrawImage(_robotImage,
                    new Rect(0, 0, _robotImage.Size.Width, _robotImage.Size.Height),
                    new Rect(-size / 2, -size / 2, size, size));
            }
        }
    }

    private static Rect GetTileRect(double originX, double originY, double cell, int i, int j) =>
        new(originX + i * cell + Gap / 2,
            originY + j * cell + Gap / 2,
            cell - Gap,
            cell - Gap);

    private static void DrawStorage(DrawingContext context, Point<byte> storagePoint,
        double originX, double originY, double cell, double cornerRadius)
    {
        var tile = GetTileRect(originX, originY, cell, storagePoint.X, storagePoint.Y);
        context.DrawRectangle(StorageFillBrush, StoragePen, tile, cornerRadius, cornerRadius);

        var boxSize = tile.Width * 0.35;
        var box = new Rect(tile.Center.X - boxSize / 2, tile.Center.Y - boxSize / 2, boxSize, boxSize);
        context.DrawRectangle(null, StoragePen, box, 3, 3);
    }

    private void DrawOrders(DrawingContext context, double originX, double originY, double cell)
    {
        if (ClientState == null)
        {
            return;
        }

        foreach (var order in ClientState.Orders.Values)
        {
            if (order.OrderStatus == OrderStatus.Delivered)
            {
                continue;
            }

            var (solid, soft) = GetStatusBrushes(order.OrderStatus);
            var center = new Point(
                originX + (order.DeliveryPoint.X + 0.5) * cell,
                originY + (order.DeliveryPoint.Y + 0.5) * cell);

            context.DrawEllipse(soft, null, center, cell * 0.30, cell * 0.30);
            context.DrawEllipse(solid, null, center, cell * 0.14, cell * 0.14);
        }
    }

    private static (IBrush Solid, IBrush Soft) GetStatusBrushes(OrderStatus status) => status switch
    {
        OrderStatus.PickingUp => (PickingUpBrush, PickingUpSoftBrush),
        OrderStatus.InTransit => (InTransitBrush, InTransitSoftBrush),
        _ => (PendingBrush, PendingSoftBrush),
    };

    private static SolidColorBrush CreateBrush(string hex) => new(Color.Parse(hex));

    public void UpdateSmoothing()
    {
        if (ClientState == null || ClientState.Position == null || ClientState.Angle == null)
        {
            _isInitialized = false;
            return;
        }

        var deltaX = ClientState.Position.X - _x;
        var deltaY = ClientState.Position.Y - _y;
        
        if (!_isInitialized || 
            Math.Sqrt(Math.Abs(deltaX) * Math.Abs(deltaX) + Math.Abs(deltaY) * Math.Abs(deltaY)) > 1.5f)
        {
            _x = ClientState.Position.X;
            _y = ClientState.Position.Y;
            _angle = (float)ClientState.Angle;
            _lastRenderTimestamp = Stopwatch.GetTimestamp();
            _isInitialized = true;
            return;
        }

        var now = Stopwatch.GetTimestamp();
        var dt = Stopwatch.GetElapsedTime(_lastRenderTimestamp, now).TotalSeconds;
        
        _lastRenderTimestamp = now;
        dt = Math.Min(dt, 0.1);
        
        var factor = 1 - Math.Exp(-dt * 15);
        _x += (float)(deltaX * factor);
        _y += (float)(deltaY * factor);
        
        var delta = ClientState.Angle - _angle;
        while (delta > 180)   delta -= 360;
        while (delta <= -180) delta += 360;
        _angle += (float)(delta * factor)!;
    }
}
