using HoverText.Core.Probing;

namespace HoverText.Core.Overlay;

public static class OverlayTextSizing
{
    private const int TargetTextWidth = 688;
    private const double TargetLineCount = 1.45;
    private const double MinimumAdaptiveTextFontSize = 48;
    private const double MaximumAdaptiveTextFontSize = 56;

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

        double adaptiveBaseFontSize = Math.Clamp(
            configuredFontSize,
            MinimumAdaptiveTextFontSize,
            MaximumAdaptiveTextFontSize);
        double estimatedUnits = text.Sum(EstimateCharacterUnits);
        if (estimatedUnits <= 0)
        {
            return adaptiveBaseFontSize;
        }

        double maxUnits = TargetTextWidth / adaptiveBaseFontSize * TargetLineCount;
        if (estimatedUnits <= maxUnits)
        {
            return adaptiveBaseFontSize;
        }

        double scaledFontSize = adaptiveBaseFontSize * maxUnits / estimatedUnits;
        return Math.Max(MinimumAdaptiveTextFontSize, Math.Floor(scaledFontSize));
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
