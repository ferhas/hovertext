using HoverText.Core.Probing;
using HoverText.Core.Settings;

namespace HoverText.Core.Overlay;

public static class OverlayTextSizing
{
    private const int TargetTextWidth = 688;
    private const double TargetLineCount = 1.45;

    public static double CalculateFontSize(int configuredFontSize, string text, ProbeDisplayKind displayKind)
    {
        if (displayKind == ProbeDisplayKind.Input)
        {
            return Math.Max(configuredFontSize, 76);
        }

        if (displayKind == ProbeDisplayKind.Magnifier || string.IsNullOrWhiteSpace(text))
        {
            return configuredFontSize;
        }

        double estimatedUnits = text.Sum(EstimateCharacterUnits);
        if (estimatedUnits <= 0)
        {
            return configuredFontSize;
        }

        double maxUnits = TargetTextWidth / configuredFontSize * TargetLineCount;
        if (estimatedUnits <= maxUnits)
        {
            return configuredFontSize;
        }

        double scaledFontSize = configuredFontSize * maxUnits / estimatedUnits;
        return Math.Max(HoverTextSettings.MinimumFontSize, Math.Floor(scaledFontSize));
    }

    private static double EstimateCharacterUnits(char character)
    {
        if (char.IsWhiteSpace(character))
        {
            return 0.35;
        }

        return character <= 0x007f ? 0.56 : 1.0;
    }
}
