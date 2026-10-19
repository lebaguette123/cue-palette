namespace Cue.Core.Hotkeys;

[Flags]
public enum HotkeyModifiers
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Win = 8,
}

public readonly record struct HotkeyBinding(HotkeyModifiers Modifiers, int VirtualKey)
{
    public static HotkeyBinding Default => new(HotkeyModifiers.Control | HotkeyModifiers.Alt, 0x20);
}

public interface IHotkeyService : IDisposable
{
    event Action? Pressed;
    
    /// <returns>false if another app already owns the combination.</returns>
    bool Register(HotkeyBinding binding);
    
    void Unregister();
}