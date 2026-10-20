using System.Runtime.InteropServices;
using Cue.Core.Hotkeys;

namespace Cue.Windows;

public sealed class HotkeyService : IHotkeyService
{
    private const uint WM_HOTKEY = 0x0312;
    private const uint WM_NCDESTROY = 0x0082;
    private const uint MOD_NOREPEAT = 0x4000;
    private const int HotkeyId = 1;
    private const nuint SubclassId = 1;

    private delegate nint SubclassProc(nint hwnd, uint msg, nint wParam, nint lParam, nuint idSubclass, nuint refData);

    private readonly nint _hwnd;
    private readonly SubclassProc _proc;
    private readonly nint _procPtr;
    private bool _registered;
    private bool _disposed;

    public event Action? Pressed;

    public HotkeyService(nint hwnd)
    {
        _hwnd = hwnd;
        _proc = WndProc;
        _procPtr = Marshal.GetFunctionPointerForDelegate(_proc);

        if (!NativeMethods.SetWindowSubclass(_hwnd, _procPtr, SubclassId, 0))
        {
            throw new InvalidOperationException("SetWindowSubclass failed.");
        }
    }

    public bool Register(HotkeyBinding binding)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Unregister();
        _registered = NativeMethods.RegisterHotKey(
            _hwnd, HotkeyId, (uint)binding.Modifiers | MOD_NOREPEAT, (uint)binding.VirtualKey);
        return _registered;
    }

    public void Unregister()
    {
        if (!_registered) return;
        NativeMethods.UnregisterHotKey(_hwnd, HotkeyId);
        _registered = false;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Unregister();
        NativeMethods.RemoveWindowSubclass(_hwnd, _procPtr, SubclassId);
    }

    private nint WndProc(nint hwnd, uint msg, nint wParam, nint lParam, nuint idSubclass, nuint refData)
    {
        switch (msg)
        {
            case WM_HOTKEY when wParam == HotkeyId:
                Pressed?.Invoke();
                return 0;
            case WM_NCDESTROY:
                NativeMethods.RemoveWindowSubclass(hwnd, _procPtr, SubclassId);
                break;
        }

        return NativeMethods.DefSubclassProc(hwnd, msg, wParam, lParam);
    }
}