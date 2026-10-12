namespace DeliveryService.Protocol.Logger;

[Flags]
public enum LoggerFilters : byte
{
    None = 0,
    ShowTelemetry = 1,
    ShowErrors = 2,
    All = ShowTelemetry | ShowErrors
}
