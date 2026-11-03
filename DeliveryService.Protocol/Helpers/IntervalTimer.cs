using System.Diagnostics;

namespace DeliveryService.Protocol.Helpers;

public class IntervalTimer(TimeSpan interval, bool isDueOnCreate = true)
{
    private TimeSpan Interval { get; } = interval;
    private readonly Stopwatch _timer = Stopwatch.StartNew();

    public bool IsDue()
    {
        if (_timer.Elapsed < Interval)
        {
            if (!isDueOnCreate) return false;
            
            isDueOnCreate = false;
            return true;
        }
        
        _timer.Restart();
        isDueOnCreate = false;
        return true;
    }
    
}