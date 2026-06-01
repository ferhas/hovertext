namespace HoverText.Core.Probing;

public static class MagnifierPollingPolicy
{
    public static readonly TimeSpan ActiveInterval = TimeSpan.FromMilliseconds(16);

    public static TimeSpan GetInterval(TimeSpan configuredInterval, bool isMagnifierPressed)
    {
        return isMagnifierPressed && configuredInterval > ActiveInterval
            ? ActiveInterval
            : configuredInterval;
    }
}
