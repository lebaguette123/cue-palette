using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

namespace Cue.Windows;

public sealed unsafe class TrayIcon : IDisposable
{
    private const uint CallbackMessage = 0x8000 + 1;
    private const uint WM_Null = 0x0000;
    private const uint WM_LBUTTONUP = 0x0202;
    private const uint WM_RBUTTONUP = 0x0205;

    private const uint NIM_ADD = 0, NIM_DELETE = 2;
    private const uint NIF_MESSAGE = 0x1, NIF_ICON = 0x2, NIF_TIP = 0x4;

    private const uint MF_STRING = 0;
    private const uint TPM_RIGHTBUTTON = 0x2, TPM_BOTTOMALIGN = 0x20, TPM_RETURNCMD = 0x100;
    private const int QuitCommandId = 1;

    private const uint IconId = 1;
    private const nuint SubclassId = 2;
    
    private delegate nint SubclassProc(nint hwnd, uint msg, nint wParam, nint lParam, nuint idSubclass, nuint refData);
    
    private readonly nint _hwnd;
    private readonly string _tooltip;
    private readonly SubclassProc _proc;
    private readonly nint _procPtr;
    private readonly nint _icon;
    private readonly bool _ownsIcon;
    private bool _added;
    private bool _disposed;

    public event Action? Clicked;
    
    public event Action? QuitRequested;

    public TrayIcon(nint hwnd, string tooltip)
    {
        _hwnd = hwnd;
        _tooltip = tooltip;
        (_icon, _ownsIcon) = LoadAppIcon();

        _proc = WndProc;
        _procPtr = Marshal.GetFunctionPointerForDelegate(_proc);
        NativeMethods.SetWindowSubclass(_hwnd, _procPtr, SubclassId, 0);

        var data = BuildData();
        _added = NativeMethods.Shell_NotifyIcon(NIM_ADD, ref data);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_added)
        {
            var data = BuildData();
            NativeMethods.Shell_NotifyIcon(NIM_DELETE, ref data);
        }
        
        NativeMethods.RemoveWindowSubclass(_hwnd, _procPtr, SubclassId);
        if (_ownsIcon) NativeMethods.DestroyIcon(_icon);
    }

    private NativeMethods.NOTIFYICONDATAW BuildData()
    {
        var data = new NativeMethods.NOTIFYICONDATAW
        {
            cbSize = (uint)sizeof(NativeMethods.NOTIFYICONDATAW),
            hWnd = _hwnd,
            uID = IconId,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = CallbackMessage,
            hIcon = _icon
        };

        var tip = _tooltip.Length > 127 ? _tooltip[..127] : _tooltip;
        for(var i = 0; i < tip.Length; i++) data.szTip[i] = tip[i];
        return data;
    }

    private static (nint Handle, bool Owned) LoadAppIcon()
    {
        var path = Environment.ProcessPath;
        if (path is not null)
        {
            var icon = NativeMethods.ExtractIconW(0, path, 0);
            if (icon > 1) return (icon, true);
        }
        
        const nint IDI_APPLICATION = 32512;
        return (NativeMethods.LoadIconW(0, IDI_APPLICATION), false);
    }

    private nint WndProc(nint hwnd, uint msg, nint wParam, nint lParam, nuint idSubclass, nuint refData)
    {
        if (msg == CallbackMessage)
        {
            switch ((uint)lParam)
            {
                case WM_LBUTTONUP: Clicked?.Invoke(); break;
                case WM_RBUTTONUP: ShowMenu(); break;
            }

            return 0;
        }
        return NativeMethods.DefSubclassProc(hwnd, msg, wParam, lParam);
    }

    private void ShowMenu()
    {
        var menu = NativeMethods.CreatePopupMenu();
        try
        {
            NativeMethods.AppendMenuW(menu, MF_STRING, QuitCommandId, "Quit Cue");
            NativeMethods.GetCursorPos(out var cursor);

            NativeMethods.SetForegroundWindow(_hwnd);

            var command = NativeMethods.TrackPopupMenu(
                menu, TPM_RETURNCMD | TPM_RIGHTBUTTON | TPM_BOTTOMALIGN, cursor.X, cursor.Y, 0, _hwnd, 0);

            NativeMethods.PostMessageW(_hwnd, WM_Null, 0, 0);

            if (command == QuitCommandId) QuitRequested?.Invoke();
        }
        finally
        {
            NativeMethods.DestroyMenu(menu);
        }
    }
}