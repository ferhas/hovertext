using HoverText.App.Input;
using HoverText.Core.Settings;

namespace HoverText.Tests;

[TestClass]
public sealed class KeyboardTriggerReaderTests
{
    [TestMethod]
    public void IsPressed_does_not_treat_generic_alt_state_as_physical_alt()
    {
        var reader = new KeyboardTriggerReader(key => key == 0x12 ? unchecked((short)0x8000) : (short)0);

        Assert.IsFalse(reader.IsPressed(TriggerKey.Alt));
    }

    [TestMethod]
    public void IsPressed_accepts_left_alt()
    {
        var reader = new KeyboardTriggerReader(key => key == 0xA4 ? unchecked((short)0x8000) : (short)0);

        Assert.IsTrue(reader.IsPressed(TriggerKey.Alt));
    }

    [TestMethod]
    public void IsPressed_accepts_right_control()
    {
        var reader = new KeyboardTriggerReader(key => key == 0xA3 ? unchecked((short)0x8000) : (short)0);

        Assert.IsTrue(reader.IsPressed(TriggerKey.Control));
    }

    [TestMethod]
    public void IsEscapePressed_accepts_short_tap_state()
    {
        var reader = new KeyboardTriggerReader(key => key == 0x1B ? (short)0x0001 : (short)0);

        Assert.IsTrue(reader.IsEscapePressed());
    }
}
