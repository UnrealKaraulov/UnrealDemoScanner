using System;
using System.Drawing;           // NOTE: Project + Add Reference required
using System.Runtime.InteropServices;
#if !NET6_0_OR_GREATER
using System.Windows.Forms;

public static class NativeConsoleMethods
{
    public static void CenterConsole()
    {
        IntPtr hWin = GetConsoleWindow();
        RECT rc;
        GetWindowRect(hWin, out rc);
        Screen scr = Screen.FromPoint(new Point(rc.left, rc.top));
        int x = scr.WorkingArea.Left + (scr.WorkingArea.Width - (rc.right - rc.left)) / 2;
        int y = scr.WorkingArea.Top + (scr.WorkingArea.Height - (rc.bottom - rc.top)) / 2;
        MoveWindow(hWin, x, y, rc.right - rc.left, rc.bottom - rc.top, false);
    }
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetConsoleWindow();
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT rc);
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool repaint);

    public struct RECT { public int left, top, right, bottom; }
}

#endif