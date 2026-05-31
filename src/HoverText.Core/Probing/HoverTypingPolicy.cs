using HoverText.Core.Settings;

namespace HoverText.Core.Probing;

public static class HoverTypingPolicy
{
    public static bool ShouldShowWithoutTrigger(HoverTextSettings settings, ProbeResult result)
    {
        return settings.IsHoverTypingEnabled
            && result.Source != ProbeSource.None
            && result.DisplayKind == ProbeDisplayKind.Input;
    }
}
