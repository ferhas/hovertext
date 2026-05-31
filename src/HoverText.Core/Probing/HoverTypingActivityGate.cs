namespace HoverText.Core.Probing;

public sealed class HoverTypingActivityGate(TimeSpan activeWindow)
{
    private TimeSpan? lastTypingActivityAt;

    public static HoverTypingActivityGate CreateDefault()
    {
        return new HoverTypingActivityGate(TimeSpan.FromMilliseconds(1200));
    }

    public void RecordTypingActivity(TimeSpan now)
    {
        lastTypingActivityAt = now;
    }

    public bool HasRecentActivity(TimeSpan now)
    {
        return lastTypingActivityAt is not null
            && now - lastTypingActivityAt.Value <= activeWindow;
    }
}
